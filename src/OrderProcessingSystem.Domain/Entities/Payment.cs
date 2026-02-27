namespace OrderProcessingSystem.Domain.Entities;

public class Payment
{
    public Guid PaymentId { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    
    
    public Payment(Guid orderId, decimal amount) 
    {
        OrderId = orderId;
        Amount = amount;
    }

}

// Updated Requirements for coding round
//  
//     - Customers need to pay for orders to be processed
//     - Customers can place orders without making any payments. 
//     The order will be placed in PENDING state till the entire payment is completed
//                                                                          - Customers can make multiple partial payments for an order
//     - The order must be moved from PENDING to PROCESSING status by the background service only when the total 
//     payment by the customer is equal to the total price of the order