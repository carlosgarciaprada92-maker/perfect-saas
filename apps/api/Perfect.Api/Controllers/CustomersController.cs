using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Authorize]
[RequireTenant]
[Route("api/v1/customers")]
public class CustomersController : ApiControllerBase
{
    private readonly ICustomerService _service;

    public CustomersController(ICustomerService service)
    {
        _service = service;
    }

    [HttpGet]
    [RequirePermission("customers.read")]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var rows = await _service.GetAsync(page, pageSize, search, ct);
        return EnvelopePaged(rows);
    }

    [HttpPost]
    [RequirePermission("customers.write")]
    public async Task<IActionResult> Create([FromBody] CustomerRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return Envelope(created);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("customers.write")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CustomerRequest request, CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(id, request, ct);
        return Envelope(updated);
    }

    [HttpPatch("{id:guid}/credit-terms")]
    [RequirePermission("customers.write")]
    public async Task<IActionResult> UpdateCreditTerms(Guid id, [FromBody] CustomerCreditTermsRequest request, CancellationToken ct)
    {
        var updated = await _service.UpdateCreditTermsAsync(id, request, ct);
        return Envelope(updated);
    }

    [HttpGet("inactive")]
    [RequirePermission("customers.read")]
    public async Task<IActionResult> Inactive([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var rows = await _service.GetInactiveAsync(days, ct);
        return EnvelopeCollection(rows);
    }
}