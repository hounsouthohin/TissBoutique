namespace ECommerce.Application.DTOs.Payments
{
    public class RefundResultDto
    {
        public bool Success { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal? RefundAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}