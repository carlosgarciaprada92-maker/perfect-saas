using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Route("api/v1/platform/tenants")]
[Authorize(Roles = "PlatformAdmin")]
public class PlatformTenantsController : ApiControllerBase
{
    private readonly IPlatformService _platform;

    public PlatformTenantsController(IPlatformService platform)
    {
        _platform = platform;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, CancellationToken ct)
    {
        var tenants = await _platform.GetTenantsAsync(search, ct);
        return EnvelopeCollection(tenants);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] TenantStatusUpdateRequest request, CancellationToken ct)
    {
        var tenant = await _platform.UpdateTenantStatusAsync(id, request, ct);
        return Envelope(tenant);
    }
}
