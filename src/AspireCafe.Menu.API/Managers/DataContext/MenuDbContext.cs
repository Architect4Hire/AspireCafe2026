using AspireCafe.Menu.API.Managers.Domain;
using Microsoft.EntityFrameworkCore;

namespace AspireCafe.Menu.API.Managers.DataContext;

public class MenuDbContext(DbContextOptions<MenuDbContext> options) : DbContext(options)
{
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>(e =>
        {
            e.ToTable("MenuItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Category).IsRequired().HasMaxLength(50);
            e.Property(x => x.ImageUrl).HasMaxLength(500);
            e.Property(x => x.Price).HasPrecision(8, 2);
            e.HasIndex(x => x.Category);
        });
    }

    public static async Task SeedAsync(MenuDbContext db, CancellationToken ct = default)
    {
        await db.Database.EnsureCreatedAsync(ct);
        if (await db.MenuItems.AnyAsync(ct)) return;

        var now = DateTime.UtcNow;
        db.MenuItems.AddRange(
            new MenuItem { Id = Guid.NewGuid(), Name = "Espresso", Description = "Double shot of rich espresso", Price = 3.50m, Category = "Coffee", ImageUrl = "/assets/img/espresso.jpg", IsAvailable = true, PrepTimeMinutes = 2, CreatedUtc = now, UpdatedUtc = now },
            new MenuItem { Id = Guid.NewGuid(), Name = "Cappuccino", Description = "Espresso with steamed milk foam", Price = 4.75m, Category = "Coffee", ImageUrl = "/assets/img/cappuccino.jpg", IsAvailable = true, PrepTimeMinutes = 3, CreatedUtc = now, UpdatedUtc = now },
            new MenuItem { Id = Guid.NewGuid(), Name = "Latte", Description = "Smooth espresso with silky milk", Price = 5.25m, Category = "Coffee", ImageUrl = "/assets/img/latte.jpg", IsAvailable = true, PrepTimeMinutes = 3, CreatedUtc = now, UpdatedUtc = now },
            new MenuItem { Id = Guid.NewGuid(), Name = "Cold Brew", Description = "Slow-steeped, smooth and bold", Price = 4.50m, Category = "Coffee", ImageUrl = "/assets/img/coldbrew.jpg", IsAvailable = true, PrepTimeMinutes = 1, CreatedUtc = now, UpdatedUtc = now },
            new MenuItem { Id = Guid.NewGuid(), Name = "Croissant", Description = "Buttery, flaky French pastry", Price = 3.95m, Category = "Pastry", ImageUrl = "/assets/img/croissant.jpg", IsAvailable = true, PrepTimeMinutes = 1, CreatedUtc = now, UpdatedUtc = now },
            new MenuItem { Id = Guid.NewGuid(), Name = "Blueberry Muffin", Description = "Fresh-baked with wild blueberries", Price = 3.25m, Category = "Pastry", ImageUrl = "/assets/img/muffin.jpg", IsAvailable = true, PrepTimeMinutes = 1, CreatedUtc = now, UpdatedUtc = now },
            new MenuItem { Id = Guid.NewGuid(), Name = "Avocado Toast", Description = "Sourdough, smashed avocado, sea salt", Price = 9.50m, Category = "Food", ImageUrl = "/assets/img/avotoast.jpg", IsAvailable = true, PrepTimeMinutes = 6, CreatedUtc = now, UpdatedUtc = now },
            new MenuItem { Id = Guid.NewGuid(), Name = "Caprese Panini", Description = "Mozzarella, tomato, basil, balsamic", Price = 11.25m, Category = "Food", ImageUrl = "/assets/img/panini.jpg", IsAvailable = true, PrepTimeMinutes = 8, CreatedUtc = now, UpdatedUtc = now },
            new MenuItem { Id = Guid.NewGuid(), Name = "Matcha Latte", Description = "Ceremonial matcha, steamed milk", Price = 5.75m, Category = "Tea", ImageUrl = "/assets/img/matcha.jpg", IsAvailable = true, PrepTimeMinutes = 3, CreatedUtc = now, UpdatedUtc = now },
            new MenuItem { Id = Guid.NewGuid(), Name = "Chai Latte", Description = "Spiced black tea with steamed milk", Price = 5.25m, Category = "Tea", ImageUrl = "/assets/img/chai.jpg", IsAvailable = true, PrepTimeMinutes = 3, CreatedUtc = now, UpdatedUtc = now }
        );
        await db.SaveChangesAsync(ct);
    }
}
