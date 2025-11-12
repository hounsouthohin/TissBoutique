using ECommerce.Application.DTOs.Payments;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentIntentDto dto);
        Task<PaymentResultDto> ConfirmPaymentAsync(ConfirmPaymentDto dto);
        Task<RefundResultDto> RefundPaymentAsync(RefundPaymentDto dto);
        Task HandleWebhookAsync(string json, string stripeSignature);
    }
}