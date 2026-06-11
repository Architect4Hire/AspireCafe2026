using AspireCafe.Payments.API.Managers.DataContext;
using AspireCafe.Payments.API.Managers.Domain;
using Microsoft.EntityFrameworkCore;

namespace AspireCafe.Payments.API.Managers.Data;

public interface IPaymentDataManager
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Payment>> GetByOrderAsync(Guid orderId, CancellationToken ct);
    Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct);
    Task<Payment> AddAsync(Payment payment, CancellationToken ct);
    Task<Payment?> UpdateStatusAsync(Guid id, PaymentStatus status, string authCode, CancellationToken ct);
}

public class PaymentDataManager(PaymentsDbContext db) : IPaymentDataManager
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Payment>> GetByOrderAsync(Guid orderId, CancellationToken ct)
        => await db.Payments.AsNoTracking().Where(p => p.OrderId == orderId).ToListAsync(ct);

    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct)
        => await db.Payments.AsNoTracking().OrderByDescending(p => p.CreatedUtc).ToListAsync(ct);

    public async Task<Payment> AddAsync(Payment payment, CancellationToken ct)
    {
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);
        return payment;
    }

    public async Task<Payment?> UpdateStatusAsync(Guid id, PaymentStatus status, string authCode, CancellationToken ct)
    {
        var existing = await db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (existing is null) return null;
        existing.Status = status;
        existing.AuthorizationCode = authCode;
        existing.UpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return existing;
    }
}
