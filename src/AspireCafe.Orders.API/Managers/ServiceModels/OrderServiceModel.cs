namespace AspireCafe.Orders.API.Managers.ServiceModels;

public class OrderServiceModel
{
    public Guid Id { get; set; }
    public int TableNumber { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public List<OrderItemServiceModel> Items { get; set; } = new();
}

public class OrderItemServiceModel
{
    public Guid Id { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => decimal.Round(UnitPrice * Quantity, 2);
    public string Notes { get; set; } = string.Empty;
}
