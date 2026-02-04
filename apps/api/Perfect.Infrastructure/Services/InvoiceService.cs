using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Models;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Domain.Enums;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;
    private readonly IInvoiceStatusCalculator _statusCalculator;
    private readonly IDateTimeProvider _clock;
    private readonly IUserContext _userContext;

    public InvoiceService(AppDbContext db, IInvoiceStatusCalculator statusCalculator, IDateTimeProvider clock, IUserContext userContext)
    {
        _db = db;
        _statusCalculator = statusCalculator;
        _clock = clock;
        _userContext = userContext;
    }

    public async Task<InvoiceResponse> CreateInvoiceAsync(InvoiceCreateRequest request, CancellationToken ct)
    {
        if (request.Items.Count == 0)
        {
            throw new AppException(ErrorCodes.Validation, "Invoice items required");
        }

        var paymentType = ParsePaymentType(request.PaymentType);
        if (paymentType == PaymentType.Credit && !request.CustomerId.HasValue)
        {
            throw new AppException(ErrorCodes.Validation, "Customer is required for credit invoices");
        }

        Customer? customer = null;
        if (request.CustomerId.HasValue)
        {
            customer = await _db.Customers.FirstOrDefaultAsync(x => x.Id == request.CustomerId.Value, ct)
                ?? throw new AppException(ErrorCodes.NotFound, "Customer not found", 404);
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(ct);
        if (products.Count != productIds.Count)
        {
            throw new AppException(ErrorCodes.Validation, "One or more products are invalid");
        }

        var date = _clock.UtcNow;
        var creditDays = paymentType == PaymentType.Cash
            ? 0
            : request.CreditDaysApplied ?? customer?.DefaultCreditDays ?? 15;

        if (creditDays < 0 || creditDays > 365)
        {
            throw new AppException(ErrorCodes.Validation, "creditDaysApplied out of allowed range");
        }

        var dueDate = _statusCalculator.ResolveDueDate(date, paymentType, creditDays);
        var number = await BuildInvoiceNumberAsync(ct);

        var invoice = new Invoice
        {
            Number = number,
            CustomerId = customer?.Id,
            Date = date,
            PaymentType = paymentType,
            CreditDaysApplied = creditDays,
            DueDate = dueDate,
            CreatedByUserId = _userContext.UserId ?? Guid.Empty
        };

        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            var unitPrice = item.UnitPrice ?? product.Price;
            if (unitPrice < 0)
            {
                throw new AppException(ErrorCodes.Validation, "Unit price cannot be negative");
            }

            var total = unitPrice * item.Quantity;
            invoice.Items.Add(new InvoiceItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                Total = total
            });

            _db.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = product.Id,
                Type = InventoryMovementType.Out,
                Quantity = item.Quantity,
                Reason = $"Invoice {number}"
            });
        }

        invoice.Total = invoice.Items.Sum(x => x.Total);
        invoice.PaidTotal = 0;
        invoice.Balance = invoice.Total;

        var thresholdDays = await GetDueSoonThresholdAsync(ct);
        invoice.Status = _statusCalculator.ResolveStatus(invoice.Balance, invoice.DueDate, date, thresholdDays);

        if (paymentType == PaymentType.Cash && invoice.Balance == invoice.Total)
        {
            invoice.PaidTotal = invoice.Total;
            invoice.Balance = 0;
            invoice.Status = InvoiceStatus.Paid;
            invoice.Payments.Add(new Payment
            {
                Amount = invoice.Total,
                Method = "CASH",
                Date = date,
                CreatedByUserId = _userContext.UserId
            });
        }

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        return ToInvoiceResponse(invoice, customer?.Name);
    }

    public async Task<PaginatedResult<InvoiceResponse>> GetInvoicesAsync(int page, int pageSize, string? search, string? status, string? paymentType, Guid? customerId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var query = _db.Invoices.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Number.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(i => i.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(paymentType) && Enum.TryParse<PaymentType>(paymentType, true, out var parsedType))
        {
            query = query.Where(i => i.PaymentType == parsedType);
        }

        if (customerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == customerId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(i => i.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(i => i.Date <= to.Value);
        }

        var mapped = query
            .OrderByDescending(i => i.Date)
            .Select(i => new InvoiceResponse(
                i.Id,
                i.Number,
                i.Date,
                i.PaymentType.ToString().ToUpperInvariant(),
                i.CreditDaysApplied,
                i.DueDate,
                i.Status.ToString().ToUpperInvariant(),
                i.Total,
                i.PaidTotal,
                i.Balance,
                i.CustomerId,
                i.Customer != null ? i.Customer.Name : null
            ));

        return await mapped.ToPagedAsync(page, pageSize, ct);
    }

    public async Task<InvoiceDetailResponse> GetInvoiceAsync(Guid id, CancellationToken ct)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .ThenInclude(ii => ii.Product)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Invoice not found", 404);

        var header = ToInvoiceResponse(invoice, invoice.Customer?.Name);
        var items = invoice.Items.Select(ii => new InvoiceLineResponse(ii.Id, ii.ProductId, ii.Product.Name, ii.Quantity, ii.UnitPrice, ii.Total)).ToList();
        var payments = invoice.Payments.OrderByDescending(p => p.Date)
            .Select(p => new PaymentResponse(p.Id, p.Date, p.Amount, p.Method, p.Reference))
            .ToList();

        return new InvoiceDetailResponse(header, items, payments);
    }

    public async Task<PaymentResponse> RegisterPaymentAsync(Guid id, PaymentRequest request, CancellationToken ct)
    {
        var invoice = await _db.Invoices.Include(i => i.Payments).FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Invoice not found", 404);

        if (request.Amount > invoice.Balance)
        {
            throw new AppException(ErrorCodes.Validation, "Payment exceeds current balance");
        }

        var payment = new Payment
        {
            InvoiceId = id,
            Amount = request.Amount,
            Method = request.Method,
            Reference = request.Reference,
            Date = _clock.UtcNow,
            CreatedByUserId = _userContext.UserId
        };

        invoice.Payments.Add(payment);
        invoice.PaidTotal += request.Amount;
        invoice.Balance = Math.Max(0, invoice.Total - invoice.PaidTotal);
        var threshold = await GetDueSoonThresholdAsync(ct);
        invoice.Status = _statusCalculator.ResolveStatus(invoice.Balance, invoice.DueDate, _clock.UtcNow, threshold);

        await _db.SaveChangesAsync(ct);
        return new PaymentResponse(payment.Id, payment.Date, payment.Amount, payment.Method, payment.Reference);
    }

    public async Task<InvoiceResponse> MarkPaidAsync(Guid id, CancellationToken ct)
    {
        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Invoice not found", 404);

        if (invoice.Balance > 0)
        {
            var amount = invoice.Balance;
            invoice.PaidTotal = invoice.Total;
            invoice.Balance = 0;
            invoice.Status = InvoiceStatus.Paid;

            _db.Payments.Add(new Payment
            {
                InvoiceId = invoice.Id,
                Amount = amount,
                Method = "MANUAL_MARK",
                Date = _clock.UtcNow,
                CreatedByUserId = _userContext.UserId
            });

            await _db.SaveChangesAsync(ct);
        }

        return ToInvoiceResponse(invoice, null);
    }

    private static PaymentType ParsePaymentType(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        if (normalized is "CONTADO" or "CASH")
        {
            return PaymentType.Cash;
        }

        if (normalized is "CREDITO" or "CRÉDITO" or "CREDIT")
        {
            return PaymentType.Credit;
        }

        if (!Enum.TryParse<PaymentType>(value, true, out var paymentType))
        {
            throw new AppException(ErrorCodes.Validation, "Invalid payment type");
        }
        return paymentType;
    }

    private async Task<int> GetDueSoonThresholdAsync(CancellationToken ct)
    {
        var configured = await _db.TenantSettings
            .Select(x => (int?)x.DueSoonThresholdDays)
            .FirstOrDefaultAsync(ct);
        return Math.Clamp(configured ?? 5, 1, 30);
    }

    private async Task<string> BuildInvoiceNumberAsync(CancellationToken ct)
    {
        var next = await _db.Invoices.CountAsync(ct) + 1;
        var format = await _db.TenantSettings.Select(x => x.InvoiceNumberingFormat).FirstOrDefaultAsync(ct) ?? "FAC-{0000}";
        var sequence = next.ToString("D4");
        if (format.Contains("{0000}"))
        {
            return format.Replace("{0000}", sequence);
        }

        return $"{format}-{sequence}";
    }

    private static InvoiceResponse ToInvoiceResponse(Invoice invoice, string? customerName)
    {
        return new InvoiceResponse(
            invoice.Id,
            invoice.Number,
            invoice.Date,
            invoice.PaymentType.ToString().ToUpperInvariant(),
            invoice.CreditDaysApplied,
            invoice.DueDate,
            invoice.Status.ToString().ToUpperInvariant(),
            invoice.Total,
            invoice.PaidTotal,
            invoice.Balance,
            invoice.CustomerId,
            customerName
        );
    }
}
