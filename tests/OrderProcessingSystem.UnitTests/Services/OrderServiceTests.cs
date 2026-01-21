using Moq;
using OrderProcessingSystem.Application.DTOs;
using OrderProcessingSystem.Application.Services;
using OrderProcessingSystem.Domain.Entities;
using OrderProcessingSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Xunit;

namespace OrderProcessingSystem.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepo;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _mockRepo = new Mock<IOrderRepository>();
        _mockLogger = new Mock<ILogger<OrderService>>();
        _orderService = new OrderService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnOrderDto_WhenInputIsValid()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductName = "Test Product", Quantity = 1, UnitPrice = 100 }
            }
        };

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o); // Return the same order passed in

        // Act
        var result = await _orderService.CreateOrderAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Pending.ToString(), result.Status);
        Assert.Single(result.Items);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldComplete_WhenOrderIsPending()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order(new List<OrderItem>()); // Defaults to Pending
        
        _mockRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        await _orderService.CancelOrderAsync(orderId);

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        _mockRepo.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldThrowException_WhenOrderIsNotPending()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order(new List<OrderItem>());
        order.UpdateStatus(OrderStatus.Processing); // Manually set to Processing for test

        _mockRepo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CancelOrderAsync(orderId));
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }
    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order(new List<OrderItem>());
        _mockRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<OrderProcessingSystem.Application.Exceptions.NotFoundException>(() => _orderService.GetOrderByIdAsync(orderId));
    }
    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnAllOrders_WhenNoFilterProvided()
    {
        // Arrange
        var orders = new List<Order> { new Order(new List<OrderItem>()), new Order(new List<OrderItem>()) };
        _mockRepo.Setup(r => r.GetAllAsync(null)).ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetAllOrdersAsync(null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnFilteredOrders_WhenFilterProvided()
    {
        // Arrange
        var status = OrderStatus.Processing;
        var orders = new List<Order> { new Order(new List<OrderItem>()) }; 
        _mockRepo.Setup(r => r.GetAllAsync(status)).ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetAllOrdersAsync(status);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockRepo.Verify(r => r.GetAllAsync(status), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<OrderProcessingSystem.Application.Exceptions.NotFoundException>(() => _orderService.CancelOrderAsync(orderId));
    }
}
