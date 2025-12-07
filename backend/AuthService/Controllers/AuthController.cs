using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMfaService _mfaService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IMfaService mfaService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _mfaService = mfaService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        
        if (response == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        if (response.MfaRequired)
        {
            return Ok(new { mfaRequired = true, message = "MFA code required" });
        }

        return Ok(response);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        var userInfo = await _authService.ValidateTokenAsync(token);
        
        if (userInfo == null)
        {
            return Unauthorized();
        }

        return Ok(userInfo);
    }

    [HttpPost("mfa/setup")]
    public IActionResult SetupMfa([FromQuery] string email)
    {
        var secret = _mfaService.GenerateSecret();
        var uri = _mfaService.GenerateQrCodeUri(email, secret);
        var qrCode = _mfaService.GenerateQrCodeImage(uri);

        return Ok(new
        {
            secret,
            qrCodeImage = Convert.ToBase64String(qrCode)
        });
    }

    [HttpPost("mfa/verify")]
    public IActionResult VerifyMfa([FromBody] MfaVerifyRequest request)
    {
        var isValid = _mfaService.ValidateCode(request.Secret, request.Code);
        return Ok(new { isValid });
    }
}

public class MfaVerifyRequest
{
    public required string Secret { get; set; }
    public required string Code { get; set; }
}
