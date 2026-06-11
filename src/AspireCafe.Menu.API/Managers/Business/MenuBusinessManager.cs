using AspireCafe.Menu.API.Managers.Data;
using AspireCafe.Menu.API.Managers.Domain;
using AspireCafe.Menu.API.Managers.Extensions;
using AspireCafe.Menu.API.Managers.ServiceModels;
using AspireCafe.Menu.API.Managers.ViewModels;

namespace AspireCafe.Menu.API.Managers.Business;

public interface IMenuBusinessManager
{
    Task<IEnumerable<MenuItemServiceModel>> GetAllAsync(CancellationToken ct);
    Task<IEnumerable<MenuItemServiceModel>> GetByCategoryAsync(string category, CancellationToken ct);
    Task<MenuItemServiceModel?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<MenuItemServiceModel> CreateAsync(MenuItemViewModel vm, CancellationToken ct);
    Task<MenuItemServiceModel?> UpdateAsync(Guid id, MenuItemViewModel vm, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

/// <summary>
/// BUSINESS layer — orchestrates rules, mapping, and the data layer.
/// Has zero knowledge of HTTP or EF specifics.
/// </summary>
public class MenuBusinessManager(IMenuDataManager data) : IMenuBusinessManager
{
    public async Task<IEnumerable<MenuItemServiceModel>> GetAllAsync(CancellationToken ct)
        => (await data.GetAllAsync(ct)).ToServiceModels();

    public async Task<IEnumerable<MenuItemServiceModel>> GetByCategoryAsync(string category, CancellationToken ct)
        => (await data.GetByCategoryAsync(category, ct)).ToServiceModels();

    public async Task<MenuItemServiceModel?> GetByIdAsync(Guid id, CancellationToken ct)
        => (await data.GetByIdAsync(id, ct))?.ToServiceModel();

    public async Task<MenuItemServiceModel> CreateAsync(MenuItemViewModel vm, CancellationToken ct)
    {
        var domain = vm.ToDomain();
        var saved = await data.AddAsync(domain, ct);
        return saved.ToServiceModel();
    }

    public async Task<MenuItemServiceModel?> UpdateAsync(Guid id, MenuItemViewModel vm, CancellationToken ct)
    {
        var existing = await data.GetByIdAsync(id, ct);
        if (existing is null) return null;
        existing.ApplyUpdate(vm);
        var updated = await data.UpdateAsync(existing, ct);
        return updated?.ToServiceModel();
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) => data.DeleteAsync(id, ct);
}
