namespace OrdersApi.Models.DTOs;

/// <summary>
/// create order DTO
/// </summary>
public class CreateOrderResponse
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

/// <summary>
/// order item DTO
/// </summary>
public class OrderItemResponse
{
    public int Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
} 