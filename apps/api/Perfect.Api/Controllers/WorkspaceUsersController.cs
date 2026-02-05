using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Route("api/v1/workspace/users")]
[Authorize(Roles = "TenantAdmin,ADMIN")]
[RequireTenant]
public class WorkspaceUsersController : ApiControllerBase
{
    private readonly IWorkspaceService _workspace;

    public WorkspaceUsersController(IWorkspaceService workspace)
    {
        _workspace = workspace;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        // TODO: Expand user list/roles once user management is exposed in workspace.
        var users = await _workspace.GetUsersAsync(ct);
        return EnvelopeCollection(users);
    }
}
