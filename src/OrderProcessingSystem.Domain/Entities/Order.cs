namespace OrderProcessingSystem.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();

    public List<Payment> Payments { get; private set; } = new();
    
    public decimal TotalAmount { get; private set; }
    
    public decimal PaidAmount => Payments.Sum(p => p.Amount);
    public decimal PendingAmount => TotalAmount - PaidAmount;

    public Order(List<OrderItem> items)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        Items = items;
        TotalAmount = Items.Sum(x => x.Quantity * x.UnitPrice);
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

    public void AddPayment(decimal amount)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot pay processed order");
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.");
        if (amount > PendingAmount)
            throw new InvalidOperationException("Payment amount cannot exceed pending amount.");

        Payments.Add(new Payment(Id, amount));
    }
    
    public bool IsFullyPaid()
    {
        return PaidAmount == TotalAmount;
    }

    public void MarkAsProcessing()
    {
        Status = OrderStatus.Processing;
    }
}
