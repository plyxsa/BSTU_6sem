using Microsoft.EntityFrameworkCore;
using Source1Service.Models;

namespace Source1Service.Data
{
    public class Source1DbContext : DbContext
    {
        public Source1DbContext(DbContextOptions<Source1DbContext> options) : base(options) { }

        public DbSet<TelemetryData> TelemetryData { get; set; } = null!;
        public DbSet<ReceivedTelemetryData> ReceivedTelemetry { get; set; } = null!;
    }
}
