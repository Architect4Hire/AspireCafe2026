using AspireCafe.Menu.API.Managers.DataContext;
using AspireCafe.Menu.API.Managers.Domain;
using Microsoft.EntityFrameworkCore;

namespace AspireCafe.Menu.API.Managers.Data;

public interface IMenuDataManager
{
    Task<IReadOnlyList<MenuItem>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<MenuItem>> GetByCategoryAsync(string category, CancellationToken ct);
    Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<MenuItem> AddAsync(MenuItem item, CancellationToken ct);
    Task<MenuItem?> UpdateAsync(MenuItem item, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

/// <summary>
/// DATA layer — only talks to the database. No business rules here.
/// </summary>
public class MenuDataManager(MenuDbContext db) : IMenuDataManager
{
    public async Task<IReadOnlyList<MenuItem>> GetAllAsync(CancellationToken ct)
        => await db.MenuItems.AsNoTracking().OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<MenuItem>> GetByCategoryAsync(string category, CancellationToken ct)
        => await db.MenuItems.AsNoTracking()
            .Where(m => m.Category == category)
            .OrderBy(m => m.Name)
            .ToListAsync(ct);

    public Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.MenuItems.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<MenuItem> AddAsync(MenuItem item, CancellationToken ct)
    {
        db.MenuItems.Add(item);
        await db.SaveChangesAsync(ct);
        return item;
    }

    public async Task<MenuItem?> UpdateAsync(MenuItem item, CancellationToken ct)
    {
        var existing = await db.MenuItems.FirstOrDefaultAsync(m => m.Id == item.Id, ct);
        if (existing is null) return null;
        db.Entry(existing).CurrentValues.SetValues(item);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var existing = await db.MenuItems.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (existing is null) return false;
        db.MenuItems.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
