using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, error, response) = await _authService.RegisterAsync(request);
        
        if (!success)
        {
            return BadRequest(new { error });
        }

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, error, response) = await _authService.LoginAsync(request);
        
        if (!success)
        {
            return Unauthorized(new { error });
        }

        return Ok(response);
    }
}
