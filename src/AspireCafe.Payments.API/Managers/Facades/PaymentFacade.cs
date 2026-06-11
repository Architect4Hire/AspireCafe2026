using AspireCafe.Payments.API.Managers.Business;
using AspireCafe.Payments.API.Managers.ServiceModels;
using AspireCafe.Payments.API.Managers.ViewModels;

namespace AspireCafe.Payments.API.Managers.Facades;

public interface IPaymentFacade
{
    Task<PaymentServiceModel> ProcessPaymentAsync(PaymentViewModel vm, CancellationToken ct);
    Task<PaymentServiceModel?> GetPaymentAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<PaymentServiceModel>> GetPaymentsByOrderAsync(Guid orderId, CancellationToken ct);
    Task<IEnumerable<PaymentServiceModel>> GetAllAsync(CancellationToken ct);
    TipSuggestionServiceModel GetTipSuggestions(decimal subtotal);
}

public class PaymentFacade(IPaymentBusinessManager business) : IPaymentFacade
{
    public Task<PaymentServiceModel> ProcessPaymentAsync(PaymentViewModel vm, CancellationToken ct) => business.ProcessAsync(vm, ct);
    public Task<PaymentServiceModel?> GetPaymentAsync(Guid id, CancellationToken ct) => business.GetByIdAsync(id, ct);
    public Task<IEnumerable<PaymentServiceModel>> GetPaymentsByOrderAsync(Guid orderId, CancellationToken ct) => business.GetByOrderAsync(orderId, ct);
    public Task<IEnumerable<PaymentServiceModel>> GetAllAsync(CancellationToken ct) => business.GetAllAsync(ct);
    public TipSuggestionServiceModel GetTipSuggestions(decimal subtotal) => business.GetTipSuggestions(subtotal);
}
