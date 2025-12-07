using AlarmManagement.Services;
using Serilog;
using Prometheus;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/alarm-management-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Alarm Management API", 
        Version = "v1",
        Description = "Real-time alarm processing and notification service"
    });
});

// Alarm services
builder.Services.AddSingleton<RabbitMQAlarmService>();
builder.Services.AddSingleton<EmailNotificationService>();
builder.Services.AddSingleton<SMSNotificationService>();
builder.Services.AddHostedService<AlarmProcessingWorker>();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();

Log.Information("Alarm Management service starting...");
app.Run();
