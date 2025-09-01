using Microsoft.EntityFrameworkCore;
using Source2Service.Models;

namespace Source2Service.Data
{
    public class Source2DbContext : DbContext
    {
        public Source2DbContext(DbContextOptions<Source2DbContext> options) : base(options) { }

        public DbSet<TelemetryData> TelemetryData { get; set; } = null!;
        public DbSet<ReceivedTelemetryData> ReceivedTelemetry { get; set; } = null!;
    }
}