using System.Collections.Generic;

namespace ECommerce.Application.DTOs.Payments
{
    public class CreatePaymentIntentDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "CAD";
        public Dictionary<string, string>? Metadata { get; set; }
    }
}