using Microsoft.EntityFrameworkCore;
using SweetShop.API.Models;

namespace SweetShop.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Sweet> Sweets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // for Price column  decimal precision set
            modelBuilder.Entity<Sweet>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
