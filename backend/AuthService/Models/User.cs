namespace AuthService.Models;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Role { get; set; } = "Viewer";
    public bool IsActive { get; set; } = true;
    public bool MfaEnabled { get; set; } = false;
    public string? MfaSecret { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? MfaCode { get; set; }
}

public class LoginResponse
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required UserInfo User { get; set; }
    public bool MfaRequired { get; set; }
}

public class UserInfo
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string Role { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
