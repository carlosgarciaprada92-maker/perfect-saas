using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Authorize]
[RequireTenant]
[Route("api/v1/products")]
public class ProductsController : ApiControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    [RequirePermission("products.read")]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        [FromQuery] string? order = null,
        CancellationToken ct = default)
    {
        var data = await _service.GetAsync(page, pageSize, search, sort, order, ct);
        return EnvelopePaged(data);
    }

    [HttpPost]
    [RequirePermission("products.write")]
    public async Task<IActionResult> Create([FromBody] ProductRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return Envelope(created);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("products.write")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductRequest request, CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(id, request, ct);
        return Envelope(updated);
    }

    [HttpPatch("{id:guid}/status")]
    [RequirePermission("products.write")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ProductStatusRequest request, CancellationToken ct)
    {
        var updated = await _service.UpdateStatusAsync(id, request.IsActive, ct);
        return Envelope(updated);
    }

    [HttpGet("low-stock")]
    [RequirePermission("products.read")]
    public async Task<IActionResult> LowStock([FromQuery] int? threshold, CancellationToken ct)
    {
        var rows = await _service.GetLowStockAsync(threshold, ct);
        return EnvelopeCollection(rows);
    }
}