using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string? _smtpServer;
        private readonly int _smtpPort;
        private readonly string? _senderEmail;
        private readonly string? _senderName;
        private readonly string? _username;
        private readonly string? _password;
        private readonly bool _enableSsl;
        private readonly bool _isConfigured; // ✅ AJOUTÉ

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // ✅ CHANGEMENT : Ne plus throw, juste marquer comme non configuré
            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _senderEmail = _configuration["EmailSettings:SenderEmail"];
            _senderName = _configuration["EmailSettings:SenderName"] ?? "ECommerce Store";
            _username = _configuration["EmailSettings:Username"] ?? _senderEmail;
            _password = _configuration["EmailSettings:Password"];
            _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

            // ✅ Vérifier si configuré correctement
            _isConfigured = !string.IsNullOrEmpty(_senderEmail) &&
                           !string.IsNullOrEmpty(_password) &&
                           !string.IsNullOrEmpty(_smtpServer);

            if (!_isConfigured)
            {
                _logger.LogWarning("⚠️ Email service is not configured. Email features will be disabled.");
            }
            else
            {
                _logger.LogInformation("✅ Email service configured for {Email}", _senderEmail);
            }
        }

        private void EnsureConfigured()
        {
            if (!_isConfigured)
            {
                throw new InvalidOperationException(
                    "Email service is not configured. Please configure EmailSettings in appsettings.json");
            }
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            EnsureConfigured(); // ✅ Vérifie seulement quand on essaie d'envoyer

            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    EnableSsl = _enableSsl,
                    Credentials = new NetworkCredential(_username, _password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail!, _senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("✅ Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending email to {To}", to);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string to, string userName)
        {
            EnsureConfigured();
            var subject = "Bienvenue sur ECommerce Store! 🎉";
            var body = GetWelcomeEmailTemplate(userName);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendOrderConfirmationEmailAsync(string to, string orderNumber, decimal totalAmount)
        {
            EnsureConfigured();
            var subject = $"Confirmation de commande #{orderNumber} ✅";
            var body = GetOrderConfirmationTemplate(orderNumber, totalAmount);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendOrderShippedEmailAsync(string to, string orderNumber, string trackingNumber)
        {
            EnsureConfigured();
            var subject = $"Votre commande #{orderNumber} a été expédiée! 📦";
            var body = GetOrderShippedTemplate(orderNumber, trackingNumber);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetToken)
        {
            EnsureConfigured();
            var subject = "Réinitialisation de votre mot de passe 🔐";
            var body = GetPasswordResetTemplate(resetToken);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendOrderDeliveredEmailAsync(string to, string orderNumber)
        {
            EnsureConfigured();
            var subject = $"Votre commande #{orderNumber} a été livrée! 🎁";
            var body = GetOrderDeliveredTemplate(orderNumber);
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendRefundConfirmationEmailAsync(string to, string orderNumber, decimal refundAmount)
        {
            EnsureConfigured();
            var subject = $"Remboursement confirmé pour la commande #{orderNumber} 💰";
            var body = GetRefundConfirmationTemplate(orderNumber, refundAmount);
            await SendEmailAsync(to, subject, body);
        }

        // Templates... (garde les mêmes que précédemment)
        private string GetWelcomeEmailTemplate(string userName) =>
            $"<h1>Bienvenue {userName}!</h1><p>Merci de vous être inscrit.</p>";

        private string GetOrderConfirmationTemplate(string orderNumber, decimal totalAmount) =>
            $"<h1>Commande #{orderNumber} confirmée</h1><p>Montant: {totalAmount:C} CAD</p>";

        private string GetOrderShippedTemplate(string orderNumber, string trackingNumber) =>
            $"<h1>Commande #{orderNumber} expédiée</h1><p>Suivi: {trackingNumber}</p>";

        private string GetPasswordResetTemplate(string resetToken) =>
            $"<h1>Réinitialisation</h1><p>Token: {resetToken}</p>";

        private string GetOrderDeliveredTemplate(string orderNumber) =>
            $"<h1>Commande #{orderNumber} livrée</h1>";

        private string GetRefundConfirmationTemplate(string orderNumber, decimal refundAmount) =>
            $"<h1>Remboursement confirmé</h1><p>Commande: #{orderNumber}, Montant: {refundAmount:C}</p>";
    }
}