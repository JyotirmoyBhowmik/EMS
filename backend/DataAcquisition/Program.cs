using DataAcquisition.Services;
using Serilog;
using Prometheus;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/data-acquisition-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Data Acquisition API", 
        Version = "v1",
        Description = "High-performance tag data collection and ingestion service (100k+ tags/sec)"
    });
});

// Data acquisition services
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddSingleton<InfluxDBWriterService>();
builder.Services.AddSingleton<StoreAndForwardService>();
builder.Services.AddHostedService<DataIngestionWorker>();
builder.Services.AddHostedService<StoreAndForwardWorker>();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpMetrics();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();

Log.Information("Data Acquisition service starting (high-performance mode)...");
app.Run();
