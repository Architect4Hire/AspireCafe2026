using AspireCafe.Payments.API.Managers.Data;
using AspireCafe.Payments.API.Managers.Domain;
using AspireCafe.Payments.API.Managers.Extensions;
using AspireCafe.Payments.API.Managers.ServiceModels;
using AspireCafe.Payments.API.Managers.ViewModels;

namespace AspireCafe.Payments.API.Managers.Business;

public interface IPaymentBusinessManager
{
    Task<PaymentServiceModel> ProcessAsync(PaymentViewModel vm, CancellationToken ct);
    Task<PaymentServiceModel?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<PaymentServiceModel>> GetByOrderAsync(Guid orderId, CancellationToken ct);
    Task<IEnumerable<PaymentServiceModel>> GetAllAsync(CancellationToken ct);
    TipSuggestionServiceModel GetTipSuggestions(decimal subtotal);
}

public class PaymentBusinessManager(IPaymentDataManager data) : IPaymentBusinessManager
{
    public async Task<PaymentServiceModel> ProcessAsync(PaymentViewModel vm, CancellationToken ct)
    {
        var domain = vm.ToDomain();
        var saved = await data.AddAsync(domain, ct);

        // POC: simulate processor authorization. Real impl would call Stripe/Square/etc.
        var auth = GenerateAuthCode();
        var captured = await data.UpdateStatusAsync(saved.Id, PaymentStatus.Captured, auth, ct);
        return captured!.ToServiceModel();
    }

    public async Task<PaymentServiceModel?> GetByIdAsync(Guid id, CancellationToken ct)
        => (await data.GetByIdAsync(id, ct))?.ToServiceModel();

    public async Task<IEnumerable<PaymentServiceModel>> GetByOrderAsync(Guid orderId, CancellationToken ct)
        => (await data.GetByOrderAsync(orderId, ct)).Select(p => p.ToServiceModel());

    public async Task<IEnumerable<PaymentServiceModel>> GetAllAsync(CancellationToken ct)
        => (await data.GetAllAsync(ct)).Select(p => p.ToServiceModel());

    public TipSuggestionServiceModel GetTipSuggestions(decimal subtotal)
        => PaymentMappingExtensions.BuildTipSuggestions(subtotal);

    private static string GenerateAuthCode()
        => "AUTH-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
}
