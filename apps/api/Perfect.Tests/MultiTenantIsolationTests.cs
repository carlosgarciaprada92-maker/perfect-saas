using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Domain.Enums;
using Perfect.Infrastructure.Persistence;
using Perfect.Infrastructure.Services;

namespace Perfect.Tests;

public class MultiTenantIsolationTests : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TestTenantProvider _tenantProvider;
    private readonly TestDateTimeProvider _clock;
    private readonly TestUserContext _userContext;
    private readonly AppDbContext _db;

    public MultiTenantIsolationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _tenantProvider = new TestTenantProvider();
        _clock = new TestDateTimeProvider { UtcNow = new DateTime(2026, 2, 4, 12, 0, 0, DateTimeKind.Utc) };
        _userContext = new TestUserContext { UserId = Guid.NewGuid() };

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options, _tenantProvider);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task QueryFilter_ReturnsOnlyCurrentTenantData()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        _db.Invoices.AddRange(
            new Invoice
            {
                TenantId = tenantA,
                Number = "A-001",
                Date = _clock.UtcNow,
                PaymentType = PaymentType.Cash,
                CreditDaysApplied = 0,
                DueDate = _clock.UtcNow,
                Total = 100,
                PaidTotal = 100,
                Balance = 0,
                Status = InvoiceStatus.Paid
            },
            new Invoice
            {
                TenantId = tenantB,
                Number = "B-001",
                Date = _clock.UtcNow,
                PaymentType = PaymentType.Credit,
                CreditDaysApplied = 15,
                DueDate = _clock.UtcNow.AddDays(15),
                Total = 300,
                PaidTotal = 0,
                Balance = 300,
                Status = InvoiceStatus.Pending
            });

        await _db.SaveChangesAsync();

        _tenantProvider.TenantId = tenantA;
        var tenantAInvoices = await _db.Invoices.ToListAsync();

        tenantAInvoices.Should().HaveCount(1);
        tenantAInvoices[0].Number.Should().Be("A-001");
    }

    [Fact]
    public async Task ServiceCannotReadInvoiceFromAnotherTenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var invoiceB = new Invoice
        {
            TenantId = tenantB,
            Number = "B-123",
            Date = _clock.UtcNow,
            PaymentType = PaymentType.Credit,
            CreditDaysApplied = 15,
            DueDate = _clock.UtcNow.AddDays(15),
            Total = 500,
            PaidTotal = 0,
            Balance = 500,
            Status = InvoiceStatus.Pending
        };

        _db.Invoices.Add(invoiceB);
        await _db.SaveChangesAsync();

        _tenantProvider.TenantId = tenantA;
        var service = new InvoiceService(_db, new InvoiceStatusCalculator(), _clock, _userContext);

        var act = () => service.GetInvoiceAsync(invoiceB.Id, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(ex => ex.Code == ErrorCodes.NotFound);
    }

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}