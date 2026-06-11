using AspireCafe.Orders.API.Managers.DataContext;
using AspireCafe.Orders.API.Managers.Domain;
using Microsoft.EntityFrameworkCore;

namespace AspireCafe.Orders.API.Managers.Data;

public interface IOrderDataManager
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<Order>> GetByTableAsync(int tableNumber, CancellationToken ct);
    Task<IReadOnlyList<Order>> GetActiveAsync(CancellationToken ct);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Order> AddAsync(Order order, CancellationToken ct);
    Task<Order?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct);
}

public class OrderDataManager(OrdersDbContext db) : IOrderDataManager
{
    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct)
        => await db.Orders.AsNoTracking().Include(o => o.Items).OrderByDescending(o => o.CreatedUtc).ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> GetByTableAsync(int tableNumber, CancellationToken ct)
        => await db.Orders.AsNoTracking().Include(o => o.Items)
            .Where(o => o.TableNumber == tableNumber)
            .OrderByDescending(o => o.CreatedUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> GetActiveAsync(CancellationToken ct)
        => await db.Orders.AsNoTracking().Include(o => o.Items)
            .Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
            .OrderBy(o => o.CreatedUtc)
            .ToListAsync(ct);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<Order> AddAsync(Order order, CancellationToken ct)
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<Order?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return null;
        order.Status = status;
        order.UpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return order;
    }
}
