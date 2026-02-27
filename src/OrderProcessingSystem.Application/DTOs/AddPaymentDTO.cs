namespace OrderProcessingSystem.Application.DTOs;

public class AddPaymentDTO
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}
