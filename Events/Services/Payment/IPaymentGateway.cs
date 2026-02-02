using Events.DATA.DTOs.Payment;
using Events.Entities;

namespace Events.Services.Payment
{
    /// <summary>
    /// Common interface for all payment gateway providers
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Creates a payment/bill in the payment provider system
        /// </summary>
        Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request);

        /// <summary>
        /// Confirms/captures a payment
        /// </summary>
        Task<PaymentResponse> ConfirmPaymentAsync(string paymentId, string? additionalData = null);

        /// <summary>
        /// Cancels a pending payment
        /// </summary>
        Task<PaymentResponse> CancelPaymentAsync(string paymentId);

        /// <summary>
        /// Gets the current status of a payment
        /// </summary>
        Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentId);

        /// <summary>
        /// Processes a refund for a paid transaction
        /// </summary>
        Task<PaymentResponse> RefundPaymentAsync(string paymentId, decimal amount);

        /// <summary>
        /// Returns the payment provider type this gateway handles
        /// </summary>
        PaymentProvider GetProviderType();

        /// <summary>
        /// Returns the display name of this payment provider
        /// </summary>
        string GetProviderName();

        /// <summary>
        /// Checks if this payment provider is currently enabled/configured
        /// </summary>
        bool IsEnabled();
    }
}

