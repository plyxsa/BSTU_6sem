using Source2Service.Data;
using Source2Service.Models;

namespace Source1Service.Services
{
    public class SourceDataGeneratorService : BackgroundService
    {
        private readonly ILogger<SourceDataGeneratorService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Для получения DbContext в фоновой службе
        private readonly Random _random = new Random();
        private const string SourceIdentifier = "Source2"; // Идентификатор этого источника
        private const int NumberOfObjects = 10; // Количество объектов/счетчиков по заданию
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
                    using (var scope = _scopeFactory.CreateScope()) // Создаем scope для DbContext
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<Source2DbContext>();

                        var timestamp = DateTime.UtcNow; // Единое время для пачки данных

                        _logger.LogInformation("Generating data for {Source} at {Timestamp}", SourceIdentifier, timestamp);

                        for (int i = 1; i <= NumberOfObjects; i++)
                        {
                            var telemetryEntry = new TelemetryData
                            {
                                SourceIdentifier = SourceIdentifier,
                                ObjectId = $"Meter_{i:D2}", // Форматируем ID объекта (Meter_01, Meter_02...)
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
                    // Ожидаемо при остановке сервиса
                    _logger.LogInformation("{ServiceName} is stopping.", nameof(SourceDataGeneratorService));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in {ServiceName}.", nameof(SourceDataGeneratorService));
                    // Добавим небольшую паузу при ошибке, чтобы не спамить лог
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }

                // Ждем перед следующей генерацией
                await Task.Delay(TimeSpan.FromSeconds(DelaySeconds), stoppingToken);
            }
        }
    }
}
