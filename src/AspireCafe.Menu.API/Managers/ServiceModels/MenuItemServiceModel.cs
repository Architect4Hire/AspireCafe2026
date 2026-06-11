namespace AspireCafe.Menu.API.Managers.ServiceModels;

/// <summary>
/// Outgoing service model returned to callers. Decoupled from the domain
/// so internal property changes never break clients.
/// </summary>
public class MenuItemServiceModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int PrepTimeMinutes { get; set; }
}
