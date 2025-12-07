using HotChocolate;
using HotChocolate.Subscriptions;

namespace GraphQLService.Schema;

public class Query
{
    public async Task<List<Tag>> GetTags([Service] IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetFromJsonAsync<List<Tag>>("http://scada-core:5000/api/tags");
        return response ?? new List<Tag>();
    }

    public async Task<Tag?> GetTag(string name, [Service] IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient();
        return await client.GetFromJsonAsync<Tag>($"http://scada-core:5000/api/tags/by-name/{name}");
    }

    public async Task<List<Alarm>> GetAlarms([Service] IHttpClientFactory httpClientFactory)
    {
        // Query alarm management service
        return new List<Alarm>();
    }

    public async Task<AnalyticsResult> GetAnalytics(AnalyticsQuery query)
    {
        // Complex analytics queries
        return new AnalyticsResult
        {
            TotalTags = 150,
            ActiveAlarms = 3,
            AverageTagValue = 523.4,
            Trend = "Increasing"
        };
    }
}

public class Mutation
{
    public async Task<Tag> CreateTag(CreateTagInput input, [Service] IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("http://scada-core:5000/api/tags", input);
        return await response.Content.ReadFromJsonAsync<Tag>() ?? throw new Exception("Failed to create tag");
    }

    public async Task<bool> AcknowledgeAlarm(int alarmId, string user)
    {
        // Acknowledge alarm logic
        return true;
    }
}

public class Subscription
{
    [Subscribe]
    [Topic("tag_updates")]
    public TagValue TagValueUpdated([EventMessage] TagValue value) => value;

    [Subscribe]
    [Topic("alarms")]
    public Alarm AlarmTriggered([EventMessage] Alarm alarm) => alarm;
}

// Models
public record Tag(int Id, string Name, string Description, string DataType, string Units, bool IsEnabled);
public record TagValue(string TagName, double Value, DateTime Timestamp, string Quality);
public record Alarm(int Id, string TagName, string Type, string Priority, string Message, DateTime TriggeredAt);
public record CreateTagInput(string Name, string Description, string DataType, string Units);
public record AnalyticsQuery(DateTime Start, DateTime End, string? Site);
public record AnalyticsResult(int TotalTags, int ActiveAlarms, double AverageTagValue, string Trend);
