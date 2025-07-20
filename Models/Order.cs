using System.ComponentModel.DataAnnotations;

namespace OrdersApi.Models;

public class Order
{
    [Key]
    public Guid OrderId { get; set; }

    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; }

    // 导航属性
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
} 