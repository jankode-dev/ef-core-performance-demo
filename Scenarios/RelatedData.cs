using EfCorePerformanceDemo.Data;
using EfCorePerformanceDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCorePerformanceDemo.Scenarios;

public static class RelatedData
{
    const string City = "Novi Sad";

    public static async Task RunAsync()
    {
        Bench.Header("1. Loading related data", $"Revenue report for customers from {City} (customers -> orders -> order lines)");

        await Bench.Run("Lazy loading (N+1)", LazyLoading, lazyLoading: true);
        await Bench.Run("Include + ThenInclude", EagerLoading);
        await Bench.Run("Include + AsSplitQuery", SplitQuery);
        await Bench.Run("Projection (Select)", Projection);
    }

    static async Task<string> LazyLoading(ShopDbContext db)
    {
        var customers = await db.Customers
            .Where(c => c.City == City)
            .ToListAsync();

        return Describe(ToReport(customers));
    }

    static async Task<string> EagerLoading(ShopDbContext db)
    {
        var customers = await db.Customers
            .Where(c => c.City == City)
            .Include(c => c.Orders)
            .ThenInclude(o => o.Lines)
            .ToListAsync();

        return Describe(ToReport(customers));
    }

    static async Task<string> SplitQuery(ShopDbContext db)
    {
        var customers = await db.Customers
            .Where(c => c.City == City)
            .Include(c => c.Orders)
            .ThenInclude(o => o.Lines)
            .AsSplitQuery()
            .ToListAsync();

        return Describe(ToReport(customers));
    }

    static async Task<string> Projection(ShopDbContext db)
    {
        var report = await db.Customers
            .Where(c => c.City == City)
            .Select(c => new CustomerRevenue(
                c.FirstName + " " + c.LastName,
                c.Orders.Count,
                c.Orders.SelectMany(o => o.Lines).Sum(l => l.Quantity * l.UnitPrice)))
            .ToListAsync();

        return Describe(report);
    }

    static List<CustomerRevenue> ToReport(List<Customer> customers) =>
        customers
            .Select(c => new CustomerRevenue(
                c.FirstName + " " + c.LastName,
                c.Orders.Count,
                c.Orders.SelectMany(o => o.Lines).Sum(l => l.Quantity * l.UnitPrice)))
            .ToList();

    static string Describe(List<CustomerRevenue> report) =>
        $"{report.Count} customers, revenue {report.Sum(r => r.Revenue):N2}";
}

public record CustomerRevenue(string Name, int Orders, decimal Revenue);
