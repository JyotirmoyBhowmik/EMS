using Microsoft.AspNetCore.SignalR;
using WebSocketService.Hubs;
using WebSocketService.Services;
using Serilog;
using Prometheus;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// SignalR for WebSocket
builder.Services.AddSignalR();

// Background service for RabbitMQ consumption
builder.Services.AddHostedService<TagStreamingWorker>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors();
app.UseHttpMetrics();

app.MapHub<TagHub>("/hubs/tags");
app.MapHealthChecks("/health");
app.MapMetrics();

Log.Information("WebSocket Service starting - SignalR Hub: /hubs/tags");
app.Run();
