using OrdersApi.Models;

namespace OrdersApi.Repositories;

/// <summary>
/// Order repository interface
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Create order and its items (transactional operation)
    /// </summary>
    Task<Order> CreateOrderAsync(Order order);
}