using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Route("api/v1/platform/modules")]
[Authorize(Roles = "PlatformAdmin")]
public class PlatformModulesController : ApiControllerBase
{
    private readonly IPlatformService _platform;

    public PlatformModulesController(IPlatformService platform)
    {
        _platform = platform;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var modules = await _platform.GetModulesAsync(ct);
        return EnvelopeCollection(modules);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ModuleCatalogRequest request, CancellationToken ct)
    {
        var module = await _platform.CreateModuleAsync(request, ct);
        return Envelope(module);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ModuleCatalogRequest request, CancellationToken ct)
    {
        var module = await _platform.UpdateModuleAsync(id, request, ct);
        return Envelope(module);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _platform.DeleteModuleAsync(id, ct);
        return Envelope(new { deleted });
    }
}
