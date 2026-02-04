using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Models;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedResult<ProductResponse>> GetAsync(int page, int pageSize, string? search, string? sort, string? order, CancellationToken ct)
    {
        var query = _db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Sku.Contains(search));
        }

        query = (sort?.ToLowerInvariant(), order?.ToLowerInvariant()) switch
        {
            ("name", "desc") => query.OrderByDescending(x => x.Name),
            ("price", "desc") => query.OrderByDescending(x => x.Price),
            ("price", _) => query.OrderBy(x => x.Price),
            (_, "desc") => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderBy(x => x.Name)
        };

        var mapped = query.Select(p => new ProductResponse(p.Id, p.Sku, p.Name, p.Description, p.Price, p.Cost, p.MinStock, p.IsActive));
        return await mapped.ToPagedAsync(page, pageSize, ct);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct)
    {
        var exists = await _db.Products.AnyAsync(p => p.Sku == request.Sku, ct);
        if (exists)
        {
            throw new AppException(ErrorCodes.Conflict, "SKU already exists", 409);
        }

        var entity = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Cost = request.Cost,
            MinStock = request.MinStock,
            IsActive = request.IsActive
        };
        _db.Products.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new ProductResponse(entity.Id, entity.Sku, entity.Name, entity.Description, entity.Price, entity.Cost, entity.MinStock, entity.IsActive);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Product not found", 404);

        entity.Sku = request.Sku;
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Price = request.Price;
        entity.Cost = request.Cost;
        entity.MinStock = request.MinStock;
        entity.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);

        return new ProductResponse(entity.Id, entity.Sku, entity.Name, entity.Description, entity.Price, entity.Cost, entity.MinStock, entity.IsActive);
    }

    public async Task<ProductResponse> UpdateStatusAsync(Guid id, bool isActive, CancellationToken ct)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Product not found", 404);
        entity.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return new ProductResponse(entity.Id, entity.Sku, entity.Name, entity.Description, entity.Price, entity.Cost, entity.MinStock, entity.IsActive);
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetLowStockAsync(int? threshold, CancellationToken ct)
    {
        var stockQuery = _db.InventoryMovements
            .GroupBy(m => m.ProductId)
            .Select(g => new { ProductId = g.Key, Stock = g.Sum(x => x.Type == Domain.Enums.InventoryMovementType.In ? x.Quantity : x.Type == Domain.Enums.InventoryMovementType.Out ? -x.Quantity : 0) });

        var lowStock = await (from p in _db.Products
                              join s in stockQuery on p.Id equals s.ProductId into stockJoined
                              from stock in stockJoined.DefaultIfEmpty()
                              where (stock == null ? 0 : stock.Stock) <= (threshold ?? p.MinStock)
                              select new ProductResponse(p.Id, p.Sku, p.Name, p.Description, p.Price, p.Cost, p.MinStock, p.IsActive))
            .ToListAsync(ct);

        return lowStock;
    }
}
