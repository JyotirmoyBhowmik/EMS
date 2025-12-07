using Serilog;
using Prometheus;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(',') 
            ?? new[] { "http://localhost:3000", "http://localhost:3001" };
        
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors();
app.UseHttpMetrics();

app.MapReverseProxy();
app.MapHealthChecks("/health");
app.MapMetrics();

Log.Information("API Gateway starting...");
app.Run();
