using System.Text;
using Events.DATA.DTOs.Amwal;
using Events.DATA.DTOs.Payment;
using Events.Entities;
using Newtonsoft.Json;

namespace Events.Services.Payment
{
    /// <summary>
    /// Amwal (PayTabs Iraq) payment gateway implementation
    /// </summary>
    public class AmwalPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverKey;
        private readonly int _profileId;
        private readonly string _baseUrl;
        private readonly string _callbackUrl;
        private readonly string _returnUrl;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AmwalPaymentGateway> _logger;

        public AmwalPaymentGateway(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AmwalPaymentGateway> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            _serverKey = configuration["Amwal:ServerKey"] ?? "";
            _profileId = int.TryParse(configuration["Amwal:ProfileId"], out var pid) ? pid : 0;
            _baseUrl = configuration["Amwal:BaseUrl"] ?? "https://secure-iraq.paytabs.com";
            _callbackUrl = configuration["Amwal:CallbackUrl"] ?? "";
            _returnUrl = configuration["Amwal:ReturnUrl"] ?? "";
        }

        public PaymentProvider GetProviderType() => PaymentProvider.Amwal;

        public string GetProviderName() => "Amwal";

        public bool IsEnabled()
        {
            return !string.IsNullOrEmpty(_serverKey) && _profileId > 0;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
        {
            try
            {
                var url = $"{_baseUrl}/payment/request";

                var amwalRequest = new AmwalCreateOrderRequest
                {
                    profile_id = _profileId,
                    tran_type = "sale",
                    tran_class = "ecom",
                    cart_id = request.BookId.ToString(),
                    cart_description = $"Event Booking - {request.CustomerName}",
                    cart_currency = "IQD",
                    cart_amount = request.Amount,
                    callback = _callbackUrl,
                    @return = _returnUrl
                };

                var jsonBody = JsonConvert.SerializeObject(amwalRequest);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("authorization", _serverKey);

                _logger.LogInformation("Creating Amwal payment for BookId: {BookId}, Amount: {Amount} IQD", 
                    request.BookId, request.Amount);

                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var amwalResponse = JsonConvert.DeserializeObject<AmwalCreateOrderResponse>(responseBody);

                    _logger.LogInformation("Amwal payment created successfully. TranRef: {TranRef}", 
                        amwalResponse?.tran_ref);

                    return new PaymentResponse
                    {
                        IsSuccess = true,
                        PaymentId = amwalResponse?.tran_ref,
                        PaymentUrl = amwalResponse?.redirect_url,
                        Error = null,
                        AdditionalData = new Dictionary<string, object>
                        {
                            { "cart_id", amwalResponse?.cart_id ?? "" },
                            { "trace", amwalResponse?.trace ?? "" },
                            { "merchant_id", amwalResponse?.merchantId ?? 0 }
                        }
                    };
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Amwal payment creation failed. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorBody);

                    return new PaymentResponse
                    {
                        IsSuccess = false,
                        PaymentId = null,
                        Error = $"Payment creation failed: {errorBody}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Amwal payment creation");
                
                return new PaymentResponse
                {
                    IsSuccess = false,
                    PaymentId = null,
                    Error = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentId)
        {
            try
            {
                var url = $"{_baseUrl}/payment/query";

                var queryRequest = new AmwalQueryRequest
                {
                    profile_id = _profileId,
                    tran_ref = paymentId
                };

                var jsonBody = JsonConvert.SerializeObject(queryRequest);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("authorization", _serverKey);

                _logger.LogInformation("Querying Amwal payment status for TranRef: {TranRef}", paymentId);

                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var queryResponse = JsonConvert.DeserializeObject<AmwalQueryResponse>(responseBody);

                    var status = ParsePaymentStatus(queryResponse?.payment_result?.response_status);
                    
                    decimal.TryParse(queryResponse?.cart_amount, out var amount);

                    _logger.LogInformation("Amwal payment status: {Status} for TranRef: {TranRef}", 
                        status, paymentId);

                    return new PaymentStatusResponse
                    {
                        IsSuccess = true,
                        Status = status,
                        PaymentId = paymentId,
                        Amount = amount,
                        PaymentDate = queryResponse?.payment_result?.transaction_time,
                        Error = null
                    };
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Amwal payment status query failed: {Error}", errorBody);

                    return new PaymentStatusResponse
                    {
                        IsSuccess = false,
                        Status = PaymentStatus.NotPaid,
                        PaymentId = paymentId,
                        Error = $"Status query failed: {errorBody}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Amwal payment status query");
                
                return new PaymentStatusResponse
                {
                    IsSuccess = false,
                    Status = PaymentStatus.NotPaid,
                    PaymentId = paymentId,
                    Error = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<PaymentResponse> ConfirmPaymentAsync(string paymentId, string? additionalData = null)
        {
            // PayTabs (Amwal) handles payment confirmation automatically through their system
            // and sends callback to our webhook. This method is not needed but required by interface.
            _logger.LogInformation("ConfirmPaymentAsync called for TranRef: {TranRef}. Amwal auto-confirms via callback.", 
                paymentId);

            // Query the current status to verify payment
            var statusResponse = await GetPaymentStatusAsync(paymentId);
            
            return new PaymentResponse
            {
                IsSuccess = statusResponse.IsSuccess && statusResponse.Status == PaymentStatus.Paid,
                PaymentId = paymentId,
                Error = statusResponse.IsSuccess ? null : "Payment not confirmed yet"
            };
        }

        public async Task<PaymentResponse> CancelPaymentAsync(string paymentId)
        {
            // PayTabs (Amwal) does not provide a direct API for cancellation
            // Cancellations are typically handled through the merchant dashboard
            _logger.LogWarning("CancelPaymentAsync called for TranRef: {TranRef}. Amwal does not support API cancellation.", 
                paymentId);

            return new PaymentResponse
            {
                IsSuccess = false,
                PaymentId = paymentId,
                Error = "Payment cancellation must be done through PayTabs merchant dashboard"
            };
        }

        public async Task<PaymentResponse> RefundPaymentAsync(string paymentId, decimal amount)
        {
            // PayTabs (Amwal) refunds are typically handled through the merchant dashboard
            // or specific refund API endpoint if available
            _logger.LogWarning("RefundPaymentAsync called for TranRef: {TranRef}, Amount: {Amount}. Amwal refunds handled via dashboard.", 
                paymentId, amount);

            return new PaymentResponse
            {
                IsSuccess = false,
                PaymentId = paymentId,
                Error = "Refunds must be processed through PayTabs merchant dashboard"
            };
        }

        private PaymentStatus ParsePaymentStatus(string? responseStatus)
        {
            return responseStatus?.ToUpper() switch
            {
                "A" => PaymentStatus.Paid,      // Approved
                "H" => PaymentStatus.NotPaid,   // Hold
                "P" => PaymentStatus.NotPaid,   // Pending
                "V" => PaymentStatus.Canceled,  // Voided
                "E" => PaymentStatus.Failed,    // Error
                "D" => PaymentStatus.Failed,    // Declined
                _ => PaymentStatus.NotPaid
            };
        }
    }
}
