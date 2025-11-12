namespace ECommerce.Application.DTOs.Payments
{
    public class RefundPaymentDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
    }
}