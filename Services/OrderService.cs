using OrdersApi.Models;
using OrdersApi.Models.DTOs;
using OrdersApi.Repositories;

namespace OrdersApi.Services;

/// <summary>
/// Order service implementation
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Starting to create order, customer name: {CustomerName}, number of items: {ItemCount}",
                request.CustomerName, request.Items.Count);

            var order = MapToEntity(request);
            var createdOrder = await _orderRepository.CreateOrderAsync(order);
            var response = MapToResponse(createdOrder);

            _logger.LogInformation("Order created successfully, order ID: {OrderId}", createdOrder.OrderId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order, customer name: {CustomerName}", request.CustomerName);
            throw;
        }
    }

    /// <summary>
    /// Map request DTO to entity
    /// </summary>
    private static Order MapToEntity(CreateOrderRequest request)
    {
        return new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList()
        };
    }

    /// <summary>
    /// Map entity to response DTO
    /// </summary>
    private static CreateOrderResponse MapToResponse(Order order)
    {
        return new CreateOrderResponse
        {
            OrderId = order.OrderId,
            CustomerName = order.CustomerName,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(item => new OrderItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList()
        };
    }
}