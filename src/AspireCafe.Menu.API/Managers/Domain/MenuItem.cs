namespace AspireCafe.Menu.API.Managers.Domain;

/// <summary>
/// Domain model representing a menu item. Mapped to/from EF entities,
/// view models (incoming), and service models (outgoing).
/// </summary>
public class MenuItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int PrepTimeMinutes { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
