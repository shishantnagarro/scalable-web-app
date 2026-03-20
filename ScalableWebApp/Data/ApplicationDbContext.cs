using Microsoft.EntityFrameworkCore;
using ScalableWebApp.Models;

namespace ScalableWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ApplicationLog> ApplicationLogs { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.UserId).HasMaxLength(50);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
            });
        }
    }
}
