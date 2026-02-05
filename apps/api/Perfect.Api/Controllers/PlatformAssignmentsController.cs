using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Route("api/v1/platform/assignments")]
[Authorize(Roles = "PlatformAdmin")]
public class PlatformAssignmentsController : ApiControllerBase
{
    private readonly IPlatformService _platform;

    public PlatformAssignmentsController(IPlatformService platform)
    {
        _platform = platform;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid tenantId, CancellationToken ct)
    {
        var assignments = await _platform.GetAssignmentsAsync(tenantId, ct);
        return EnvelopeCollection(assignments);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ModuleAssignmentUpdateRequest request, CancellationToken ct)
    {
        var updated = await _platform.UpdateAssignmentsAsync(request, ct);
        return Envelope(new { updated });
    }
}
