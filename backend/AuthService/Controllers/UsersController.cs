using Microsoft.AspNetCore.Mvc;
using AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(AuthDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.Select(u => new { u.Id, u.Email, u.FullName, u.IsActive }).ToListAsync();
        return Ok(users);
    }

    // Additional user management methods (Create, Update, disable) would go here
}
