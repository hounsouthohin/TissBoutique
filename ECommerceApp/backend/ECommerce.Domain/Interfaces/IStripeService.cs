using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Interfaces
{
    public interface IStripeService
    {
        /// <summary>
        /// Crée un Payment Intent pour un paiement
        /// </summary>
        /// <param name="amount">Montant en dollars (sera converti en centimes)</param>
        /// <param name="currency">Code devise (CAD, USD, EUR...)</param>
        /// <param name="metadata">Métadonnées (OrderId, UserId, etc.)</param>
        /// <returns>Client Secret pour compléter le paiement côté client</returns>
        Task<string> CreatePaymentIntentAsync(decimal amount, string currency, Dictionary<string, string> metadata);

        /// <summary>
        /// Confirme un Payment Intent (côté serveur)
        /// </summary>
        /// <param name="paymentIntentId">ID du Payment Intent</param>
        /// <returns>True si le paiement est réussi</returns>
        Task<bool> ConfirmPaymentAsync(string paymentIntentId);

        /// <summary>
        /// Rembourse un paiement (total ou partiel)
        /// </summary>
        /// <param name="paymentIntentId">ID du Payment Intent</param>
        /// <param name="amount">Montant à rembourser (null = remboursement total)</param>
        /// <returns>True si le remboursement est réussi</returns>
        Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null);

        /// <summary>
        /// Crée un client Stripe
        /// </summary>
        /// <param name="email">Email du client</param>
        /// <param name="name">Nom du client</param>
        /// <param name="metadata">Métadonnées additionnelles</param>
        /// <returns>ID du client Stripe créé</returns>
        Task<string> CreateCustomerAsync(string email, string name, Dictionary<string, string> metadata);

        /// <summary>
        /// Récupère les détails d'un Payment Intent
        /// </summary>
        /// <param name="paymentIntentId">ID du Payment Intent</param>
        /// <returns>Objet contenant les détails du paiement</returns>
        Task<object> GetPaymentDetailsAsync(string paymentIntentId);

        /// <summary>
        /// Annule un Payment Intent (avant confirmation)
        /// </summary>
        /// <param name="paymentIntentId">ID du Payment Intent</param>
        /// <returns>True si l'annulation est réussie</returns>
        Task<bool> CancelPaymentIntentAsync(string paymentIntentId);

        /// <summary>
        /// Crée une session de paiement Checkout (alternative au Payment Intent)
        /// </summary>
        /// <param name="amount">Montant</param>
        /// <param name="currency">Devise</param>
        /// <param name="successUrl">URL de redirection en cas de succès</param>
        /// <param name="cancelUrl">URL de redirection en cas d'annulation</param>
        /// <param name="metadata">Métadonnées</param>
        /// <returns>URL de la session Checkout</returns>
        Task<string> CreateCheckoutSessionAsync(
            decimal amount,
            string currency,
            string successUrl,
            string cancelUrl,
            Dictionary<string, string> metadata);

        /// <summary>
        /// Attache un mode de paiement à un client
        /// </summary>
        /// <param name="paymentMethodId">ID du mode de paiement</param>
        /// <param name="customerId">ID du client Stripe</param>
        /// <returns>True si l'attachement est réussi</returns>
        Task<bool> AttachPaymentMethodAsync(string paymentMethodId, string customerId);

        /// <summary>
        /// Liste les modes de paiement d'un client
        /// </summary>
        /// <param name="customerId">ID du client Stripe</param>
        /// <returns>Liste des modes de paiement</returns>
        Task<List<object>> ListPaymentMethodsAsync(string customerId);

        /// <summary>
        /// Récupère le statut d'un remboursement
        /// </summary>
        /// <param name="refundId">ID du remboursement</param>
        /// <returns>Statut du remboursement (succeeded, pending, failed)</returns>
        Task<string> GetRefundStatusAsync(string refundId);
    }
}