namespace ECommerce.Application.DTOs.Payments
{
    public class PaymentResultDto
    {
        public bool Success { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}