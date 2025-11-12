using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public ShippingAddressDto ShippingAddress { get; set; } = null!;
        public string? TrackingNumber { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public PaymentInfoDto? Payment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }
}
