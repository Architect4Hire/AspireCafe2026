using AspireCafe.Menu.API.Managers.Domain;
using AspireCafe.Menu.API.Managers.ServiceModels;
using AspireCafe.Menu.API.Managers.ViewModels;

namespace AspireCafe.Menu.API.Managers.Extensions;

/// <summary>
/// Custom mapping/transformation extensions. Hand-rolled mappers keep
/// transformations explicit and easy to review (no AutoMapper magic).
/// </summary>
public static class MenuItemMappingExtensions
{
    public static MenuItem ToDomain(this MenuItemViewModel vm) => new()
    {
        Id = Guid.NewGuid(),
        Name = vm.Name.Trim(),
        Description = vm.Description.Trim(),
        Price = decimal.Round(vm.Price, 2),
        Category = vm.Category.Trim(),
        ImageUrl = vm.ImageUrl.Trim(),
        IsAvailable = vm.IsAvailable,
        PrepTimeMinutes = vm.PrepTimeMinutes,
        CreatedUtc = DateTime.UtcNow,
        UpdatedUtc = DateTime.UtcNow,
    };

    public static void ApplyUpdate(this MenuItem domain, MenuItemViewModel vm)
    {
        domain.Name = vm.Name.Trim();
        domain.Description = vm.Description.Trim();
        domain.Price = decimal.Round(vm.Price, 2);
        domain.Category = vm.Category.Trim();
        domain.ImageUrl = vm.ImageUrl.Trim();
        domain.IsAvailable = vm.IsAvailable;
        domain.PrepTimeMinutes = vm.PrepTimeMinutes;
        domain.UpdatedUtc = DateTime.UtcNow;
    }

    public static MenuItemServiceModel ToServiceModel(this MenuItem domain) => new()
    {
        Id = domain.Id,
        Name = domain.Name,
        Description = domain.Description,
        Price = domain.Price,
        Category = domain.Category,
        ImageUrl = domain.ImageUrl,
        IsAvailable = domain.IsAvailable,
        PrepTimeMinutes = domain.PrepTimeMinutes,
    };

    public static IEnumerable<MenuItemServiceModel> ToServiceModels(this IEnumerable<MenuItem> items)
        => items.Select(i => i.ToServiceModel());
}
