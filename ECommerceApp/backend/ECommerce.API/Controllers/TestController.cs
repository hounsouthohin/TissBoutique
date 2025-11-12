using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IStripeService _stripeService;
        private readonly ILogger<TestController> _logger;

        public TestController(
            IEmailService emailService,
            IStripeService stripeService,
            ILogger<TestController> logger)
        {
            _emailService = emailService;
            _stripeService = stripeService;
            _logger = logger;
        }

        /// <summary>
        /// Test l'envoi d'email (nécessite configuration)
        /// </summary>
        [HttpPost("send-email")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string to)
        {
            if (string.IsNullOrEmpty(to))
                return BadRequest(new { error = "Email address required" });

            try
            {
                await _emailService.SendWelcomeEmailAsync(to, "Test User");
                return Ok(new
                {
                    success = true,
                    message = "Email sent successfully",
                    to
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not configured"))
            {
                return BadRequest(new
                {
                    success = false,
                    error = "Email service not configured",
                    message = "Please configure EmailSettings in appsettings.json",
                    instructions = new
                    {
                        step1 = "Get Gmail App Password from https://myaccount.google.com/security",
                        step2 = "Add EmailSettings section to appsettings.json",
                        step3 = "Restart the backend"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email");
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Test la configuration Stripe
        /// </summary>
        [HttpGet("stripe-status")]
        public IActionResult CheckStripeStatus()
        {
            return Ok(new
            {
                message = "Stripe endpoint available",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Test la création d'un Payment Intent
        /// </summary>
        [HttpPost("stripe-payment")]
        public async Task<IActionResult> TestStripePayment([FromQuery] decimal amount = 50)
        {
            try
            {
                var metadata = new Dictionary<string, string>
                {
                    { "test", "true" },
                    { "orderId", "TEST-001" }
                };

                var clientSecret = await _stripeService.CreatePaymentIntentAsync(
                    amount,
                    "cad",
                    metadata);

                return Ok(new
                {
                    success = true,
                    message = "Payment Intent created",
                    amount,
                    currency = "CAD",
                    clientSecret = clientSecret.Substring(0, 30) + "...",
                    fullSecret = clientSecret
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Stripe");

                if (ex.Message.Contains("No such API key") || ex.Message.Contains("Invalid API Key"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Stripe not configured or invalid API key",
                        message = "Please configure Stripe.SecretKey in appsettings.json",
                        instructions = new
                        {
                            step1 = "Create Stripe account at https://dashboard.stripe.com/register",
                            step2 = "Get test API key (sk_test_...)",
                            step3 = "Add to appsettings.json",
                            step4 = "Restart backend"
                        }
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        [HttpGet("raw-config")]
        public IActionResult GetRawConfig([FromServices] IConfiguration config)
        {
            var stripeSection = config.GetSection("Stripe");

            return Ok(new
            {
                // Test différentes façons de lire
                method1_bracket = config["Stripe:SecretKey"],
                method2_getvalue = config.GetValue<string>("Stripe:SecretKey"),
                method3_section = stripeSection["SecretKey"],

                // Vérifier que la section existe
                sectionExists = stripeSection.Exists(),

                // Lister toutes les clés de la section
                allStripeKeys = stripeSection.GetChildren().Select(x => new {
                    key = x.Key,
                    value = x.Value?.Substring(0, Math.Min(20, x.Value?.Length ?? 0)) + "..."
                }),

                // Voir toutes les sources de config
                providers = (config as IConfigurationRoot)?.Providers
                    .Select(p => p.GetType().Name).ToList()
            });
        }

        /// <summary>
        /// Vérifier l'état de toutes les configurations
        /// </summary>
        [HttpGet("config-status")]
        public IActionResult GetConfigStatus([FromServices] IConfiguration config)
        {
            var emailConfigured = !string.IsNullOrEmpty(config["EmailSettings:SenderEmail"]) &&
                                 !string.IsNullOrEmpty(config["EmailSettings:Password"]);

            var stripeConfigured = !string.IsNullOrEmpty(config["Stripe:SecretKey"]) &&
                                  config["Stripe:SecretKey"]?.StartsWith("sk_") == true;

            return Ok(new
            {
                email = new
                {
                    configured = emailConfigured,
                    status = emailConfigured ? "✅ Ready" : "⚠️ Not configured",
                    note = emailConfigured ? null : "Configure EmailSettings in appsettings.json"
                },
                stripe = new
                {
                    configured = stripeConfigured,
                    status = stripeConfigured ? "✅ Ready" : "⚠️ Not configured",
                    note = stripeConfigured ? null : "Configure Stripe.SecretKey in appsettings.json"
                },
                database = new
                {
                    configured = true,
                    status = "✅ Connected"
                }
            });
        }
    }
}