using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace ECommerce.Infrastructure.Services
{
    public class StripePaymentService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly string _secretKey;
        private readonly string _currency;

        public StripePaymentService(
            IConfiguration configuration,
            ILogger<StripePaymentService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _secretKey = _configuration["Stripe:SecretKey"]
                ?? throw new InvalidOperationException("Stripe Secret Key not configured");
            _currency = _configuration["Stripe:Currency"] ?? "cad";

            StripeConfiguration.ApiKey = _secretKey;
        }

        public async Task<string> CreatePaymentIntentAsync(
            decimal amount,
            string currency,
            Dictionary<string, string> metadata)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Conversion en centimes
                    Currency = currency?.ToLower() ?? _currency,
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = metadata ?? new Dictionary<string, string>(),
                    Description = $"Order payment - {metadata?.GetValueOrDefault("OrderNumber", "N/A")}",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Payment Intent created: {PaymentIntentId} for amount {Amount} {Currency}",
                    paymentIntent.Id, amount, currency);

                return paymentIntent.ClientSecret;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error while creating payment intent");
                throw new Exception($"Payment error: {ex.Message}", ex);
            }
        }

        public async Task<bool> ConfirmPaymentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                var isSuccessful = paymentIntent.Status == "succeeded";

                _logger.LogInformation(
                    "Payment {PaymentIntentId} status: {Status}",
                    paymentIntentId,
                    paymentIntent.Status);

                return isSuccessful;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error confirming payment {PaymentIntentId}", paymentIntentId);
                return false;
            }
        }

        public async Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Reason = RefundReasons.RequestedByCustomer
                };

                if (amount.HasValue)
                {
                    options.Amount = (long)(amount.Value * 100);
                }

                var service = new RefundService();
                var refund = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Refund created: {RefundId} for payment {PaymentIntentId}, amount: {Amount}",
                    refund.Id,
                    paymentIntentId,
                    amount.HasValue ? $"{amount.Value:C}" : "Full amount");

                return refund.Status == "succeeded" || refund.Status == "pending";
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error refunding payment {PaymentIntentId}", paymentIntentId);
                return false;
            }
        }

        public async Task<string> CreateCustomerAsync(
            string email,
            string name,
            Dictionary<string, string> metadata)
        {
            try
            {
                var options = new CustomerCreateOptions
                {
                    Email = email,
                    Name = name,
                    Metadata = metadata ?? new Dictionary<string, string>()
                };

                var service = new CustomerService();
                var customer = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Stripe customer created: {CustomerId} for {Email}",
                    customer.Id,
                    email);

                return customer.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error creating Stripe customer for {Email}", email);
                throw;
            }
        }

        public async Task<object> GetPaymentDetailsAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                return new
                {
                    PaymentIntentId = paymentIntent.Id,
                    Amount = paymentIntent.Amount / 100m,
                    Currency = paymentIntent.Currency.ToUpper(),
                    Status = paymentIntent.Status,
                    Created = paymentIntent.Created,
                    Description = paymentIntent.Description,
                    Metadata = paymentIntent.Metadata,
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentMethod = paymentIntent.PaymentMethodId
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error retrieving payment details for {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }

        public async Task<bool> CancelPaymentIntentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var options = new PaymentIntentCancelOptions
                {
                    CancellationReason = "requested_by_customer"
                };

                var paymentIntent = await service.CancelAsync(paymentIntentId, options);

                _logger.LogInformation(
                    "Payment Intent cancelled: {PaymentIntentId}",
                    paymentIntentId);

                return paymentIntent.Status == "canceled";
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error cancelling payment intent {PaymentIntentId}", paymentIntentId);
                return false;
            }
        }

        public async Task<string> CreateCheckoutSessionAsync(
            decimal amount,
            string currency,
            string successUrl,
            string cancelUrl,
            Dictionary<string, string> metadata)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = currency?.ToLower() ?? _currency,
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Order Payment",
                                    Description = metadata?.GetValueOrDefault("OrderNumber", "Order")
                                },
                                UnitAmount = (long)(amount * 100)
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = metadata
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                _logger.LogInformation(
                    "Checkout session created: {SessionId}",
                    session.Id);

                return session.Url;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error creating checkout session");
                throw;
            }
        }

        public async Task<bool> AttachPaymentMethodAsync(string paymentMethodId, string customerId)
        {
            try
            {
                var service = new PaymentMethodService();
                var options = new PaymentMethodAttachOptions
                {
                    Customer = customerId
                };

                await service.AttachAsync(paymentMethodId, options);

                _logger.LogInformation(
                    "Payment method {PaymentMethodId} attached to customer {CustomerId}",
                    paymentMethodId,
                    customerId);

                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error attaching payment method");
                return false;
            }
        }

        public async Task<List<object>> ListPaymentMethodsAsync(string customerId)
        {
            try
            {
                var service = new PaymentMethodService();
                var options = new PaymentMethodListOptions
                {
                    Customer = customerId,
                    Type = "card"
                };

                var paymentMethods = await service.ListAsync(options);

                return paymentMethods.Data.Select(pm => new
                {
                    Id = pm.Id,
                    Type = pm.Type,
                    Card = pm.Card != null ? new
                    {
                        Brand = pm.Card.Brand,
                        Last4 = pm.Card.Last4,
                        ExpMonth = pm.Card.ExpMonth,
                        ExpYear = pm.Card.ExpYear
                    } : null,
                    Created = pm.Created
                } as object).ToList();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error listing payment methods for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<string> GetRefundStatusAsync(string refundId)
        {
            try
            {
                var service = new RefundService();
                var refund = await service.GetAsync(refundId);

                return refund.Status;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error getting refund status for {RefundId}", refundId);
                throw;
            }
        }
    }
}