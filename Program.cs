using System.Diagnostics;
using EfCorePerformanceDemo.Data;
using EfCorePerformanceDemo.Scenarios;
using Microsoft.EntityFrameworkCore;

await using (var db = new ShopDbContext())
{
    if (args.Contains("--reset"))
        await db.Database.EnsureDeletedAsync();

    await db.Database.EnsureCreatedAsync();

    if (!await db.Customers.AnyAsync())
    {
        Console.Write("Seeding shop.db... ");
        var stopwatch = Stopwatch.StartNew();
        await Seeder.SeedAsync(db);
        Console.WriteLine($"done in {stopwatch.Elapsed.TotalSeconds:N1} s");
    }

    var customers = await db.Customers.CountAsync();
    var orders = await db.Orders.CountAsync();
    var lines = await db.OrderLines.CountAsync();

    Console.WriteLine($"shop.db: {customers:N0} customers, {orders:N0} orders, {lines:N0} order lines");
}

await RelatedData.RunAsync();
await ChangeTracking.RunAsync();
await BulkOperations.RunAsync();
await CompiledQueries.RunAsync();
