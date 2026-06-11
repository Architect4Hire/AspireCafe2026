using AspireCafe.Payments.API.Managers.Domain;
using Microsoft.EntityFrameworkCore;

namespace AspireCafe.Payments.API.Managers.DataContext;

public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("Payments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Subtotal).HasPrecision(10, 2);
            e.Property(x => x.TaxAmount).HasPrecision(10, 2);
            e.Property(x => x.TipAmount).HasPrecision(10, 2);
            e.Property(x => x.TipPercent).HasPrecision(5, 2);
            e.Property(x => x.Total).HasPrecision(10, 2);
            e.Property(x => x.Method).HasConversion<int>();
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.Last4).HasMaxLength(4);
            e.Property(x => x.AuthorizationCode).HasMaxLength(32);
            e.HasIndex(x => x.OrderId);
            e.HasIndex(x => x.TableNumber);
        });
    }

    public static async Task EnsureCreatedAsync(PaymentsDbContext db, CancellationToken ct = default)
        => await db.Database.EnsureCreatedAsync(ct);
}
