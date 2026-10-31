using Bookify.Authentication;
using Microsoft.AspNetCore.RateLimiting;

namespace Bookify.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("auth-sliding")]
public class AuthController(IAuthService auth) : ControllerBase
{

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request , CancellationToken cancellationToken)
    {
        var result = await auth.RegisterAsync(request , cancellationToken);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return BadRequest(ModelState);
        }

        return Ok(new { Message = "User registered successfully!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authResponse = await auth.LoginAsync(request,default);

        if (authResponse == null)
        {
            return Unauthorized(new { Message = "Invalid Email or Password." });
        }

        return Ok(authResponse);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResponse = await auth.GetRefreshTokenAsync(request, cancellationToken);
        if (authResponse == null)
        {
            return BadRequest(new { Message = "Invalid Token or Refresh Token" });
        }
        return Ok(authResponse);
    }

    [HttpPut("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeRefresheTokenRequest request, CancellationToken cancellationToken)
    {
        var isRevoked = await auth.RevokeTokenAsync(request, cancellationToken);
        if (!isRevoked)
        {
            return BadRequest(new { Message = "Token is invalid or already revoked." });
        }
        return Ok(new { Message = "Token revoked successfully!" });
    }
}