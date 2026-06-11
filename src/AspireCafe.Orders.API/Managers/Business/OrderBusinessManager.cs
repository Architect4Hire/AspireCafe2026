using AspireCafe.Orders.API.Managers.Data;
using AspireCafe.Orders.API.Managers.Domain;
using AspireCafe.Orders.API.Managers.Extensions;
using AspireCafe.Orders.API.Managers.ServiceModels;
using AspireCafe.Orders.API.Managers.ViewModels;

namespace AspireCafe.Orders.API.Managers.Business;

public interface IOrderBusinessManager
{
    Task<IEnumerable<OrderServiceModel>> GetAllAsync(CancellationToken ct);
    Task<IEnumerable<OrderServiceModel>> GetByTableAsync(int tableNumber, CancellationToken ct);
    Task<IEnumerable<OrderServiceModel>> GetActiveAsync(CancellationToken ct);
    Task<OrderServiceModel?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<OrderServiceModel> SubmitAsync(OrderViewModel vm, CancellationToken ct);
    Task<OrderServiceModel?> UpdateStatusAsync(Guid id, string status, CancellationToken ct);
}

public class OrderBusinessManager(IOrderDataManager data) : IOrderBusinessManager
{
    public async Task<IEnumerable<OrderServiceModel>> GetAllAsync(CancellationToken ct)
        => (await data.GetAllAsync(ct)).ToServiceModels();

    public async Task<IEnumerable<OrderServiceModel>> GetByTableAsync(int tableNumber, CancellationToken ct)
        => (await data.GetByTableAsync(tableNumber, ct)).ToServiceModels();

    public async Task<IEnumerable<OrderServiceModel>> GetActiveAsync(CancellationToken ct)
        => (await data.GetActiveAsync(ct)).ToServiceModels();

    public async Task<OrderServiceModel?> GetByIdAsync(Guid id, CancellationToken ct)
        => (await data.GetByIdAsync(id, ct))?.ToServiceModel();

    public async Task<OrderServiceModel> SubmitAsync(OrderViewModel vm, CancellationToken ct)
    {
        if (vm.Items.Count == 0)
            throw new InvalidOperationException("Order must contain at least one item.");

        var domain = vm.ToDomain();
        var saved = await data.AddAsync(domain, ct);
        return saved.ToServiceModel();
    }

    public async Task<OrderServiceModel?> UpdateStatusAsync(Guid id, string status, CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed))
            throw new InvalidOperationException($"Invalid order status: {status}");

        var updated = await data.UpdateStatusAsync(id, parsed, ct);
        return updated?.ToServiceModel();
    }
}
