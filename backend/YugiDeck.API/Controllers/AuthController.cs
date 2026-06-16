using Microsoft.AspNetCore.Mvc;
using YugiDeck.Core.DTOs.Auth;
using YugiDeck.Core.Interfaces;

namespace YugiDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var result = await authService.RefreshAsync(request.RefreshToken);
        return Ok(result);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshRequest request)
    {
        await authService.RevokeAsync(request.RefreshToken);
        return NoContent();
    }
}
