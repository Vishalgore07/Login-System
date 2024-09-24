using LoginSystem.Data;
using LoginSystem.DTOs;
using Microsoft.AspNetCore.Mvc;
using LoginSystem.Models;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Authorization;

namespace LoginSystem.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly MyDbContext _db;
        private readonly IConfiguration _configuration;

        public UsersController(MyDbContext db,IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration(UserDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var objUser = _db.Users.FirstOrDefault(x => x.Email == userDTO.Email);
            if (objUser == null)
            {
                _db.Users.Add(new User
                {
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Email = userDTO.Email,
                    Password = userDTO.Password
                });
                _db.SaveChanges();
                return Ok("User registered successfully");
            }
            else
            {
                return BadRequest("User already exists with the same email address");
            }
        }

        [HttpPost]
        public IActionResult Login(LoginDTO loginDTO)
        {
            var user = _db.Users.FirstOrDefault(x => x.Email == loginDTO.Email && x.Password == loginDTO.Password);
            if(user != null)
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub,_configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    new Claim("UserId",user.UserId.ToString()),
                    new Claim("Email",user.Email.ToString())

                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(60),
                    signingCredentials: signIn);

                string tokenValue=new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new { Token = tokenValue, User = user });
                //return Ok(user);
            }
            return NoContent();
        }

        
        [HttpGet]
        [Route("GetUsers")]

        public IActionResult GetUsers()
        {
            return Ok(_db.Users.ToList());
        }

        [Authorize]
        [HttpGet]
        [Route("GetUserById")]
        public IActionResult GetUserById(int id)
        {
            var user = _db.Users.FirstOrDefault(x => x.UserId == id);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NoContent();
            }
        }
    }
}
