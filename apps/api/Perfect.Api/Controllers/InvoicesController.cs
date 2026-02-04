using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Authorize]
[RequireTenant]
[Route("api/v1/invoices")]
public class InvoicesController : ApiControllerBase
{
    private readonly IInvoiceService _service;

    public InvoicesController(IInvoiceService service)
    {
        _service = service;
    }

    [HttpPost]
    [RequirePermission("invoices.write")]
    public async Task<IActionResult> Create([FromBody] InvoiceCreateRequest request, CancellationToken ct)
    {
        var invoice = await _service.CreateInvoiceAsync(request, ct);
        return Envelope(invoice);
    }

    [HttpGet]
    [RequirePermission("invoices.read")]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentType = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var rows = await _service.GetInvoicesAsync(page, pageSize, search, status, paymentType, customerId, from, to, ct);
        return EnvelopePaged(rows);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("invoices.read")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
    {
        var detail = await _service.GetInvoiceAsync(id, ct);
        return Envelope(detail);
    }

    [HttpPost("{id:guid}/payments")]
    [RequirePermission("payments.write")]
    public async Task<IActionResult> RegisterPayment(Guid id, [FromBody] PaymentRequest request, CancellationToken ct)
    {
        var payment = await _service.RegisterPaymentAsync(id, request, ct);
        return Envelope(payment);
    }

    [HttpPost("{id:guid}/mark-paid")]
    [RequirePermission("payments.write")]
    public async Task<IActionResult> MarkPaid(Guid id, CancellationToken ct)
    {
        var invoice = await _service.MarkPaidAsync(id, ct);
        return Envelope(invoice);
    }
}