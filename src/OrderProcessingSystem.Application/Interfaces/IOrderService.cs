using OrderProcessingSystem.Application.DTOs;
using OrderProcessingSystem.Domain.Entities;

namespace OrderProcessingSystem.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task<OrderDto> GetOrderByIdAsync(Guid id);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(OrderStatus? statusFilter);
    Task CancelOrderAsync(Guid id);
    Task AddPayment(Guid orderId , decimal amount);
}
