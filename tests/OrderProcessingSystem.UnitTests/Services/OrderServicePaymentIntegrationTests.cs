using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessingSystem.Application.DTOs;
using OrderProcessingSystem.Application.Services;
using OrderProcessingSystem.Domain.Entities;
using OrderProcessingSystem.Infrastructure.Data;
using OrderProcessingSystem.Infrastructure.Repositories;
using Xunit;

namespace OrderProcessingSystem.UnitTests.Services;

public class OrderServicePaymentIntegrationTests
{
    [Fact]
    public async Task AddPayment_ShouldPersistPayment_WhenUsingInMemoryRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var repository = new OrderRepository(context);
        var logger = new Mock<ILogger<OrderService>>();
        var service = new OrderService(repository, logger.Object);

        var createOrderDto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Test Product", Quantity = 2, UnitPrice = 50m }
            }
        };

        var createdOrder = await service.CreateOrderAsync(createOrderDto);

        await service.AddPayment(createdOrder.Id, 40m);

        var savedOrder = await repository.GetByIdAsync(createdOrder.Id);

        Assert.NotNull(savedOrder);
        Assert.Single(savedOrder.Payments);
        Assert.Equal(40m, savedOrder.Payments[0].Amount);
        Assert.Equal(createdOrder.Id, savedOrder.Payments[0].OrderId);
    }
}
