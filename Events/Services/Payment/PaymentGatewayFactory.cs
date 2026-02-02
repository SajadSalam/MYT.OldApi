using Events.Entities;

namespace Events.Services.Payment
{
    public interface IPaymentGatewayFactory
    {
        /// <summary>
        /// Gets the payment gateway for the specified provider
        /// </summary>
        IPaymentGateway GetGateway(PaymentProvider provider);

        /// <summary>
        /// Gets all available (enabled) payment gateways
        /// </summary>
        IEnumerable<IPaymentGateway> GetAvailableGateways();

        /// <summary>
        /// Checks if a specific payment provider is available
        /// </summary>
        bool IsProviderAvailable(PaymentProvider provider);
    }

    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IEnumerable<IPaymentGateway> _gateways;
        private readonly ILogger<PaymentGatewayFactory> _logger;

        public PaymentGatewayFactory(
            IEnumerable<IPaymentGateway> gateways,
            ILogger<PaymentGatewayFactory> logger)
        {
            _gateways = gateways;
            _logger = logger;
        }

        public IPaymentGateway GetGateway(PaymentProvider provider)
        {
            var gateway = _gateways.FirstOrDefault(g => g.GetProviderType() == provider);

            if (gateway == null)
            {
                _logger.LogError("Payment gateway not found for provider: {Provider}", provider);
                throw new InvalidOperationException($"Payment gateway not found for provider: {provider}");
            }

            if (!gateway.IsEnabled())
            {
                _logger.LogWarning("Payment gateway {Provider} is not enabled/configured", provider);
                throw new InvalidOperationException($"Payment gateway {provider} is not enabled or configured properly");
            }

            return gateway;
        }

        public IEnumerable<IPaymentGateway> GetAvailableGateways()
        {
            return _gateways.Where(g => g.IsEnabled());
        }

        public bool IsProviderAvailable(PaymentProvider provider)
        {
            var gateway = _gateways.FirstOrDefault(g => g.GetProviderType() == provider);
            return gateway != null && gateway.IsEnabled();
        }
    }
}

