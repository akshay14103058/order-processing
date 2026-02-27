using Microsoft.EntityFrameworkCore;
using OrderProcessingSystem.Domain.Entities;
using OrderProcessingSystem.Domain.Interfaces;
using OrderProcessingSystem.Infrastructure.Data;

namespace OrderProcessingSystem.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order> AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(OrderStatus? statusFilter = null)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(o => o.Status == statusFilter.Value);
        }

        return await query.ToListAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        if (_context.Entry(order).State == EntityState.Detached)
        {
            _context.Orders.Attach(order);
        }

        _context.ChangeTracker.DetectChanges();

        foreach (var paymentEntry in _context.ChangeTracker.Entries<Payment>()
                     .Where(entry => entry.State == EntityState.Modified))
        {
            var paymentExists = await _context.Payments
                .AsNoTracking()
                .AnyAsync(p => p.PaymentId == paymentEntry.Entity.PaymentId);

            if (!paymentExists)
            {
                paymentEntry.State = EntityState.Added;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.Payments)
            .Where(o => o.Status == status)
            .ToListAsync();
    }
}
