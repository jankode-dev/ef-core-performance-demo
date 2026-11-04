using EfCorePerformanceDemo.Entities;

namespace EfCorePerformanceDemo.Data;

public static class Seeder
{
    static readonly string[] FirstNames = ["Marko", "Nikola", "Stefan", "Luka", "Milos", "Jelena", "Ana", "Milica", "Jovana", "Teodora"];
    static readonly string[] LastNames = ["Petrovic", "Jovanovic", "Nikolic", "Markovic", "Djordjevic", "Stojanovic", "Ilic", "Pavlovic", "Kovacevic", "Popovic"];
    static readonly string[] Cities = ["Beograd", "Novi Sad", "Nis", "Kragujevac", "Subotica", "Cacak", "Kraljevo", "Zrenjanin", "Pancevo", "Sabac"];
    static readonly string[] Categories = ["Laptops", "Monitors", "Peripherals", "Storage", "Networking"];

    public static async Task SeedAsync(ShopDbContext db, int customerCount = 1_000)
    {
        var random = new Random(42);

        var products = new List<Product>();
        for (var i = 1; i <= 200; i++)
        {
            products.Add(new Product
            {
                Name = $"Product {i:000}",
                Category = Categories[i % Categories.Length],
                Price = random.Next(1_000, 250_000) / 100m
            });
        }

        var customers = new List<Customer>();
        for (var i = 1; i <= customerCount; i++)
        {
            var firstName = FirstNames[random.Next(FirstNames.Length)];
            var lastName = LastNames[random.Next(LastNames.Length)];

            var customer = new Customer
            {
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName}.{lastName}{i}@example.com".ToLowerInvariant(),
                City = Cities[random.Next(Cities.Length)]
            };

            for (var o = random.Next(1, 10); o > 0; o--)
            {
                var order = new Order
                {
                    CreatedAt = DateTime.Today.AddDays(-random.Next(730)),
                    Status = (OrderStatus)random.Next(4)
                };

                for (var l = random.Next(1, 6); l > 0; l--)
                {
                    var product = products[random.Next(products.Count)];
                    order.Lines.Add(new OrderLine { Product = product, Quantity = random.Next(1, 5), UnitPrice = product.Price });
                }

                customer.Orders.Add(order);
            }

            customers.Add(customer);
        }

        db.Products.AddRange(products);
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();
    }
}
