using AspireCafe.Orders.API.Managers.Business;
using AspireCafe.Orders.API.Managers.ServiceModels;
using AspireCafe.Orders.API.Managers.ViewModels;

namespace AspireCafe.Orders.API.Managers.Facades;

public interface IOrderFacade
{
    Task<IEnumerable<OrderServiceModel>> GetOrdersAsync(CancellationToken ct);
    Task<IEnumerable<OrderServiceModel>> GetActiveAsync(CancellationToken ct);
    Task<IEnumerable<OrderServiceModel>> GetByTableAsync(int tableNumber, CancellationToken ct);
    Task<OrderServiceModel?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<OrderServiceModel> SubmitOrderAsync(OrderViewModel vm, CancellationToken ct);
    Task<OrderServiceModel?> ChangeStatusAsync(Guid id, string status, CancellationToken ct);
}

public class OrderFacade(IOrderBusinessManager business) : IOrderFacade
{
    public Task<IEnumerable<OrderServiceModel>> GetOrdersAsync(CancellationToken ct) => business.GetAllAsync(ct);
    public Task<IEnumerable<OrderServiceModel>> GetActiveAsync(CancellationToken ct) => business.GetActiveAsync(ct);
    public Task<IEnumerable<OrderServiceModel>> GetByTableAsync(int tableNumber, CancellationToken ct) => business.GetByTableAsync(tableNumber, ct);
    public Task<OrderServiceModel?> GetByIdAsync(Guid id, CancellationToken ct) => business.GetByIdAsync(id, ct);
    public Task<OrderServiceModel> SubmitOrderAsync(OrderViewModel vm, CancellationToken ct) => business.SubmitAsync(vm, ct);
    public Task<OrderServiceModel?> ChangeStatusAsync(Guid id, string status, CancellationToken ct) => business.UpdateStatusAsync(id, status, ct);
}
