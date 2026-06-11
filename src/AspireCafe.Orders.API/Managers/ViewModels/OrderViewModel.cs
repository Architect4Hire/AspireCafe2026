using System.ComponentModel.DataAnnotations;

namespace AspireCafe.Orders.API.Managers.ViewModels;

public class OrderViewModel
{
    [Range(1, 999)]
    public int TableNumber { get; set; }

    [Required, StringLength(80)]
    public string ServerName { get; set; } = string.Empty;

    [MinLength(1)]
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderItemViewModel
{
    [Required]
    public Guid MenuItemId { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(0.0, 9999.99)]
    public decimal UnitPrice { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; }

    [StringLength(200)]
    public string Notes { get; set; } = string.Empty;
}

public class OrderStatusUpdateViewModel
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
