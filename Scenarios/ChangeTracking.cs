using EfCorePerformanceDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCorePerformanceDemo.Scenarios;

public static class ChangeTracking
{
    public static async Task RunAsync()
    {
        Bench.Header("2. Change tracking", "Read-only load of all orders with their lines and products");

        await Bench.Run("Tracking (default)", db => Load(db.Orders));
        await Bench.Run("AsNoTracking", db => Load(db.Orders.AsNoTracking()));
        await Bench.Run("AsNoTrackingWithIdentityResolution", db => Load(db.Orders.AsNoTrackingWithIdentityResolution()));
    }

    static async Task<string> Load(IQueryable<Order> query)
    {
        var orders = await query
            .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
            .ToListAsync();

        var products = orders
            .SelectMany(o => o.Lines)
            .Select(l => l.Product)
            .Distinct(ReferenceEqualityComparer.Instance)
            .Count();

        return $"{orders.Count:N0} orders, {products:N0} Product objects";
    }
}
