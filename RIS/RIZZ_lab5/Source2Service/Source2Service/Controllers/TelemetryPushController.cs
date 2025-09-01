using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Source2Service.Data;
using Source2Service.Models;

namespace Source2Service.Controllers
{
    [Route("api/telemetry")]
    [ApiController]
    public class TelemetryPushController : ControllerBase
    {
        private readonly Source2DbContext _context;
        private readonly ILogger<TelemetryPushController> _logger;

        public TelemetryPushController(Source2DbContext context, ILogger<TelemetryPushController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Метод для приема данных через HTTP POST
        // Маршрут будет /api/telemetry/push
        [HttpPost("push")]
        public async Task<IActionResult> ReceivePushData([FromBody] List<TelemetryData> receivedData)
        {
            if (receivedData == null || !receivedData.Any())
            {
                _logger.LogWarning("Received empty or null data package via push.");
                return BadRequest("Data package cannot be null or empty.");
            }

            _logger.LogInformation("Received {Count} telemetry records via push.", receivedData.Count);

            try
            {
                // 1. Преобразуем полученные TelemetryData в ReceivedTelemetryData
                var receivedEntities = receivedData.Select(d => new ReceivedTelemetryData
                {
                    SourceIdentifier = d.SourceIdentifier,
                    ObjectId = d.ObjectId,
                    Timestamp = d.Timestamp,
                    Value = d.Value,
                    ReceivedTimestampUtc = DateTime.UtcNow // Фиксируем время получения
                }).ToList();

                // 2. Добавляем преобразованные данные в DbSet НОВОЙ таблицы
                await _context.ReceivedTelemetry.AddRangeAsync(receivedEntities);

                // 3. Сохраняем изменения в БД
                int savedCount = await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved {Count} received records to ReceivedTelemetry table.", receivedEntities.Count);
                return Ok($"Successfully processed and saved {receivedEntities.Count} records.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error saving received telemetry data.");
                return StatusCode(500, $"A database error occurred while saving data: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing received telemetry data.");
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
