using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Authorize]
[RequireTenant]
[Route("api/v1/roles")]
public class RolesController : ApiControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [RequirePermission("roles.manage")]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var roles = await _roleService.GetAsync(ct);
        return EnvelopeCollection(roles);
    }

    [HttpPost]
    [RequirePermission("roles.manage")]
    public async Task<IActionResult> Create([FromBody] RoleRequest request, CancellationToken ct)
    {
        var created = await _roleService.CreateAsync(request, ct);
        return Envelope(created);
    }

    [HttpPut("{id:guid}/permissions")]
    [RequirePermission("roles.manage")]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] AssignPermissionsRequest request, CancellationToken ct)
    {
        await _roleService.AssignPermissionsAsync(id, request, ct);
        return Envelope(new { updated = true });
    }
}