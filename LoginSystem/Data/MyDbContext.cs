using LoginSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginSystem.Data
{
    public class MyDbContext:DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
    }
}
