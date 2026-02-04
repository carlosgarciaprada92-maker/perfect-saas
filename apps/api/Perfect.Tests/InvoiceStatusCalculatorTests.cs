using FluentAssertions;
using Perfect.Application.Services;
using Perfect.Domain.Enums;

namespace Perfect.Tests;

public class InvoiceStatusCalculatorTests
{
    private readonly InvoiceStatusCalculator _calculator = new();

    [Fact]
    public void ResolveDueDate_UsesInvoiceDate_ForCash()
    {
        var invoiceDate = new DateTime(2026, 2, 4, 10, 0, 0, DateTimeKind.Utc);

        var dueDate = _calculator.ResolveDueDate(invoiceDate, PaymentType.Cash, 30);

        dueDate.Should().Be(invoiceDate);
    }

    [Fact]
    public void ResolveDueDate_AddsCreditDays_ForCredit()
    {
        var invoiceDate = new DateTime(2026, 2, 4, 10, 0, 0, DateTimeKind.Utc);

        var dueDate = _calculator.ResolveDueDate(invoiceDate, PaymentType.Credit, 15);

        dueDate.Should().Be(invoiceDate.AddDays(15));
    }

    [Theory]
    [InlineData(0, "Paid")]
    [InlineData(150, "Overdue")]
    [InlineData(150, "DueSoon")]
    [InlineData(150, "Pending")]
    public void ResolveStatus_CoversExpectedStates(decimal balance, string expected)
    {
        var now = new DateTime(2026, 2, 4, 0, 0, 0, DateTimeKind.Utc);
        var dueDate = expected switch
        {
            "Overdue" => now.AddDays(-1),
            "DueSoon" => now.AddDays(3),
            _ => now.AddDays(20)
        };

        var status = _calculator.ResolveStatus(balance, dueDate, now, 5);

        status.ToString().Should().Be(expected);
    }
}