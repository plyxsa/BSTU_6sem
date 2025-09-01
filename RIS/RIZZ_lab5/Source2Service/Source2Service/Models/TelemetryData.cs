using System.ComponentModel.DataAnnotations;

namespace Source2Service.Models
{
    public class TelemetryData
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
    }
}