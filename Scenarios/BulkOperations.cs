using EfCorePerformanceDemo.Data;
using EfCorePerformanceDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCorePerformanceDemo.Scenarios;

public static class BulkOperations
{
    static readonly DateTime Cutoff = DateTime.Today.AddDays(-30);

    public static async Task RunAsync()
    {
        Bench.Header("3. Bulk operations", "Cancel pending orders older than 30 days, delete cancelled orders (each run is rolled back)");

        await Bench.Run("Update: loop + SaveChanges", UpdateWithSaveChanges, rollback: true);
        await Bench.Run("Update: ExecuteUpdate", UpdateWithExecuteUpdate, rollback: true);
        await Bench.Run("Delete: RemoveRange + SaveChanges", DeleteWithSaveChanges, rollback: true);
        await Bench.Run("Delete: ExecuteDelete", DeleteWithExecuteDelete, rollback: true);
    }

    static IQueryable<Order> StalePendingOrders(ShopDbContext db) =>
        db.Orders.Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < Cutoff);

    static IQueryable<Order> CancelledOrders(ShopDbContext db) =>
        db.Orders.Where(o => o.Status == OrderStatus.Cancelled);

    static async Task<string> UpdateWithSaveChanges(ShopDbContext db)
    {
        var orders = await StalePendingOrders(db).ToListAsync();
        foreach (var order in orders)
            order.Status = OrderStatus.Cancelled;

        await db.SaveChangesAsync();
        return $"{orders.Count:N0} orders cancelled";
    }

    static async Task<string> UpdateWithExecuteUpdate(ShopDbContext db)
    {
        var updated = await StalePendingOrders(db)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Cancelled));

        return $"{updated:N0} orders cancelled";
    }

    static async Task<string> DeleteWithSaveChanges(ShopDbContext db)
    {
        var orders = await CancelledOrders(db).ToListAsync();
        db.Orders.RemoveRange(orders);

        await db.SaveChangesAsync();
        return $"{orders.Count:N0} orders deleted";
    }

    static async Task<string> DeleteWithExecuteDelete(ShopDbContext db)
    {
        var deleted = await CancelledOrders(db).ExecuteDeleteAsync();
        return $"{deleted:N0} orders deleted";
    }
}
