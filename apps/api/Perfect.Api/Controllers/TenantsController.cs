using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Route("api/v1/tenants")]
public class TenantsController : ApiControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly IConfiguration _configuration;

    public TenantsController(ITenantService tenantService, IConfiguration configuration)
    {
        _tenantService = tenantService;
        _configuration = configuration;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] TenantCreateRequest request, CancellationToken ct)
    {
        var configuredKey = _configuration["Platform:BootstrapKey"];
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            var provided = HttpContext.Request.Headers["X-Platform-Key"].FirstOrDefault();
            if (!string.Equals(provided, configuredKey, StringComparison.Ordinal))
            {
                return Unauthorized(new
                {
                    data = (object?)null,
                    meta = (object?)null,
                    errors = new[] { new { code = "unauthorized", message = "Invalid platform key" } }
                });
            }
        }

        var response = await _tenantService.CreateTenantAsync(request, ct);
        return Envelope(response);
    }

    [HttpGet("me")]
    [Authorize]
    [RequireTenant]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var response = await _tenantService.GetCurrentTenantAsync(ct);
        return Envelope(response);
    }

    [HttpPut("settings")]
    [Authorize]
    [RequireTenant]
    [RequirePermission("tenant.settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] TenantSettingsRequest request, CancellationToken ct)
    {
        var response = await _tenantService.UpdateSettingsAsync(request, ct);
        return Envelope(response);
    }
}
