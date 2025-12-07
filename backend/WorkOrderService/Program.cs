using WorkOrderService.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Prometheus;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/workorder-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Work Order Management API", 
        Version = "v1",
        Description = "Maintenance work order tracking and management"
    });
});

// Database
var postgresConnection = builder.Configuration.GetConnectionString("PostgreSQL") 
    ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
    ?? "Host=postgres;Database=scada;Username=scada;Password=scada123";

builder.Services.AddDbContext<WorkOrderDbContext>(options =>
    options.UseNpgsql(postgresConnection));

builder.Services.AddHealthChecks()
    .AddNpgSql(postgresConnection);

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WorkOrderDbContext>();
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

Log.Information("Work Order Service starting...");
app.Run();
