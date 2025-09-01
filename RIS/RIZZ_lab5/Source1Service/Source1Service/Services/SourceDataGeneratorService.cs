using Source1Service.Data;
using Source1Service.Models;

namespace Source1Service.Services
{
    public class SourceDataGeneratorService : BackgroundService
    {
        private readonly ILogger<SourceDataGeneratorService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Для получения DbContext в фоновой службе
        private readonly Random _random = new Random();
        private const string SourceIdentifier = "Source1"; // Идентификатор этого источника
        private const int NumberOfObjects = 10; // Количество объектов
        private const int DelaySeconds = 20; // Пауза между генерацией пачек данных

        public SourceDataGeneratorService(ILogger<SourceDataGeneratorService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{ServiceName} is starting.", nameof(SourceDataGeneratorService));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<Source1DbContext>();

                        var timestamp = DateTime.UtcNow;

                        _logger.LogInformation("Generating data for {Source} at {Timestamp}", SourceIdentifier, timestamp);

                        for (int i = 1; i <= NumberOfObjects; i++)
                        {
                            var telemetryEntry = new TelemetryData
                            {
                                SourceIdentifier = SourceIdentifier,
                                ObjectId = $"Meter_{i:D2}",
                                Timestamp = timestamp,
                                // Генерируем случайное значение (номер 8)
                                Value = _random.NextDouble() * (8 - 1) + 1
                            };
                            await dbContext.TelemetryData.AddAsync(telemetryEntry, stoppingToken);
                        }

                        int recordsSaved = await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Saved {Count} new telemetry records for {Source}.", recordsSaved, SourceIdentifier);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("{ServiceName} is stopping.", nameof(SourceDataGeneratorService));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in {ServiceName}.", nameof(SourceDataGeneratorService));
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(DelaySeconds), stoppingToken);
            }
        }
    }
}
