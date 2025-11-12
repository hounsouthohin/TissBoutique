using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Crée un Payment Intent pour initier un paiement
        /// </summary>
        [HttpPost("create-intent")]
        public async Task<ActionResult<PaymentIntentDto>> CreatePaymentIntent(
            [FromBody] CreatePaymentIntentDto dto)
        {
            try
            {
                var result = await _paymentService.CreatePaymentIntentAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Confirme un paiement côté serveur
        /// </summary>
        [HttpPost("confirm")]
        public async Task<ActionResult<PaymentResultDto>> ConfirmPayment(
            [FromBody] ConfirmPaymentDto dto)
        {
            try
            {
                var result = await _paymentService.ConfirmPaymentAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Rembourse un paiement (total ou partiel)
        /// </summary>
        [HttpPost("refund")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RefundResultDto>> RefundPayment(
            [FromBody] RefundPaymentDto dto)
        {
            try
            {
                var result = await _paymentService.RefundPaymentAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test endpoint pour vérifier la configuration Stripe
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult TestStripeConfiguration()
        {
            return Ok(new
            {
                message = "Stripe is configured",
                timestamp = DateTime.UtcNow
            });
        }
    }
}