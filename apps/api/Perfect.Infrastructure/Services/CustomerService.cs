using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Models;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedResult<CustomerResponse>> GetAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        var query = _db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Name.Contains(search) || x.Identification.Contains(search));
        }

        var mapped = query.OrderBy(x => x.Name)
            .Select(c => new CustomerResponse(c.Id, c.Name, c.Identification, c.Phone, c.Email, c.DefaultCreditDays, c.CreditLimit, c.IsActive));
        return await mapped.ToPagedAsync(page, pageSize, ct);
    }

    public async Task<CustomerResponse> CreateAsync(CustomerRequest request, CancellationToken ct)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Identification = request.Identification,
            Phone = request.Phone,
            Email = request.Email,
            DefaultCreditDays = request.DefaultCreditDays,
            CreditLimit = request.CreditLimit,
            IsActive = request.IsActive
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
        return new CustomerResponse(customer.Id, customer.Name, customer.Identification, customer.Phone, customer.Email, customer.DefaultCreditDays, customer.CreditLimit, customer.IsActive);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, CustomerRequest request, CancellationToken ct)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Customer not found", 404);

        customer.Name = request.Name;
        customer.Identification = request.Identification;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.DefaultCreditDays = request.DefaultCreditDays;
        customer.CreditLimit = request.CreditLimit;
        customer.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return new CustomerResponse(customer.Id, customer.Name, customer.Identification, customer.Phone, customer.Email, customer.DefaultCreditDays, customer.CreditLimit, customer.IsActive);
    }

    public async Task<CustomerResponse> UpdateCreditTermsAsync(Guid id, CustomerCreditTermsRequest request, CancellationToken ct)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Customer not found", 404);

        customer.DefaultCreditDays = request.DefaultCreditDays;
        customer.CreditLimit = request.CreditLimit;
        await _db.SaveChangesAsync(ct);
        return new CustomerResponse(customer.Id, customer.Name, customer.Identification, customer.Phone, customer.Email, customer.DefaultCreditDays, customer.CreditLimit, customer.IsActive);
    }

    public async Task<IReadOnlyCollection<CustomerResponse>> GetInactiveAsync(int days, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        var query = from c in _db.Customers
                    join i in _db.Invoices on c.Id equals i.CustomerId into inv
                    let lastDate = inv.OrderByDescending(x => x.Date).Select(x => (DateTime?)x.Date).FirstOrDefault()
                    where !lastDate.HasValue || lastDate.Value < cutoff
                    select new CustomerResponse(c.Id, c.Name, c.Identification, c.Phone, c.Email, c.DefaultCreditDays, c.CreditLimit, c.IsActive);

        return await query.ToListAsync(ct);
    }
}
