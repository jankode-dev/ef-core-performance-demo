using System.Diagnostics;
using EfCorePerformanceDemo.Data;

namespace EfCorePerformanceDemo;

public static class Bench
{
    const int Runs = 3;

    public static void Header(string title, string description)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine($"  {description}");
        Console.WriteLine();
        Console.WriteLine($"  {"Approach",-36}{"Queries",8}{"Time",12}{"Allocated",11}{"Tracked",9}  Result");
    }

    public static async Task Run(string name, Func<ShopDbContext, Task<string>> action, bool lazyLoading = false, bool rollback = false)
    {
        await Measure(action, lazyLoading, rollback);

        var runs = new List<Measurement>();
        for (var i = 0; i < Runs; i++)
            runs.Add(await Measure(action, lazyLoading, rollback));

        var time = runs.Average(r => r.Elapsed.TotalMilliseconds);
        var megabytes = runs.Average(r => r.AllocatedBytes) / 1024 / 1024;
        var last = runs[^1];

        Console.WriteLine($"  {name,-36}{last.Queries,8:N0}{time,9:N1} ms{megabytes,8:N1} MB{last.Tracked,9:N0}  {last.Result}");
    }

    static async Task<Measurement> Measure(Func<ShopDbContext, Task<string>> action, bool lazyLoading, bool rollback)
    {
        await using var db = new ShopDbContext(lazyLoading);
        await using var transaction = rollback ? await db.Database.BeginTransactionAsync() : null;

        GC.Collect();
        QueryCounter.Instance.Reset();
        var allocatedBefore = GC.GetTotalAllocatedBytes(true);
        var stopwatch = Stopwatch.StartNew();

        var result = await action(db);

        stopwatch.Stop();
        var allocated = GC.GetTotalAllocatedBytes(true) - allocatedBefore;
        var measurement = new Measurement(QueryCounter.Instance.Count, stopwatch.Elapsed, allocated, db.ChangeTracker.Entries().Count(), result);

        if (transaction is not null)
            await transaction.RollbackAsync();

        return measurement;
    }

    record Measurement(int Queries, TimeSpan Elapsed, long AllocatedBytes, int Tracked, string Result);
}
