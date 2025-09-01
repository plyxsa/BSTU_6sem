using Microsoft.EntityFrameworkCore;
using CentralService.Models;

namespace CentralService.Data
{
    public class CentralDbContext : DbContext
    {
        public CentralDbContext(DbContextOptions<CentralDbContext> options) : base(options) { }

        public DbSet<TelemetryData> TelemetryData { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TelemetryData>()
                .HasIndex(t => new { t.SourceIdentifier, t.Timestamp });
        }
    }
}
