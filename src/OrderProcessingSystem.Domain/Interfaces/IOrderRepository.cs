using OrderProcessingSystem.Domain.Entities;

namespace OrderProcessingSystem.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order);
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetAllAsync(OrderStatus? statusFilter = null);
    Task UpdateAsync(Order order);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
}
