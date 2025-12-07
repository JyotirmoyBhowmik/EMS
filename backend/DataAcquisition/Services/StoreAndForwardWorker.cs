namespace DataAcquisition.Services;

/// <summary>
/// Background worker that replays buffered data when connectivity is restored
/// Runs periodically to check for buffered messages and attempt to flush them
/// </summary>
public class StoreAndForwardWorker : BackgroundService
{
    private readonly StoreAndForwardService _storeAndForward;
    private readonly InfluxDBWriterService _influxWriter;
    private readonly ILogger<StoreAndForwardWorker> _logger;
    private const int REPLAY_INTERVAL_SECONDS = 30;
    private const int REPLAY_BATCH_SIZE = 1000;

    public StoreAndForwardWorker(
        StoreAndForwardService storeAndForward,
        InfluxDBWriterService influxWriter,
        ILogger<StoreAndForwardWorker> logger)
    {
        _storeAndForward = storeAndForward;
        _influxWriter = influxWriter;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Store-and-Forward Worker starting...");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(REPLAY_INTERVAL_SECONDS));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await ReplayBufferedDataAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Store-and-Forward worker");
            }
        }
    }

    private async Task ReplayBufferedDataAsync()
    {
        var bufferedCount = await _storeAndForward.GetBufferedCountAsync();
        
        if (bufferedCount == 0)
            return;

        _logger.LogInformation("Attempting to replay {Count} buffered messages", bufferedCount);

        try
        {
            var messages = await _storeAndForward.GetBufferedMessagesAsync(REPLAY_BATCH_SIZE);
            
            if (messages.Count == 0)
                return;

            // Attempt to write to InfluxDB
            foreach (var message in messages)
            {
                await _influxWriter.QueuePointAsync(message);
            }

            // If successful, delete the buffered messages
            await _storeAndForward.DeleteBufferedAsync(messages.Count);
            
            _logger.LogInformation("Successfully replayed {Count} buffered messages", messages.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to replay buffered data, will retry later");
        }
    }
}
