using System.Threading.Tasks;

namespace ECommerce.Domain.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Envoie un email générique
        /// </summary>
        Task SendEmailAsync(string to, string subject, string body);

        /// <summary>
        /// Envoie un email de bienvenue après inscription
        /// </summary>
        Task SendWelcomeEmailAsync(string to, string userName);

        /// <summary>
        /// Envoie un email de confirmation de commande
        /// </summary>
        Task SendOrderConfirmationEmailAsync(string to, string orderNumber, decimal totalAmount);

        /// <summary>
        /// Envoie un email quand la commande est expédiée
        /// </summary>
        Task SendOrderShippedEmailAsync(string to, string orderNumber, string trackingNumber);

        /// <summary>
        /// Envoie un email de réinitialisation de mot de passe
        /// </summary>
        Task SendPasswordResetEmailAsync(string to, string resetToken);

        /// <summary>
        /// Envoie un email de livraison confirmée
        /// </summary>
        Task SendOrderDeliveredEmailAsync(string to, string orderNumber);

        /// <summary>
        /// Envoie un email de remboursement
        /// </summary>
        Task SendRefundConfirmationEmailAsync(string to, string orderNumber, decimal refundAmount);
    }
}