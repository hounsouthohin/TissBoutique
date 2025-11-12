using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders
{
    public class PaymentInfoDto
    {
        public int Id { get; set; }
        public string StripePaymentIntentId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
