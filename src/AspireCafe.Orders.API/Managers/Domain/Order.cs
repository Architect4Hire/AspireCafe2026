namespace AspireCafe.Orders.API.Managers.Domain;

public enum OrderStatus
{
    Pending = 0,
    Submitted = 1,
    Preparing = 2,
    Ready = 3,
    Delivered = 4,
    Cancelled = 9
}

public class Order
{
    public Guid Id { get; set; }
    public int TableNumber { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string Notes { get; set; } = string.Empty;
}
