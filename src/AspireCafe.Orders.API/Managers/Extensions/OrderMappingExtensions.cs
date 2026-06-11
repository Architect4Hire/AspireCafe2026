using AspireCafe.Orders.API.Managers.Domain;
using AspireCafe.Orders.API.Managers.ServiceModels;
using AspireCafe.Orders.API.Managers.ViewModels;

namespace AspireCafe.Orders.API.Managers.Extensions;

public static class OrderMappingExtensions
{
    private const decimal TaxRate = 0.07m; // POC tax rate (7%)

    public static Order ToDomain(this OrderViewModel vm)
    {
        var now = DateTime.UtcNow;
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            TableNumber = vm.TableNumber,
            ServerName = vm.ServerName.Trim(),
            Status = OrderStatus.Submitted,
            CreatedUtc = now,
            UpdatedUtc = now,
            Items = vm.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                MenuItemId = i.MenuItemId,
                Name = i.Name.Trim(),
                UnitPrice = decimal.Round(i.UnitPrice, 2),
                Quantity = i.Quantity,
                Notes = i.Notes?.Trim() ?? string.Empty
            }).ToList()
        };

        order.RecalculateTotals();
        return order;
    }

    public static void RecalculateTotals(this Order order)
    {
        order.Subtotal = decimal.Round(order.Items.Sum(i => i.UnitPrice * i.Quantity), 2);
        order.TaxAmount = decimal.Round(order.Subtotal * TaxRate, 2);
        order.Total = decimal.Round(order.Subtotal + order.TaxAmount, 2);
    }

    public static OrderServiceModel ToServiceModel(this Order o) => new()
    {
        Id = o.Id,
        TableNumber = o.TableNumber,
        ServerName = o.ServerName,
        Status = o.Status.ToString(),
        Subtotal = o.Subtotal,
        TaxAmount = o.TaxAmount,
        Total = o.Total,
        CreatedUtc = o.CreatedUtc,
        UpdatedUtc = o.UpdatedUtc,
        Items = o.Items.Select(i => new OrderItemServiceModel
        {
            Id = i.Id,
            MenuItemId = i.MenuItemId,
            Name = i.Name,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            Notes = i.Notes
        }).ToList()
    };

    public static IEnumerable<OrderServiceModel> ToServiceModels(this IEnumerable<Order> orders)
        => orders.Select(o => o.ToServiceModel());
}
