using System.ComponentModel.DataAnnotations;

namespace AspireCafe.Menu.API.Managers.ViewModels;

/// <summary>
/// Incoming view model used to create/update a menu item from external callers.
/// </summary>
public class MenuItemViewModel
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.0, 9999.99)]
    public decimal Price { get; set; }

    [Required, StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    [Range(0, 240)]
    public int PrepTimeMinutes { get; set; }
}
