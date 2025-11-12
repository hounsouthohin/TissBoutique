using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Orders
{
    public class CreateOrderDto
    {
        [Required]
        public ShippingAddressDto ShippingAddress { get; set; } = null!;

        public string? Notes { get; set; }

        [Required]
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
