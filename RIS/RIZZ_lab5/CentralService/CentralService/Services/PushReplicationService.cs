using Microsoft.EntityFrameworkCore;
using CentralService.Data;
using CentralService.Models;

namespace CentralService.Services
{
    public class PushReplicationService : BackgroundService
    {
        private readonly ILogger<PushReplicationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TimeSpan _interval;
        // Метка времени последней записи из CentralDB, которая была успешно отправлена во все цели
        private DateTime _lastSuccessfullyPushedTimestamp = DateTime.MinValue;

        public PushReplicationService(ILogger<PushReplicationService> logger,
                                  IServiceScopeFactory scopeFactory,
                                  IConfiguration configuration,
                                  IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _interval = TimeSpan.FromSeconds(
                _configuration.GetValue<int>("PushReplicationSettings:IntervalSeconds", 20));
            _logger.LogInformation("[Push Service] Interval set to {Interval}", _interval);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[Push Service] Starting.", nameof(PushReplicationService));

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (OperationCanceledException) { _logger.LogInformation("[Push Service] Cancelled during initial delay."); return; }


            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("[Push Service] Starting push cycle at {Time}", DateTimeOffset.Now);
                DateTime currentCycleMaxTimestamp = DateTime.MinValue;

                try
                {
                    List<TelemetryData> newDataToSend;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CentralDbContext>();

                        // Находим новые данные в CentralDB
                        // Берем данные новее, чем _lastSuccessfullyPushedTimestamp
                        newDataToSend = await dbContext.TelemetryData
                            .Where(t => t.Timestamp > _lastSuccessfullyPushedTimestamp)
                            .OrderBy(t => t.Timestamp)
                            .AsNoTracking()
                            .ToListAsync(stoppingToken);

                        if (newDataToSend.Any())
                        {
                            _logger.LogInformation("[Push Service] Found {Count} new records in Central DB to push (since {LastPushTime}).",
                                newDataToSend.Count, _lastSuccessfullyPushedTimestamp);
                            currentCycleMaxTimestamp = newDataToSend.Max(t => t.Timestamp);
                        }
                        else
                        {
                            _logger.LogInformation("[Push Service] No new records found in Central DB to push (since {LastPushTime}).", _lastSuccessfullyPushedTimestamp);
                            await Task.Delay(_interval, stoppingToken);
                            continue;
                        }
                    }

                    // Отправляем найденные данные каждому получателю
                    var targets = _configuration.GetSection("PushTargets").GetChildren().ToList();
                    if (!targets.Any())
                    {
                        _logger.LogWarning("[Push Service] No push targets configured in appsettings.json under PushTargets section.");
                        await Task.Delay(_interval, stoppingToken);
                        continue;
                    }

                    bool allPushesSuccessful = true;

                    var httpClient = _httpClientFactory.CreateClient();

                    foreach (var target in targets)
                    {
                        string targetName = target.Key;
                        string targetUrl = target.Value;

                        if (string.IsNullOrEmpty(targetUrl))
                        {
                            _logger.LogWarning("[Push Service] Push target URL for '{TargetName}' is empty, skipping.", targetName);
                            continue;
                        }

                        bool pushSuccess = await PushDataToTargetAsync(httpClient, targetName, targetUrl, newDataToSend, stoppingToken);
                        if (!pushSuccess)
                        {
                            allPushesSuccessful = false;
                            _logger.LogWarning("[Push Service] Push to {TargetName} failed. Data will be retried.", targetName);
                        }
                    }

                    // Обновляем метку _lastSuccessfullyPushedTimestamp
                    if (allPushesSuccessful && newDataToSend.Any())
                    {
                        _lastSuccessfullyPushedTimestamp = currentCycleMaxTimestamp;
                        _logger.LogInformation("[Push Service] Successfully pushed data to all targets. Last pushed timestamp updated to {Timestamp}", _lastSuccessfullyPushedTimestamp);
                    }
                    else if (!allPushesSuccessful && newDataToSend.Any())
                    {
                        _logger.LogWarning("[Push Service] Not all pushes successful. Last pushed timestamp *not* updated.");
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("[Push Service] Stopping requested.", nameof(PushReplicationService));
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Push Service] Unhandled error during the push cycle.");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }

                _logger.LogInformation("[Push Service] Push cycle finished. Waiting for {Interval}.", _interval);
                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("[Push Service] Stopping during delay.", nameof(PushReplicationService));
                    break;
                }
            }

            _logger.LogInformation("[Push Service] Stopped.", nameof(PushReplicationService));
        }

        private async Task<bool> PushDataToTargetAsync(HttpClient httpClient, string targetName, string targetUrl, List<TelemetryData> data, CancellationToken stoppingToken)
        {
            _logger.LogInformation("[Push Service] Attempting to push {Count} records to {TargetName} ({TargetUrl})", data.Count, targetName, targetUrl);
            try
            {
                HttpResponseMessage response = await httpClient.PostAsJsonAsync(targetUrl, data, stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[Push Service] Successfully pushed data to {TargetName}. Status: {StatusCode}", targetName, response.StatusCode);
                    return true;
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync(stoppingToken);
                    _logger.LogError("[Push Service] Failed to push data to {TargetName}. Status: {StatusCode} ({Reason}). URL: {TargetUrl}. Response: {ErrorContent}",
                                     targetName, response.StatusCode, response.ReasonPhrase, targetUrl, errorContent);
                    return false;
                }
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[Push Service] HTTP request error pushing to {TargetName} ({TargetUrl}). Check connectivity/URL.", targetName, targetUrl);
                return false;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[Push Service] Push operation to {TargetName} was cancelled.", targetName);
                return false;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "[Push Service] Unexpected error pushing data to {TargetName} ({TargetUrl})", targetName, targetUrl);
                return false;
            }
        }
    }
}
