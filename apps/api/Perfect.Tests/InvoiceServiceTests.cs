using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Domain.Enums;
using Perfect.Infrastructure.Persistence;
using Perfect.Infrastructure.Services;

namespace Perfect.Tests;

public class InvoiceServiceTests : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TestTenantProvider _tenantProvider;
    private readonly TestDateTimeProvider _clock;
    private readonly TestUserContext _userContext;
    private readonly AppDbContext _db;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _tenantProvider = new TestTenantProvider { TenantId = Guid.NewGuid(), TenantSlug = "demo" };
        _clock = new TestDateTimeProvider { UtcNow = new DateTime(2026, 2, 4, 12, 0, 0, DateTimeKind.Utc) };
        _userContext = new TestUserContext { UserId = Guid.NewGuid(), TenantId = _tenantProvider.TenantId };

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options, _tenantProvider);
        _db.Database.EnsureCreated();

        _service = new InvoiceService(_db, new InvoiceStatusCalculator(), _clock, _userContext);
    }

    [Fact]
    public async Task RegisterPayment_Throws_WhenAmountExceedsBalance()
    {
        var tenantId = _tenantProvider.TenantId!.Value;
        var invoice = new Invoice
        {
            TenantId = tenantId,
            Number = "FAC-0001",
            Date = _clock.UtcNow.AddDays(-5),
            PaymentType = PaymentType.Credit,
            CreditDaysApplied = 30,
            DueDate = _clock.UtcNow.AddDays(25),
            Total = 1000,
            PaidTotal = 200,
            Balance = 800,
            Status = InvoiceStatus.Pending,
            CreatedByUserId = _userContext.UserId
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        var act = () => _service.RegisterPaymentAsync(invoice.Id, new PaymentRequest(900, "TRANSFER", null), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>().WithMessage("*exceeds*");
    }

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
