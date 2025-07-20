using Microsoft.EntityFrameworkCore;
using OrdersApi.Data;
using OrdersApi.Models;

namespace OrdersApi.Repositories;

/// <summary>
/// Order repository implementation
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            order.OrderId = Guid.NewGuid();
            order.CreatedAt = DateTime.UtcNow;

            foreach (var item in order.Items)
            {
                item.OrderId = order.OrderId;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return order;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}