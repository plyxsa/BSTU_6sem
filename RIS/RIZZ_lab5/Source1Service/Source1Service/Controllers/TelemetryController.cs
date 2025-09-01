using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Source1Service.Data;
using Source1Service.Models;

namespace Source1Service.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private readonly Source1DbContext _context;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(Source1DbContext context, ILogger<TelemetryController> logger)
        {
            _context = context;
            _logger = logger;
        }

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
