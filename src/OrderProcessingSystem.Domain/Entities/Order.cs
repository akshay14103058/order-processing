namespace OrderProcessingSystem.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();

    public Order(List<OrderItem> items)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        Items = items;
    }

    // Required for EF Core
    protected Order() { }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be cancelled.");
        }
        Status = OrderStatus.Cancelled;
    }
}
