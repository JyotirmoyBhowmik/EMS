using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Models;
using AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<UserInfo?> ValidateTokenAsync(string token);
    string GenerateToken(User user);
}

public class JwtAuthService : IAuthService
{
    private readonly AuthDbContext _context;
    private readonly ILogger<JwtAuthService> _logger;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpiryMinutes;

    public JwtAuthService(AuthDbContext context, ILogger<JwtAuthService> logger)
    {
        _context = context;
        _logger = logger;
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? "change-this-secret-key-in-production-use-256-bit";
        _jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "ScadaSystem";
        _jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "ScadaClient";
        _jwtExpiryMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "60");
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
            return null;
        }

        // Check MFA if enabled
        if (user.MfaEnabled && string.IsNullOrEmpty(request.MfaCode))
        {
            return new LoginResponse
            {
                Token = "",
                RefreshToken = "",
                ExpiresAt = DateTime.UtcNow,
                User = MapToUserInfo(user),
                MfaRequired = true
            };
        }

        // Update last login
        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = GenerateToken(user);
        var refreshToken = Guid.NewGuid().ToString();

        _logger.LogInformation("User {Username} logged in successfully", user.Username);

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
            User = MapToUserInfo(user),
            MfaRequired = false
        };
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("firstName", user.FirstName ?? ""),
            new Claim("lastName", user.LastName ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<UserInfo?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

            var user = await _context.Users.FindAsync(userId);
            return user != null ? MapToUserInfo(user) : null;
        }
        catch
        {
            return null;
        }
    }

    private static UserInfo MapToUserInfo(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Username = user.Username,
        Role = user.Role,
        FirstName = user.FirstName,
        LastName = user.LastName
    };
}
