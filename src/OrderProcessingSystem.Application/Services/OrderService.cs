using OrderProcessingSystem.Application.DTOs;
using OrderProcessingSystem.Application.Interfaces;
using OrderProcessingSystem.Application.Exceptions;
using OrderProcessingSystem.Domain.Entities;
using OrderProcessingSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace OrderProcessingSystem.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        var items = createOrderDto.Items
            .Select(i => new OrderItem(i.ProductName, i.Quantity, i.UnitPrice))
            .ToList();

        var order = new Order(items);
        
        _logger.LogInformation("Creating new order with {ItemCount} items.", items.Count);
        
        var createdOrder = await _orderRepository.AddAsync(order);
        
        _logger.LogInformation("Order {OrderId} created successfully.", createdOrder.Id);

        return OrderDto.FromEntity(createdOrder);
    }

    public async Task<OrderDto> GetOrderByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), id);
        }

        return OrderDto.FromEntity(order);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(OrderStatus? statusFilter)
    {
        var orders = await _orderRepository.GetAllAsync(statusFilter);
        return orders.Select(OrderDto.FromEntity);
    }

    public async Task CancelOrderAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
             throw new NotFoundException(nameof(Order), id);
        }

        // Domain rule: Cancel() will throw InvalidOperationException if status is not Pending.
        // Middleware will catch this and return 400 BadRequest.
        order.Cancel();
        
        await _orderRepository.UpdateAsync(order);
        _logger.LogInformation("Order {OrderId} was successfully cancelled.", id);
    }

    public async Task AddPayment(Guid orderId, decimal amount)
    {
        var order = await _orderRepository.GetByIdAsync((orderId));
        if(order == null) 
        {
            throw new NotFoundException(nameof(Order), orderId);
        }
        order.AddPayment(amount);
        await _orderRepository.UpdateAsync(order);
    }
}
