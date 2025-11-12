using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IStripeService _stripeService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IStripeService stripeService,
            ILogger<PaymentService> logger)
        {
            _stripeService = stripeService;
            _logger = logger;
        }

        public async Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentIntentDto dto)
        {
            try
            {
                // ✅ CORRECTION: CreatePaymentIntentAsync retourne le ClientSecret, pas le PaymentIntentId
                var clientSecret = await _stripeService.CreatePaymentIntentAsync(
                    dto.Amount,
                    dto.Currency,
                    dto.Metadata ?? new Dictionary<string, string>());

                _logger.LogInformation(
                    "Payment Intent created for amount {Amount} {Currency}",
                    dto.Amount,
                    dto.Currency);

                return new PaymentIntentDto
                {
                    ClientSecret = clientSecret,
                    Amount = dto.Amount,
                    Currency = dto.Currency
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                throw;
            }
        }

        public async Task<PaymentResultDto> ConfirmPaymentAsync(ConfirmPaymentDto dto)
        {
            try
            {
                // ✅ CORRECTION: Utilise ConfirmPaymentAsync (pas ConfirmPaymentIntentAsync)
                var success = await _stripeService.ConfirmPaymentAsync(dto.PaymentIntentId);

                // ✅ CORRECTION: Récupère les détails du paiement
                var paymentDetails = await _stripeService.GetPaymentDetailsAsync(dto.PaymentIntentId);

                // Cast vers dynamic pour accéder aux propriétés
                dynamic details = paymentDetails;
                string status = details.Status ?? "unknown";

                _logger.LogInformation(
                    "Payment {PaymentIntentId} confirmation: {Success}, Status: {Status}",
                    dto.PaymentIntentId,
                    success,
                    status);

                return new PaymentResultDto
                {
                    Success = success,
                    PaymentIntentId = dto.PaymentIntentId,
                    Status = status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment {PaymentIntentId}", dto.PaymentIntentId);
                throw;
            }
        }

        public async Task<RefundResultDto> RefundPaymentAsync(RefundPaymentDto dto)
        {
            try
            {
                var success = await _stripeService.RefundPaymentAsync(
                    dto.PaymentIntentId,
                    dto.Amount);

                _logger.LogInformation(
                    "Refund processed for payment {PaymentIntentId}: {Success}",
                    dto.PaymentIntentId,
                    success);

                return new RefundResultDto
                {
                    Success = success,
                    PaymentIntentId = dto.PaymentIntentId,
                    RefundAmount = dto.Amount,
                    Status = success ? "succeeded" : "failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment {PaymentIntentId}", dto.PaymentIntentId);
                throw;
            }
        }

        public async Task HandleWebhookAsync(string json, string stripeSignature)
        {
            // Cette méthode est gérée par WebhooksController
            // On la laisse vide pour l'instant
            await Task.CompletedTask;
        }
    }
}