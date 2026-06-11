using AspireCafe.Menu.API.Managers.Business;
using AspireCafe.Menu.API.Managers.ServiceModels;
using AspireCafe.Menu.API.Managers.ViewModels;

namespace AspireCafe.Menu.API.Managers.Facades;

public interface IMenuFacade
{
    Task<IEnumerable<MenuItemServiceModel>> GetMenuAsync(CancellationToken ct);
    Task<IEnumerable<MenuItemServiceModel>> GetByCategoryAsync(string category, CancellationToken ct);
    Task<MenuItemServiceModel?> GetItemAsync(Guid id, CancellationToken ct);
    Task<MenuItemServiceModel> AddItemAsync(MenuItemViewModel vm, CancellationToken ct);
    Task<MenuItemServiceModel?> UpdateItemAsync(Guid id, MenuItemViewModel vm, CancellationToken ct);
    Task<bool> RemoveItemAsync(Guid id, CancellationToken ct);
}

/// <summary>
/// FACADE — the only thing controllers see. Stable contract for callers,
/// even if internal business/data composition changes.
/// </summary>
public class MenuFacade(IMenuBusinessManager business) : IMenuFacade
{
    public Task<IEnumerable<MenuItemServiceModel>> GetMenuAsync(CancellationToken ct) => business.GetAllAsync(ct);
    public Task<IEnumerable<MenuItemServiceModel>> GetByCategoryAsync(string category, CancellationToken ct) => business.GetByCategoryAsync(category, ct);
    public Task<MenuItemServiceModel?> GetItemAsync(Guid id, CancellationToken ct) => business.GetByIdAsync(id, ct);
    public Task<MenuItemServiceModel> AddItemAsync(MenuItemViewModel vm, CancellationToken ct) => business.CreateAsync(vm, ct);
    public Task<MenuItemServiceModel?> UpdateItemAsync(Guid id, MenuItemViewModel vm, CancellationToken ct) => business.UpdateAsync(id, vm, ct);
    public Task<bool> RemoveItemAsync(Guid id, CancellationToken ct) => business.DeleteAsync(id, ct);
}
