using Prometheus;

namespace DataAcquisition.Services;

/// <summary>
/// Background worker that consumes tag data from RabbitMQ and writes to InfluxDB
/// Implements high-throughput data pipeline with error handling
/// </summary>
public class DataIngestionWorker : BackgroundService
{
    private readonly RabbitMQService _rabbitMQ;
    private readonly InfluxDBWriterService _influxWriter;
    private readonly StoreAndForwardService _storeAndForward;
    private readonly ILogger<DataIngestionWorker> _logger;

    // Prometheus metrics
    private static readonly Counter _messagesProcessed = Metrics.CreateCounter(
        "scada_messages_processed_total",
        "Total number of messages processed from RabbitMQ");

    private static readonly Counter _messagesFailed = Metrics.CreateCounter(
        "scada_messages_failed_total",
        "Total number of messages that failed processing");

    private static readonly Gauge _processingRate = Metrics.CreateGauge(
        "scada_tag_scan_rate",
        "Current tag processing rate (tags/sec)");

    private DateTime _lastRateCalculation = DateTime.UtcNow;
    private long _tagsProcessedSinceLastCalc = 0;

    public DataIngestionWorker(
        RabbitMQService rabbitMQ,
        InfluxDBWriterService influxWriter,
        StoreAndForwardService storeAndForward,
        ILogger<DataIngestionWorker> logger)
    {
        _rabbitMQ = rabbitMQ;
        _influxWriter = influxWriter;
        _storeAndForward = storeAndForward;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Ingestion Worker starting...");

        // Start consuming from RabbitMQ
        _rabbitMQ.StartConsuming(ProcessMessageAsync);

        // Calculate processing rate periodically
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (!stoppingToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(stoppingToken);
            UpdateProcessingRate();
        }
    }

    /// <summary>
    /// Process a single tag data message
    /// </summary>
    private async Task<bool> ProcessMessageAsync(TagDataMessage message)
    {
        try
        {
            // Write to InfluxDB
            await _influxWriter.QueuePointAsync(message);

            _messagesProcessed.Inc();
            Interlocked.Increment(ref _tagsProcessedSinceLastCalc);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message for tag {TagName}", message.TagName);
            _messagesFailed.Inc();

            // Store for later replay
            try
            {
                await _storeAndForward.BufferAsync(message);
                _logger.LogWarning("Message buffered for tag {TagName}", message.TagName);
            }
            catch (Exception bufferEx)
            {
                _logger.LogError(bufferEx, "Failed to buffer message for tag {TagName}", message.TagName);
            }

            return false;
        }
    }

    /// <summary>
    /// Calculate and update the current processing rate
    /// </summary>
    private void UpdateProcessingRate()
    {
        var now = DateTime.UtcNow;
        var elapsed = (now - _lastRateCalculation).TotalSeconds;
        
        if (elapsed > 0)
        {
            var rate = _tagsProcessedSinceLastCalc / elapsed;
            _processingRate.Set(rate);

            if (rate > 0)
            {
                _logger.LogInformation("Processing rate: {Rate:N0} tags/sec", rate);
            }
        }

        _lastRateCalculation = now;
        Interlocked.Exchange(ref _tagsProcessedSinceLastCalc, 0);
    }
}
