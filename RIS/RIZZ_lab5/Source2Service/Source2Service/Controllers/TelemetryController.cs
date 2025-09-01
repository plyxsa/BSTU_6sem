using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Source2Service.Data;
using Source2Service.Models;

namespace Source2Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private readonly Source2DbContext _context;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(Source2DbContext context, ILogger<TelemetryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/telemetry?since=2023-10-27T10:20:30.123Z
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TelemetryData>>> GetTelemetryData(
            [FromQuery] DateTime? since = null)
        {
            DateTime filterTime = since ?? DateTime.MinValue;

            _logger.LogInformation("API request received for telemetry data since {FilterTime}", filterTime);

            try
            {
                var telemetryData = await _context.TelemetryData
                    .Where(t => t.Timestamp > filterTime)
                    .OrderBy(t => t.Timestamp)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("Returning {Count} telemetry records.", telemetryData.Count);
                return Ok(telemetryData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching telemetry data since {FilterTime}", filterTime);
                return StatusCode(500, "Internal server error retrieving telemetry data.");
            }
        }
    }
}
