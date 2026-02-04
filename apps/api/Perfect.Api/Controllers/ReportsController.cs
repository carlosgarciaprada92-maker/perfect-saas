using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Perfect.Api.Authorization;
using Perfect.Application.Contracts;
using Perfect.Application.Services;

namespace Perfect.Api.Controllers;

[Authorize]
[RequireTenant]
[Route("api/v1/reports")]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reportService;
    private readonly IArService _arService;

    public ReportsController(IReportService reportService, IArService arService)
    {
        _reportService = reportService;
        _arService = arService;
    }

    [HttpGet("sales-summary")]
    [RequirePermission("reports.read")]
    public async Task<IActionResult> SalesSummary([FromQuery] int rangeDays = 30, CancellationToken ct = default)
    {
        var data = await _reportService.GetSalesSummaryAsync(rangeDays, ct);
        return Envelope(data);
    }

    [HttpGet("top-customers")]
    [RequirePermission("reports.read")]
    public async Task<IActionResult> TopCustomers([FromQuery] int rangeDays = 30, [FromQuery] int topN = 10, CancellationToken ct = default)
    {
        var data = await _reportService.GetTopCustomersAsync(rangeDays, topN, ct);
        return EnvelopeCollection(data);
    }

    [HttpGet("customer-activity")]
    [RequirePermission("reports.read")]
    public async Task<IActionResult> CustomerActivity([FromQuery] int inactiveDays = 30, CancellationToken ct = default)
    {
        var data = await _reportService.GetCustomerActivityAsync(inactiveDays, ct);
        return EnvelopeCollection(data);
    }

    [HttpGet("credit-kpis")]
    [RequirePermission("reports.read")]
    public async Task<IActionResult> CreditKpis([FromQuery] int rangeDays = 30, [FromQuery] int thresholdDays = 5, CancellationToken ct = default)
    {
        var data = await _reportService.GetCreditKpisAsync(rangeDays, thresholdDays, ct);
        return Envelope(data);
    }

    [HttpGet("sales-by-day")]
    [RequirePermission("reports.read")]
    public async Task<IActionResult> SalesByDay([FromQuery] int rangeDays = 30, CancellationToken ct = default)
    {
        var data = await _reportService.GetSalesByDayAsync(rangeDays, ct);
        return EnvelopeCollection(data);
    }

    [HttpGet("sales-by-payment-type")]
    [RequirePermission("reports.read")]
    public async Task<IActionResult> SalesByPaymentType([FromQuery] int rangeDays = 30, CancellationToken ct = default)
    {
        var data = await _reportService.GetSalesByPaymentTypeAsync(rangeDays, ct);
        return EnvelopeCollection(data);
    }

    [HttpGet("overdue-customers")]
    [RequirePermission("reports.read")]
    public async Task<IActionResult> OverdueCustomers([FromQuery] int topN = 20, CancellationToken ct = default)
    {
        var data = await _reportService.GetOverdueCustomersAsync(topN, ct);
        return EnvelopeCollection(data);
    }

    [HttpGet("export")]
    [RequirePermission("reports.read")]
    public async Task<IActionResult> Export(
        [FromQuery] string type,
        [FromQuery] string format = "csv",
        [FromQuery] int rangeDays = 30,
        [FromQuery] int thresholdDays = 5,
        [FromQuery] bool includePaid = false,
        [FromQuery] Guid? customerId = null,
        CancellationToken ct = default)
    {
        if (!string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { data = (object?)null, meta = (object?)null, errors = new[] { new { code = "validation_error", message = "Only CSV export is supported" } } });
        }

        var loweredType = type?.Trim().ToLowerInvariant();
        return loweredType switch
        {
            "cartera" => await ExportCarteraCsv(rangeDays, thresholdDays, includePaid, customerId, ct),
            "ventas" => await ExportVentasCsv(rangeDays, ct),
            "clientes" => await ExportClientesCsv(rangeDays, ct),
            _ => BadRequest(new { data = (object?)null, meta = (object?)null, errors = new[] { new { code = "validation_error", message = "Unknown export type" } } })
        };
    }

    private async Task<FileContentResult> ExportCarteraCsv(int rangeDays, int thresholdDays, bool includePaid, Guid? customerId, CancellationToken ct)
    {
        var data = await _arService.GetOpenItemsAsync(1, 5000, null, includePaid, customerId, thresholdDays, null, rangeDays, ct);
        var csv = new StringBuilder();
        csv.AppendLine("invoice_number,customer,due_date,days_to_due,days_overdue,total,balance,status");

        foreach (var row in data.Items)
        {
            csv.AppendLine(string.Join(',',
                Csv(row.InvoiceNumber),
                Csv(row.CustomerName),
                row.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                row.DaysToDue,
                row.DaysOverdue,
                row.Total.ToString("0.##", CultureInfo.InvariantCulture),
                row.Balance.ToString("0.##", CultureInfo.InvariantCulture),
                row.Status));
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "cartera.csv");
    }

    private async Task<FileContentResult> ExportVentasCsv(int rangeDays, CancellationToken ct)
    {
        var byDay = await _reportService.GetSalesByDayAsync(rangeDays, ct);
        var byPaymentType = await _reportService.GetSalesByPaymentTypeAsync(rangeDays, ct);

        var csv = new StringBuilder();
        csv.AppendLine("section,date,payment_type,count,total");

        foreach (var day in byDay)
        {
            csv.AppendLine(string.Join(',', "daily", day.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), "", day.Count, day.Total.ToString("0.##", CultureInfo.InvariantCulture)));
        }

        foreach (var paymentType in byPaymentType)
        {
            csv.AppendLine(string.Join(',', "payment_type", "", paymentType.PaymentType, paymentType.Count, paymentType.Total.ToString("0.##", CultureInfo.InvariantCulture)));
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "ventas.csv");
    }

    private async Task<FileContentResult> ExportClientesCsv(int rangeDays, CancellationToken ct)
    {
        var top = await _reportService.GetTopCustomersAsync(rangeDays, 100, ct);
        var inactive = await _reportService.GetCustomerActivityAsync(30, ct);

        var csv = new StringBuilder();
        csv.AppendLine("section,customer,total,count,avg_ticket,credit_pct,cash_pct,last_purchase,days_inactive");

        foreach (var row in top)
        {
            csv.AppendLine(string.Join(',',
                "top_customers",
                Csv(row.CustomerName),
                row.Total.ToString("0.##", CultureInfo.InvariantCulture),
                row.Count,
                row.AvgTicket.ToString("0.##", CultureInfo.InvariantCulture),
                row.CreditPct.ToString("0.##", CultureInfo.InvariantCulture),
                row.CashPct.ToString("0.##", CultureInfo.InvariantCulture),
                "",
                ""));
        }

        foreach (var row in inactive)
        {
            csv.AppendLine(string.Join(',',
                "inactive_customers",
                Csv(row.CustomerName),
                row.Total.ToString("0.##", CultureInfo.InvariantCulture),
                "",
                "",
                "",
                "",
                row.LastPurchase?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "",
                row.DaysInactive));
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "clientes.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}