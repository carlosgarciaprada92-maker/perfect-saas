using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Authorize]
[RequireTenant]
[Route("api/v1/inventory")]
public class InventoryController : ApiControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    [HttpPost("in")]
    [RequirePermission("inventory.write")]
    public async Task<IActionResult> InventoryIn([FromBody] InventoryMovementRequest request, CancellationToken ct)
    {
        var response = await _service.InventoryInAsync(request, ct);
        return Envelope(response);
    }

    [HttpPost("adjust")]
    [RequirePermission("inventory.write")]
    public async Task<IActionResult> InventoryAdjust([FromBody] InventoryMovementRequest request, CancellationToken ct)
    {
        var response = await _service.InventoryAdjustAsync(request, ct);
        return Envelope(response);
    }

    [HttpGet("movements")]
    [RequirePermission("products.read")]
    public async Task<IActionResult> Movements([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var rows = await _service.GetMovementsAsync(page, pageSize, ct);
        return EnvelopePaged(rows);
    }
}