using System.ComponentModel.DataAnnotations;

namespace OrdersApi.Models.DTOs;

/// <summary>
/// DTO for creating an order request
/// </summary>
public class CreateOrderRequest
{
    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Order items are required")]
    [MinLength(1, ErrorMessage = "At least one order item is required")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating an order item request
/// </summary>
public class CreateOrderItemRequest
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}