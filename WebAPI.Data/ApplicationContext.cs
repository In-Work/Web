using Microsoft.EntityFrameworkCore;
using Web.Data.Entities;

namespace Web.Data
{
    public class ApplicationContext : DbContext 
    {
        public ApplicationContext(DbContextOptions options) : base(options)
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<Item> Items { get; set; }
    }

}
