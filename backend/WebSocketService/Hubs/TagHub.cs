using Microsoft.AspNetCore.SignalR;
using Prometheus;

namespace WebSocketService.Hubs;

public class TagHub : Hub
{
    private static readonly Counter _connectionsTotal = Metrics.CreateCounter(
        "signalr_connections_total", "Total SignalR connections");
    private static readonly Gauge _activeConnections = Metrics.CreateGauge(
        "signalr_connections_active", "Current active SignalR connections");
    
    private readonly ILogger<TagHub> _logger;

    public TagHub(ILogger<TagHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _connectionsTotal.Inc();
        _activeConnections.Inc();
        
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Client connected: {ConnectionId}", connectionId);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _activeConnections.Dec();
        
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Client disconnected: {ConnectionId}", connectionId);
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToTags(List<string> tagNames)
    {
        foreach (var tagName in tagNames)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tag:{tagName}");
            _logger.LogInformation("Client {ConnectionId} subscribed to {TagName}", 
                Context.ConnectionId, tagName);
        }

        await Clients.Caller.SendAsync("SubscriptionConfirmed", tagNames);
    }

    public async Task UnsubscribeFromTags(List<string> tagNames)
    {
        foreach (var tagName in tagNames)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tag:{tagName}");
        }

        await Clients.Caller.SendAsync("UnsubscriptionConfirmed", tagNames);
    }

    public async Task SubscribeToAlarms()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "alarms");
        await Clients.Caller.SendAsync("AlarmSubscriptionConfirmed");
    }

    // Server method to broadcast tag updates
    public async Task BroadcastTagUpdate(string tagName, double value, DateTime timestamp, string quality)
    {
        await Clients.Group($"tag:{tagName}").SendAsync("TagUpdate", new
        {
            tagName,
            value,
            timestamp,
            quality
        });
    }

    // Broadcast alarm
    public async Task BroadcastAlarm(object alarm)
    {
        await Clients.Group("alarms").SendAsync("AlarmTriggered", alarm);
    }
}
