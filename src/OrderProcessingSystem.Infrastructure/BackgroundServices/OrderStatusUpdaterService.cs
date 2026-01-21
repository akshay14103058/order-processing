using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderProcessingSystem.Domain.Entities;
using OrderProcessingSystem.Domain.Interfaces;

namespace OrderProcessingSystem.Infrastructure.BackgroundServices;

public class OrderStatusUpdaterService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderStatusUpdaterService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public OrderStatusUpdaterService(
        IServiceProvider serviceProvider, 
        ILogger<OrderStatusUpdaterService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Status Updater Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing orders.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingOrdersAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var pendingOrders = await orderRepository.GetOrdersByStatusAsync(OrderStatus.Pending);

            if (pendingOrders.Any())
            {
                _logger.LogInformation($"Found {pendingOrders.Count()} pending orders. Updating to Processing.");

                foreach (var order in pendingOrders)
                {
                    order.UpdateStatus(OrderStatus.Processing);
                    await orderRepository.UpdateAsync(order);
                }
            }
        }
    }
}
