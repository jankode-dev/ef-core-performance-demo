using EfCorePerformanceDemo.Data;
using EfCorePerformanceDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCorePerformanceDemo.Scenarios;

public static class CompiledQueries
{
    const int Lookups = 2_000;

    static readonly Func<ShopDbContext, int, Task<Order?>> GetOrder =
        EF.CompileAsyncQuery((ShopDbContext db, int id) => db.Orders.AsNoTracking().FirstOrDefault(o => o.Id == id));

    public static async Task RunAsync()
    {
        Bench.Header("4. Compiled queries", $"{Lookups:N0} lookups of a single order by id");

        await Bench.Run("Regular LINQ query", db => LookupAll(id => db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id)));
        await Bench.Run("EF.CompileAsyncQuery", db => LookupAll(id => GetOrder(db, id)));
    }

    static async Task<string> LookupAll(Func<int, Task<Order?>> lookup)
    {
        var found = 0;
        for (var id = 1; id <= Lookups; id++)
        {
            if (await lookup(id) is not null)
                found++;
        }

        return $"{found:N0} orders found";
    }
}
