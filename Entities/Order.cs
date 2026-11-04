namespace EfCorePerformanceDemo.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual List<OrderLine> Lines { get; set; } = [];
}
