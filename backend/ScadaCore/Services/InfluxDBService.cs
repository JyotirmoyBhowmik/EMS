using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using ScadaCore.Models;

namespace ScadaCore.Services;

/// <summary>
/// Service for interacting with InfluxDB time-series database
/// Handles high-speed writes and queries for tag historical data
/// </summary>
public class InfluxDBService : IDisposable
{
    private readonly InfluxDBClient _client;
    private readonly string _bucket;
    private readonly string _org;

    public InfluxDBService(string url, string token, string org, string bucket)
    {
        _client = new InfluxDBClient(url, token);
        _org = org;
        _bucket = bucket;
    }

    /// <summary>
    /// Write a single tag value to InfluxDB
    /// </summary>
    public async Task WriteTagValueAsync(TagValue tagValue)
    {
        using var writeApi = _client.GetWriteApiAsync();
        
        var point = PointData
            .Measurement("tag_values")
            .Tag("tag_name", tagValue.TagName)
            .Tag("quality", tagValue.Quality ?? "Good")
            .Field("value", tagValue.Value)
            .Timestamp(tagValue.Timestamp, WritePrecision.Ms);

        await writeApi.WritePointAsync(point, _bucket, _org);
    }

    /// <summary>
    /// Write multiple tag values in batch (optimized for high throughput)
    /// </summary>
    public async Task WriteBatchAsync(IEnumerable<TagValue> tagValues)
    {
        using var writeApi = _client.GetWriteApiAsync();
        
        var points = tagValues.Select(tv => PointData
            .Measurement("tag_values")
            .Tag("tag_name", tv.TagName)
            .Tag("quality", tv.Quality ?? "Good")
            .Field("value", tv.Value)
            .Timestamp(tv.Timestamp, WritePrecision.Ms));

        await writeApi.WritePointsAsync(points, _bucket, _org);
    }

    /// <summary>
    /// Query historical data for a specific tag
    /// </summary>
    public async Task<List<TagValue>> QueryTagHistoryAsync(string tagName, DateTime start, DateTime end)
    {
        var flux = $@"
            from(bucket: ""{_bucket}"")
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})
                |> filter(fn: (r) => r._measurement == ""tag_values"")
                |> filter(fn: (r) => r.tag_name == ""{tagName}"")
                |> filter(fn: (r) => r._field == ""value"")
        ";

        var queryApi = _client.GetQueryApi();
        var tables = await queryApi.QueryAsync(flux, _org);

        var results = new List<TagValue>();
        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                results.Add(new TagValue
                {
                    TagName = tagName,
                    Value = record.GetValue(),
                    Timestamp = record.GetTime().GetValueOrDefault().ToDateTimeUtc(),
                    Quality = record.GetValueByKey("quality")?.ToString() ?? "Good"
                });
            }
        }

        return results;
    }

    /// <summary>
    /// Query latest value for a tag
    /// </summary>
    public async Task<TagValue?> QueryLatestValueAsync(string tagName)
    {
        var flux = $@"
            from(bucket: ""{_bucket}"")
                |> range(start: -1h)
                |> filter(fn: (r) => r._measurement == ""tag_values"")
                |> filter(fn: (r) => r.tag_name == ""{tagName}"")
                |> filter(fn: (r) => r._field == ""value"")
                |> last()
        ";

        var queryApi = _client.GetQueryApi();
        var tables = await queryApi.QueryAsync(flux, _org);

        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                return new TagValue
                {
                    TagName = tagName,
                    Value = record.GetValue(),
                    Timestamp = record.GetTime().GetValueOrDefault().ToDateTimeUtc(),
                    Quality = record.GetValueByKey("quality")?.ToString() ?? "Good"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Query aggregated data (e.g., average, min, max)
    /// </summary>
    public async Task<Dictionary<string, object>> QueryAggregatesAsync(
        string tagName, 
        DateTime start, 
        DateTime end,
        string window = "1m")
    {
        var flux = $@"
            from(bucket: ""{_bucket}"")
                |> range(start: {start:yyyy-MM-ddTHH:mm:ssZ}, stop: {end:yyyy-MM-ddTHH:mm:ssZ})
                |> filter(fn: (r) => r._measurement == ""tag_values"")
                |> filter(fn: (r) => r.tag_name == ""{tagName}"")
                |> filter(fn: (r) => r._field == ""value"")
                |> aggregateWindow(every: {window}, fn: mean)
        ";

        var queryApi = _client.GetQueryApi();
        var tables = await queryApi.QueryAsync(flux, _org);

        var results = new Dictionary<string, object>();
        var values = new List<double>();

        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                if (double.TryParse(record.GetValue()?.ToString(), out var val))
                {
                    values.Add(val);
                }
            }
        }

        if (values.Any())
        {
            results["average"] = values.Average();
            results["min"] = values.Min();
            results["max"] = values.Max();
            results["count"] = values.Count;
        }

        return results;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
