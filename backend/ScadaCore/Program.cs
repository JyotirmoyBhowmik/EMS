using ScadaCore.Services;
using ScadaCore.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Serilog;
using Prometheus;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/scada-core-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "SCADA Core API", 
        Version = "v1",
        Description = "Core SCADA engine for tag management and real-time data access"
    });
});

// Database contexts
var postgresConnection = builder.Configuration.GetConnectionString("PostgreSQL") 
    ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
builder.Services.AddDbContext<ScadaDbContext>(options =>
    options.UseNpgsql(postgresConnection));

// Redis cache
var redisConnection = builder.Configuration.GetConnectionString("Redis") 
    ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection!));

// InfluxDB
builder.Services.AddSingleton<InfluxDBService>(sp =>
{
    var url = Environment.GetEnvironmentVariable("INFLUXDB_URL") ?? "http://localhost:8086";
    var token = Environment.GetEnvironmentVariable("INFLUXDB_TOKEN") ?? "scada-token-change-in-production";
    var org = Environment.GetEnvironmentVariable("INFLUXDB_ORG") ?? "scada-org";
    var bucket = Environment.GetEnvironmentVariable("INFLUXDB_BUCKET") ?? "scada-data";
    return new InfluxDBService(url, token, org, bucket);
});

// SCADA services
builder.Services.AddSingleton<TagCacheService>();
builder.Services.AddScoped<TagManagementService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddHostedService<TagSyncService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(postgresConnection!)
    .AddRedis(redisConnection!);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SCADA Core API v1"));
}

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ScadaDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseCors();
app.UseRouting();
app.UseHttpMetrics(); // Prometheus metrics

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics(); // Prometheus endpoint

Log.Information("SCADA Core service starting...");
app.Run();
