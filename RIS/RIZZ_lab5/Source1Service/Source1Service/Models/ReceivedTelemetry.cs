using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Source1Service.Models
{
    [Table("ReceivedTelemetry")]
    public class ReceivedTelemetryData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SourceIdentifier { get; set; } = null!;

        [Required]
        public string ObjectId { get; set; } = null!;

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public double Value { get; set; }

        public DateTime ReceivedTimestampUtc { get; set; } = DateTime.UtcNow;
    }
}
