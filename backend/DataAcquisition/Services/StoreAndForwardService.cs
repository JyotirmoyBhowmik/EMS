using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace DataAcquisition.Services;

/// <summary>
/// Store-and-forward service for handling network outages
/// Uses local SQLite database to buffer data when InfluxDB is unavailable
/// Automatically replays buffered data when connectivity is restored
/// </summary>
public class StoreAndForwardService : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ILogger<StoreAndForwardService> _logger;
    private const int MAX_BUFFER_SIZE = 1_000_000; // Maximum buffered points
    private const string DB_PATH = "store_and_forward.db";

    public StoreAndForwardService(ILogger<StoreAndForwardService> logger)
    {
        _logger = logger;

        // Initialize SQLite database
        _connection = new SqliteConnection($"Data Source={DB_PATH}");
        _connection.Open();

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS buffered_data (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                tag_name TEXT NOT NULL,
                value TEXT NOT NULL,
                timestamp TEXT NOT NULL,
                quality TEXT,
                site TEXT,
                device TEXT,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP
            );
            CREATE INDEX IF NOT EXISTS idx_timestamp ON buffered_data(timestamp);
        ";
        cmd.ExecuteNonQuery();

        _logger.LogInformation("Store-and-forward database initialized");
    }

    /// <summary>
    /// Store a failed message for later replay
    /// </summary>
    public async Task BufferAsync(TagDataMessage message)
    {
        // Check buffer size limit
        var count = await GetBufferedCountAsync();
        if (count >= MAX_BUFFER_SIZE)
        {
            _logger.LogWarning("Buffer is full ({Count} messages), discarding oldest", count);
            await PurgeOldestAsync(1000);
        }

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO buffered_data (tag_name, value, timestamp, quality, site, device)
            VALUES (@tagName, @value, @timestamp, @quality, @site, @device)
        ";
        cmd.Parameters.AddWithValue("@tagName", message.TagName);
        cmd.Parameters.AddWithValue("@value", JsonSerializer.Serialize(message.Value));
        cmd.Parameters.AddWithValue("@timestamp", message.Timestamp.ToString("o"));
        cmd.Parameters.AddWithValue("@quality", message.Quality);
        cmd.Parameters.AddWithValue("@site", message.Site ?? "");
        cmd.Parameters.AddWithValue("@device", message.Device ?? "");

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Get buffered messages for replay
    /// </summary>
    public async Task<List<TagDataMessage>> GetBufferedMessagesAsync(int limit = 1000)
    {
        var messages = new List<TagDataMessage>();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            SELECT id, tag_name, value, timestamp, quality, site, device
            FROM buffered_data
            ORDER BY timestamp ASC
            LIMIT @limit
        ";
        cmd.Parameters.AddWithValue("@limit", limit);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            messages.Add(new TagDataMessage
            {
                TagName = reader.GetString(1),
                Value = JsonSerializer.Deserialize<object>(reader.GetString(2))!,
                Timestamp = DateTime.Parse(reader.GetString(3)),
                Quality = reader.GetString(4),
                Site = reader.GetString(5),
                Device = reader.GetString(6)
            });
        }

        return messages;
    }

    /// <summary>
    /// Delete successfully replayed messages
    /// </summary>
    public async Task DeleteBufferedAsync(int count)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            DELETE FROM buffered_data
            WHERE id IN (
                SELECT id FROM buffered_data
                ORDER BY timestamp ASC
                LIMIT @count
            )
        ";
        cmd.Parameters.AddWithValue("@count", count);
        await cmd.ExecuteNonQueryAsync();

        _logger.LogInformation("Deleted {Count} replayed messages from buffer", count);
    }

    /// <summary>
    /// Get current buffer count
    /// </summary>
    public async Task<int> GetBufferedCountAsync()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM buffered_data";
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Purge oldest messages when buffer is full
    /// </summary>
    private async Task PurgeOldestAsync(int count)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            DELETE FROM buffered_data
            WHERE id IN (
                SELECT id FROM buffered_data
                ORDER BY timestamp ASC
                LIMIT @count
            )
        ";
        cmd.Parameters.AddWithValue("@count", count);
        await cmd.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
