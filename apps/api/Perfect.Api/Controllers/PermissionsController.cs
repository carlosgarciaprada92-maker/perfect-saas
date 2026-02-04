using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Authorize]
[RequireTenant]
[Route("api/v1/permissions")]
public class PermissionsController : ApiControllerBase
{
    private readonly IRoleService _roleService;

    public PermissionsController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [RequirePermission("roles.manage")]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var permissions = await _roleService.GetPermissionsAsync(ct);
        return EnvelopeCollection(permissions);
    }
}