using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;

namespace AnalyticsService.Services;

public class ClickHouseService
{
    private readonly ILogger<ClickHouseService> _logger;
    private readonly string _connectionString;

    public ClickHouseService(ILogger<ClickHouseService> logger)
    {
        _logger = logger;
        
        var host = Environment.GetEnvironmentVariable("CLICKHOUSE_HOST") ?? "clickhouse";
        var port = Environment.GetEnvironmentVariable("CLICKHOUSE_PORT") ?? "9000";
        var database = Environment.GetEnvironmentVariable("CLICKHOUSE_DB") ?? "scada_analytics";
        var user = Environment.GetEnvironmentVariable("CLICKHOUSE_USER") ?? "scada";
        var password = Environment.GetEnvironmentVariable("CLICKHOUSE_PASSWORD") ?? "clickhouse123";

        _connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";
    }

    public async Task InsertTagData(List<TagAnalytics> data)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO tag_analytics (tag_name, timestamp, value, quality, site, device, data_type) 
            VALUES @bulk";

        command.Parameters.Add(new ClickHouseParameter
        {
            ParameterName = "bulk",
            Value = data.Select(d => new object[]
            {
                d.TagName,
                d.Timestamp,
                d.Value,
                d.Quality,
                d.Site,
                d.Device,
                d.DataType
            })
        });

        await command.ExecuteNonQueryAsync();
        _logger.LogInformation("Inserted {Count} records to ClickHouse", data.Count);
    }

    public async Task<List<HourlyStats>> GetHourlyStats(string tagName, DateTime start, DateTime end)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                tag_name,
                timestamp,
                avg_value,
                min_value,
                max_value,
                sample_count
            FROM tag_hourly_stats
            WHERE tag_name = {tagName:String}
              AND timestamp >= {start:DateTime}
              AND timestamp <= {end:DateTime}
            ORDER BY timestamp";

        command.Parameters.Add(new ClickHouseParameter { ParameterName = "tagName", Value = tagName });
        command.Parameters.Add(new ClickHouseParameter { ParameterName = "start", Value = start });
        command.Parameters.Add(new ClickHouseParameter { ParameterName = "end", Value = end });

        var results = new List<HourlyStats>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            results.Add(new HourlyStats
            {
                TagName = reader.GetString(0),
                Timestamp = reader.GetDateTime(1),
                AvgValue = reader.GetDouble(2),
                MinValue = reader.GetDouble(3),
                MaxValue = reader.GetDouble(4),
                SampleCount = reader.GetInt64(5)
            });
        }

        return results;
    }

    public async Task<Dictionary<string, double>> GetSiteKPIs(string site, DateTime date)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                uniqMerge(unique_tags) as tag_count,
                avgMerge(avg_value) as avg_value,
                sumMerge(good_quality_count) as good_count,
                countMerge() as total_count
            FROM site_kpis
            WHERE site = {site:String}
              AND timestamp = {date:Date}";

        command.Parameters.Add(new ClickHouseParameter { ParameterName = "site", Value = site });
        command.Parameters.Add(new ClickHouseParameter { ParameterName = "date", Value = date.Date });

        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new Dictionary<string, double>
            {
                ["tag_count"] = reader.GetInt64(0),
                ["avg_value"] = reader.GetDouble(1),
                ["data_quality"] = (double)reader.GetInt64(2) / reader.GetInt64(3) * 100
            };
        }

        return new Dictionary<string, double>();
    }
}

public class TagAnalytics
{
    public required string TagName { get; set; }
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string Quality { get; set; } = "Good";
    public required string Site { get; set; }
    public required string Device { get; set; }
    public required string DataType { get; set; }
}

public class HourlyStats
{
    public required string TagName { get; set; }
    public DateTime Timestamp { get; set; }
    public double AvgValue { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public long SampleCount { get; set; }
}
