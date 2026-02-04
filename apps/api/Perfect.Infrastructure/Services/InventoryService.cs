using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Models;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Domain.Enums;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;

    public InventoryService(AppDbContext db)
    {
        _db = db;
    }

    public Task<InventoryMovementResponse> InventoryInAsync(InventoryMovementRequest request, CancellationToken ct)
        => CreateMovementAsync(request, InventoryMovementType.In, ct);

    public Task<InventoryMovementResponse> InventoryAdjustAsync(InventoryMovementRequest request, CancellationToken ct)
        => CreateMovementAsync(request, InventoryMovementType.Adjust, ct);

    public async Task<PaginatedResult<InventoryMovementResponse>> GetMovementsAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = _db.InventoryMovements
            .Include(x => x.Product)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new InventoryMovementResponse(x.Id, x.ProductId, x.Product.Name, x.Type.ToString().ToUpperInvariant(), x.Quantity, x.Reason, x.CreatedAt));

        return await query.ToPagedAsync(page, pageSize, ct);
    }

    private async Task<InventoryMovementResponse> CreateMovementAsync(InventoryMovementRequest request, InventoryMovementType type, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Product not found", 404);

        var movement = new InventoryMovement
        {
            ProductId = request.ProductId,
            Type = type,
            Quantity = request.Quantity,
            Reason = request.Reason
        };

        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync(ct);

        return new InventoryMovementResponse(movement.Id, movement.ProductId, product.Name, movement.Type.ToString().ToUpperInvariant(), movement.Quantity, movement.Reason, movement.CreatedAt);
    }
}
