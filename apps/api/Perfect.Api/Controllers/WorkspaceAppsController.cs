using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Route("api/v1/workspace/apps")]
[Authorize(Roles = "TenantAdmin,ADMIN")]
[RequireTenant]
public class WorkspaceAppsController : ApiControllerBase
{
    private readonly IWorkspaceService _workspace;

    public WorkspaceAppsController(IWorkspaceService workspace)
    {
        _workspace = workspace;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var apps = await _workspace.GetAppsAsync(ct);
        return EnvelopeCollection(apps);
    }
}
