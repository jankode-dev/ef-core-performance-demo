using EfCorePerformanceDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCorePerformanceDemo.Data;

public class ShopDbContext(bool lazyLoading = false) : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options
            .UseSqlite("Data Source=shop.db")
            .AddInterceptors(QueryCounter.Instance);

        if (lazyLoading)
            options.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasIndex(c => c.City);
        modelBuilder.Entity<Order>().HasIndex(o => new { o.Status, o.CreatedAt });
    }
}
