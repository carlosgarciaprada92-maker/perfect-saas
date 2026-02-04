using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Authorize]
[RequireTenant]
[Route("api/v1/ar")]
public class AccountsReceivableController : ApiControllerBase
{
    private readonly IArService _service;

    public AccountsReceivableController(IArService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    [RequirePermission("ar.read")]
    public async Task<IActionResult> Summary(
        [FromQuery] int rangeDays = 30,
        [FromQuery] int thresholdDays = 5,
        [FromQuery] bool includePaid = false,
        [FromQuery] Guid? customerId = null,
        CancellationToken ct = default)
    {
        var data = await _service.GetSummaryAsync(rangeDays, thresholdDays, includePaid, customerId, ct);
        return Envelope(data);
    }

    [HttpGet("due-soon")]
    [RequirePermission("ar.read")]
    public async Task<IActionResult> DueSoon([FromQuery] int thresholdDays = 5, CancellationToken ct = default)
    {
        var data = await _service.GetDueSoonAsync(thresholdDays, ct);
        return EnvelopeCollection(data);
    }

    [HttpGet("overdue")]
    [RequirePermission("ar.read")]
    public async Task<IActionResult> Overdue(CancellationToken ct)
    {
        var data = await _service.GetOverdueAsync(ct);
        return EnvelopeCollection(data);
    }

    [HttpGet("open-items")]
    [RequirePermission("ar.read")]
    public async Task<IActionResult> OpenItems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] bool includePaid = false,
        [FromQuery] Guid? customerId = null,
        [FromQuery] int thresholdDays = 5,
        [FromQuery] string? search = null,
        [FromQuery] int? rangeDays = null,
        CancellationToken ct = default)
    {
        var data = await _service.GetOpenItemsAsync(page, pageSize, status, includePaid, customerId, thresholdDays, search, rangeDays, ct);
        return EnvelopePaged(data);
    }
}