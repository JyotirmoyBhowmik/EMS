using System.ComponentModel.DataAnnotations;

namespace AuthService.Models;

public class User
{
    public Guid Id { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    
    public Guid RoleId { get; set; }
    // Navigation property if Role model existed in this context, 
    // simplified for now as microservice might duplicate or reference shared role definitions
}
