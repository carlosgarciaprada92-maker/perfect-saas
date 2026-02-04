using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Authorize]
[RequireTenant]
[Route("api/v1/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [RequirePermission("users.manage")]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _userService.GetAsync(page, pageSize, search, ct);
        return EnvelopePaged(result);
    }

    [HttpPost]
    [RequirePermission("users.manage")]
    public async Task<IActionResult> Create([FromBody] UserRequest request, CancellationToken ct)
    {
        var created = await _userService.CreateAsync(request, ct);
        return Envelope(created);
    }

    [HttpPatch("{id:guid}/status")]
    [RequirePermission("users.manage")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UserStatusRequest request, CancellationToken ct)
    {
        var updated = await _userService.UpdateStatusAsync(id, request.IsActive, ct);
        return Envelope(updated);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("users.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequest request, CancellationToken ct)
    {
        var updated = await _userService.UpdateAsync(id, request, ct);
        return Envelope(updated);
    }

    [HttpPut("{id:guid}/roles")]
    [RequirePermission("users.manage")]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        await _userService.AssignRolesAsync(id, request, ct);
        return Envelope(new { updated = true });
    }
}
