using AuthService.Data;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/auth-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var postgresConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION") 
    ?? "Host=localhost;Database=scada;Username=scada;Password=scada123";
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(postgresConnection));

// JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? "change-this-secret-key-in-production-use-256-bit";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "ScadaSystem";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "ScadaClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<IAuthService, JwtAuthService>();
builder.Services.AddScoped<IMfaService, TotpMfaService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("Auth Service starting...");
app.Run();
