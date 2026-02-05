using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Perfect.Application.Contracts;
using Perfect.Application.Services;
using Perfect.Api.Authorization;

namespace Perfect.Api.Controllers;

[Route("api/v1/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IDemoSeedService _demoSeedService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IDemoSeedService demoSeedService, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _authService = authService;
        _demoSeedService = demoSeedService;
        _environment = environment;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);
        return Envelope(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var response = await _authService.RefreshAsync(request, ct);
        return Envelope(response);
    }

    [HttpPost("logout")]
    [Authorize]
    [RequireTenant]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await _authService.LogoutAsync(request.RefreshToken, ct);
        return Envelope(new { loggedOut = true });
    }

    [HttpPost("seed-demo")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedDemo([FromQuery] string? slug, CancellationToken ct)
    {
        var allowSeed = _configuration.GetValue("Platform:AllowDemoSeed", false);
        if (!_environment.IsDevelopment() && !allowSeed)
        {
            return NotFound();
        }

        await _demoSeedService.SeedDemoAsync(slug, ct);
        return Envelope(new { seeded = true, slug = string.IsNullOrWhiteSpace(slug) ? "demo" : slug.Trim().ToLowerInvariant() });
    }
}
