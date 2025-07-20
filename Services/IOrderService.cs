using OrdersApi.Models.DTOs;

namespace OrdersApi.Services;

/// <summary>
/// Order service interface
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <returns>Created order information</returns>
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);
}