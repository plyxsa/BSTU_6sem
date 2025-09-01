using CentralService.Data;
using CentralService.Models; 
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CentralService.Services
{
    public class ReplicationService : BackgroundService
    {
        private readonly ILogger<ReplicationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TimeSpan _interval;
        private readonly string[] _sourceIdentifiers = { "Source1", "Source2" };

        public ReplicationService(ILogger<ReplicationService> logger,
                                  IServiceScopeFactory scopeFactory,
                                  IConfiguration configuration,
                                  IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _interval = TimeSpan.FromSeconds(
                _configuration.GetValue<int>("ReplicationSettings:IntervalSeconds", 15));
            _logger.LogInformation("Replication interval set to {Interval}", _interval);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{ServiceName} is starting.", nameof(ReplicationService));
            try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting replication cycle at {Time}", DateTimeOffset.Now);
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var centralDbContext = scope.ServiceProvider.GetRequiredService<CentralDbContext>();

                        foreach (var sourceId in _sourceIdentifiers)
                        {
                            await ReplicateFromSourceAsync(sourceId, centralDbContext, stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("{ServiceName} cycle cancelled during processing.", nameof(ReplicationService));
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the main replication cycle loop.");
                }

                _logger.LogInformation("Replication cycle finished. Waiting for {Interval}.", _interval);
                try { await Task.Delay(_interval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
            _logger.LogInformation("{ServiceName} has stopped.", nameof(ReplicationService));
        }

        private async Task ReplicateFromSourceAsync(string sourceIdentifier,
                                            CentralDbContext centralDbContext,
                                            CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Replicating data for source: {SourceId}", sourceIdentifier);
                DateTime lastReplicatedTimestamp = await centralDbContext.TelemetryData
                   .Where(t => t.SourceIdentifier == sourceIdentifier)
                   .OrderByDescending(t => t.Timestamp)
                   .Select(t => t.Timestamp)
                   .FirstOrDefaultAsync(stoppingToken);
                _logger.LogDebug("Last replicated timestamp for {SourceId} is {Timestamp}", sourceIdentifier, lastReplicatedTimestamp);

                string? baseUrl = _configuration[$"ReplicationSettings:SourceServices:{sourceIdentifier}:BaseUrl"];
                string? path = _configuration[$"ReplicationSettings:SourceServices:{sourceIdentifier}:TelemetryPath"];
                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(path)) { return; }

                string formattedTimestamp = lastReplicatedTimestamp.ToString("O", CultureInfo.InvariantCulture);
                string requestUrl = $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}?since={Uri.EscapeDataString(formattedTimestamp)}";
                _logger.LogDebug("Requesting data from URL: {Url}", requestUrl);

                List<TelemetryData> newTelemetryDataForDb = null;

                HttpClient client = _httpClientFactory.CreateClient();
                HttpResponseMessage response = await client.GetAsync(requestUrl, stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    var receivedData = await response.Content.ReadFromJsonAsync<List<TelemetryData>>(cancellationToken: stoppingToken);
                    _logger.LogInformation("Received {Count} records from {SourceId} API.", receivedData?.Count ?? 0, sourceIdentifier);

                    if (receivedData != null && receivedData.Any())
                    {
                        newTelemetryDataForDb = receivedData.Select(sourceData => new TelemetryData
                        {
                            SourceIdentifier = sourceData.SourceIdentifier,
                            ObjectId = sourceData.ObjectId,
                            Timestamp = sourceData.Timestamp,
                            Value = sourceData.Value
                        }).ToList();
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync(stoppingToken);
                    _logger.LogWarning("Failed to get data from {SourceId} API. Status: {StatusCode} {ReasonPhrase}. URL: {Url}. Content: {ErrorContent}",
                                      sourceIdentifier, (int)response.StatusCode, response.ReasonPhrase, requestUrl, errorContent);
                    return;
                }

                if (newTelemetryDataForDb != null && newTelemetryDataForDb.Any())
                {
                    await ProcessNewData(newTelemetryDataForDb, centralDbContext, sourceIdentifier, stoppingToken);
                }
                else if (newTelemetryDataForDb != null)
                {
                    _logger.LogInformation("No new records found for {SourceId} after processing API response.", sourceIdentifier);
                }

            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Source {SourceId} seems unavailable. Failed to connect or communicate. Message: {ErrorMessage}",
                                 sourceIdentifier, ex.Message);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Replication for {SourceId} cancelled.", sourceIdentifier);
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to deserialize JSON response from {SourceId}.", sourceIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while replicating data for source: {SourceId}", sourceIdentifier);
            }
        }

        private async Task ProcessNewData(List<TelemetryData> newData,
                                          CentralDbContext centralDbContext,
                                          string sourceId,
                                          CancellationToken stoppingToken)
        {
            if (newData.Any())
            {
                _logger.LogInformation("Found {Count} new records from API for {SourceId} to insert/update.", newData.Count, sourceId); // Уточнил сообщение
                await centralDbContext.TelemetryData.AddRangeAsync(newData, stoppingToken);
                int recordsSaved = await centralDbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Successfully saved {Count} new records from {SourceId} to Central DB.", recordsSaved, sourceId);
            }
        }
    }
}