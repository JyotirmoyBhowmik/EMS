using EnergyManagement.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Prometheus;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/energy-management-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Energy Management API", 
        Version = "v1",
        Description = "ISO 50001 compliant energy monitoring and carbon footprint tracking"
    });
});

// Database
var postgresConnection = builder.Configuration.GetConnectionString("PostgreSQL") 
    ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
    ?? "Host=postgres;Database=scada;Username=scada;Password=scada123";

builder.Services.AddDbContext<EnergyDbContext>(options =>
    options.UseNpgsql(postgresConnection));

builder.Services.AddHealthChecks()
    .AddNpgSql(postgresConnection);

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();

Log.Information("Energy Management service starting...");
app.Run();
