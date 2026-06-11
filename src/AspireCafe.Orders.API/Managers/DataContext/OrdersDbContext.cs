using AspireCafe.Orders.API.Managers.Domain;
using Microsoft.EntityFrameworkCore;

namespace AspireCafe.Orders.API.Managers.DataContext;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(x => x.Id);
            e.Property(x => x.ServerName).HasMaxLength(80);
            e.Property(x => x.Subtotal).HasPrecision(10, 2);
            e.Property(x => x.TaxAmount).HasPrecision(10, 2);
            e.Property(x => x.Total).HasPrecision(10, 2);
            e.Property(x => x.Status).HasConversion<int>();
            e.HasIndex(x => x.TableNumber);
            e.HasIndex(x => x.Status);
            e.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120);
            e.Property(x => x.UnitPrice).HasPrecision(8, 2);
            e.Property(x => x.Notes).HasMaxLength(200);
        });
    }

    public static async Task EnsureCreatedAsync(OrdersDbContext db, CancellationToken ct = default)
    {
        await db.Database.EnsureCreatedAsync(ct);
    }
}
