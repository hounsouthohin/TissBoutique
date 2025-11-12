using System.IO;
using System.Threading.Tasks;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhooksController> _logger;
        private readonly IOrderService _orderService;
        private readonly IEmailService _emailService;
        private readonly string _webhookSecret;

        public WebhooksController(
            IConfiguration configuration,
            ILogger<WebhooksController> logger,
            IOrderService orderService,
            IEmailService emailService)
        {
            _configuration = configuration;
            _logger = logger;
            _orderService = orderService;
            _emailService = emailService;
            _webhookSecret = _configuration["Stripe:WebhookSecret"] ?? "";
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                _logger.LogInformation("🔔 Stripe webhook received: {EventType}", stripeEvent.Type);

                // Gérer les différents types d'événements
                switch (stripeEvent.Type)
                {
                    case Events.PaymentIntentSucceeded:
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        await HandlePaymentSucceeded(paymentIntent);
                        break;

                    case Events.PaymentIntentPaymentFailed:
                        var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                        await HandlePaymentFailed(failedPayment);
                        break;

                    case Events.ChargeRefunded:
                        var charge = stripeEvent.Data.Object as Charge;
                        await HandleRefund(charge);
                        break;

                    default:
                        _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "❌ Stripe webhook error");
                return BadRequest();
            }
        }

        private async Task HandlePaymentSucceeded(PaymentIntent paymentIntent)
        {
            _logger.LogInformation("✅ Payment succeeded: {PaymentIntentId}", paymentIntent.Id);

            // Extraire l'OrderId des metadata
            if (paymentIntent.Metadata.TryGetValue("OrderId", out var orderIdStr)
                && int.TryParse(orderIdStr, out var orderId))
            {
                try
                {
                    // ✅ UTILISE LA NOUVELLE MÉTHODE avec OrderStatus (enum)
                    var order = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Processing);

                    // Envoyer email de confirmation
                    if (paymentIntent.Metadata.TryGetValue("UserEmail", out var userEmail))
                    {
                        await _emailService.SendOrderConfirmationEmailAsync(
                            userEmail,
                            order.OrderNumber,
                            order.TotalAmount);
                    }

                    _logger.LogInformation("Order {OrderId} updated to Processing", orderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating order {OrderId}", orderId);
                }
            }
        }

        private async Task HandlePaymentFailed(PaymentIntent paymentIntent)
        {
            _logger.LogWarning("❌ Payment failed: {PaymentIntentId}", paymentIntent.Id);

            if (paymentIntent.Metadata.TryGetValue("OrderId", out var orderIdStr)
                && int.TryParse(orderIdStr, out var orderId))
            {
                try
                {
                    await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled);
                    _logger.LogInformation("Order {OrderId} cancelled due to payment failure", orderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                }
            }
        }

        private async Task HandleRefund(Charge charge)
        {
            _logger.LogInformation("💰 Refund processed: {ChargeId}", charge.Id);

            if (charge.Metadata.TryGetValue("OrderId", out var orderIdStr)
                && int.TryParse(orderIdStr, out var orderId))
            {
                try
                {
                    await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Refunded);

                    // Envoyer email de confirmation de remboursement
                    if (charge.Metadata.TryGetValue("UserEmail", out var userEmail)
                        && charge.Metadata.TryGetValue("OrderNumber", out var orderNumber))
                    {
                        await _emailService.SendRefundConfirmationEmailAsync(
                            userEmail,
                            orderNumber,
                            charge.AmountRefunded / 100m);
                    }

                    _logger.LogInformation("Order {OrderId} marked as refunded", orderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing refund for order {OrderId}", orderId);
                }
            }
        }
    }
}