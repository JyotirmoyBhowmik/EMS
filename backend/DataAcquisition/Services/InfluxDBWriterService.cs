using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Threading.Channels;
using Prometheus;

namespace DataAcquisition.Services;

/// <summary>
/// High-performance InfluxDB writer service
/// Uses System.Threading.Channels for lock-free, high-throughput buffering
/// Target: 100k+ points per second
/// </summary>
public class InfluxDBWriterService : IDisposable
{
    private readonly InfluxDBClient _client;
    private readonly string _bucket;
    private readonly string _org;
    private readonly Channel<PointData> _channel;
    private readonly ILogger<InfluxDBWriterService> _logger;

    // Prometheus metrics
    private static readonly Counter _pointsWritten = Metrics.CreateCounter(
        "scada_influxdb_points_written_total",
        "Total number of data points written to InfluxDB");

    private static readonly Histogram _writeDuration = Metrics.CreateHistogram(
        "scada_influxdb_write_duration_seconds",
        "Duration of InfluxDB write operations");

    private static readonly Gauge _queueDepth = Metrics.CreateGauge(
        "scada_influxdb_queue_depth",
        "Current depth of the InfluxDB write queue");

    private const int BATCH_SIZE = 5000;
    private const int FLUSH_INTERVAL_MS = 1000;

    public InfluxDBWriterService(ILogger<InfluxDBWriterService> logger)
    {
        _logger = logger;

        var url = Environment.GetEnvironmentVariable("INFLUXDB_URL") ?? "http://localhost:8086";
        var token = Environment.GetEnvironmentVariable("INFLUXDB_TOKEN") ?? "scada-token-change-in-production";
        _org = Environment.GetEnvironmentVariable("INFLUXDB_ORG") ?? "scada-org";
        _bucket = Environment.GetEnvironmentVariable("INFLUXDB_BUCKET") ?? "scada-data";

        _client = new InfluxDBClient(url, token);

        // Create unbounded channel for maximum throughput
        var options = new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true
        };
        _channel = Channel.CreateUnbounded<PointData>(options);

        // Start batch writer background task
        _ = Task.Run(BatchWriterAsync);

        _logger.LogInformation("InfluxDB Writer initialized (batch size: {BatchSize})", BATCH_SIZE);
    }

    /// <summary>
    /// Queue a point for writing (non-blocking)
    /// </summary>
    public async ValueTask QueuePointAsync(TagDataMessage tagData)
    {
        var point = PointData
            .Measurement("tag_values")
            .Tag("tag_name", tagData.TagName)
            .Tag("quality", tagData.Quality)
            .Tag("site", tagData.Site ?? "unknown")
            .Tag("device", tagData.Device ?? "unknown")
            .Field("value", tagData.Value)
            .Timestamp(tagData.Timestamp, WritePrecision.Ms);

        await _channel.Writer.WriteAsync(point);
        _queueDepth.Inc();
    }

    /// <summary>
    /// Background batch writer - continuously flushes points to InfluxDB
    /// </summary>
    private async Task BatchWriterAsync()
    {
        var writeApi = _client.GetWriteApiAsync();
        var batch = new List<PointData>(BATCH_SIZE);
        var lastFlushTime = DateTime.UtcNow;

        await foreach (var point in _channel.Reader.ReadAllAsync())
        {
            batch.Add(point);
            _queueDepth.Dec();

            // Flush when batch is full OR flush interval has elapsed
            var shouldFlush = batch.Count >= BATCH_SIZE ||
                            (DateTime.UtcNow - lastFlushTime).TotalMilliseconds >= FLUSH_INTERVAL_MS;

            if (shouldFlush && batch.Count > 0)
            {
                try
                {
                    using (_writeDuration.NewTimer())
                    {
                        await writeApi.WritePointsAsync(batch, _bucket, _org);
                    }

                    _pointsWritten.Inc(batch.Count);
                    _logger.LogDebug("Wrote {Count} points to InfluxDB", batch.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing batch to InfluxDB");
                    // TODO: Implement retry logic or forward to dead-letter queue
                }
                finally
                {
                    batch.Clear();
                    lastFlushTime = DateTime.UtcNow;
                }
            }
        }
    }

    public void Dispose()
    {
        _channel.Writer.Complete();
        _client?.Dispose();
    }
}
