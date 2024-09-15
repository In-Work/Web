using Microsoft.EntityFrameworkCore;
using Web.Data.Entities;

namespace Web.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        // Другие DbSet...

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
    }
}