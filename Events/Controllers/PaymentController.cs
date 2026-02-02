using Events.DATA;
using Events.DATA.DTOs.Amwal;
using Events.DATA.DTOs.Payment;
using Events.Entities;
using Events.Services.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Events.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly IPaymentGatewayFactory _paymentGatewayFactory;
        private readonly DataContext _context;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IPaymentGatewayFactory paymentGatewayFactory,
            DataContext context,
            ILogger<PaymentController> logger,
            IConfiguration configuration)
        {
            _paymentGatewayFactory = paymentGatewayFactory;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get all available payment methods
        /// </summary>
        [HttpGet("methods")]
        public IActionResult GetPaymentMethods()
        {
            var availableGateways = _paymentGatewayFactory.GetAvailableGateways();

            var methods = availableGateways.Select(gateway => new PaymentMethodDto
            {
                Provider = gateway.GetProviderType(),
                Name = gateway.GetProviderType().ToString(),
                DisplayName = gateway.GetProviderName(),
                IsEnabled = gateway.IsEnabled(),
                Description = GetProviderDescription(gateway.GetProviderType())
            }).ToList();

            return Ok(methods);
        }

        /// <summary>
        /// Check payment status
        /// </summary>
        [HttpGet("status/{billId}")]
        public async Task<IActionResult> GetPaymentStatus(string billId)
        {
            var bill = await _context.Bills
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BillId == billId);

            if (bill == null)
            {
                return NotFound(new { error = "Bill not found" });
            }

            var paymentProvider = bill.PaymentProvider ?? PaymentProvider.Amwal;

            try
            {
                var gateway = _paymentGatewayFactory.GetGateway(paymentProvider);
                var statusResponse = await gateway.GetPaymentStatusAsync(billId);

                if (statusResponse.IsSuccess)
                {
                    // Update bill status if changed
                    if (bill.PaymentStatus != statusResponse.Status)
                    {
                        var trackedBill = await _context.Bills.FindAsync(bill.Id);
                        if (trackedBill != null)
                        {
                            trackedBill.PaymentStatus = statusResponse.Status;
                            if (statusResponse.Status == PaymentStatus.Paid && trackedBill.PaymentDate == null)
                            {
                                trackedBill.PaymentDate = DateTime.UtcNow;
                            }
                            await _context.SaveChangesAsync();
                        }
                    }

                    return Ok(new
                    {
                        billId,
                        status = statusResponse.Status.ToString(),
                        paymentProvider = paymentProvider.ToString(),
                        amount = statusResponse.Amount,
                        paymentDate = statusResponse.PaymentDate
                    });
                }

                return BadRequest(new { error = statusResponse.Error });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status for bill {BillId}", billId);
                return StatusCode(500, new { error = "Error checking payment status" });
            }
        }

        /// <summary>
        /// Amwal (PayTabs) callback handler
        /// </summary>
        [HttpPost("amwal/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> AmwalCallback([FromBody] AmwalCallbackDto callback)
        {
            try
            {
                _logger.LogInformation("Received Amwal callback. TranRef: {TranRef}, Status: {Status}", 
                    callback.tran_ref, callback.payment_result?.response_status);

                // Find bill by tran_ref (which is stored as BillId)
                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.BillId == callback.tran_ref);

                if (bill == null)
                {
                    _logger.LogWarning("Bill not found for Amwal callback. TranRef: {TranRef}", callback.tran_ref);
                    return NotFound(new { error = "Bill not found" });
                }

                // Check response_status: "A" = Approved
                if (callback.payment_result?.response_status == "A")
                {
                    // Payment approved
                    if (bill.PaymentStatus != PaymentStatus.Paid)
                    {
                        bill.PaymentStatus = PaymentStatus.Paid;
                        bill.PaymentDate = callback.payment_result.transaction_time ?? DateTime.UtcNow;

                        // Update book as paid
                        var book = await _context.Books.FindAsync(bill.BookId);
                        if (book != null)
                        {
                            book.IsPaid = true;
                        }

                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Payment approved and processed. TranRef: {TranRef}, BillId: {BillId}", 
                            callback.tran_ref, bill.BillId);
                    }

                    return Ok(new { success = true, message = "Payment processed successfully" });
                }
                else
                {
                    // Payment failed or declined
                    bill.PaymentStatus = callback.payment_result?.response_status switch
                    {
                        "D" => PaymentStatus.Failed,    // Declined
                        "E" => PaymentStatus.Failed,    // Error
                        "V" => PaymentStatus.Canceled,  // Voided
                        _ => PaymentStatus.NotPaid
                    };

                    await _context.SaveChangesAsync();

                    _logger.LogWarning("Payment not approved. TranRef: {TranRef}, Status: {Status}, Message: {Message}", 
                        callback.tran_ref, 
                        callback.payment_result?.response_status,
                        callback.payment_result?.response_message);

                    // Still return 200 to acknowledge receipt
                    return Ok(new { success = true, message = "Payment status updated" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Amwal callback");
                // Return 200 even on error to prevent PayTabs from retrying
                return Ok(new { success = false, error = "Error processing callback" });
            }
        }

        /// <summary>
        /// Cancel a payment
        /// </summary>
        [HttpPost("cancel/{billId}")]
        [Authorize]
        public async Task<IActionResult> CancelPayment(string billId)
        {
            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.BillId == billId);

            if (bill == null)
            {
                return NotFound(new { error = "Bill not found" });
            }

            // Check if payment can be canceled
            if (bill.PaymentStatus == PaymentStatus.Paid)
            {
                return BadRequest(new { error = "Cannot cancel a paid bill. Use refund instead." });
            }

            var paymentProvider = bill.PaymentProvider ?? PaymentProvider.Amwal;

            try
            {
                var gateway = _paymentGatewayFactory.GetGateway(paymentProvider);
                var cancelResult = await gateway.CancelPaymentAsync(billId);

                if (cancelResult.IsSuccess)
                {
                    bill.PaymentStatus = PaymentStatus.Canceled;
                    await _context.SaveChangesAsync();

                    return Ok(new { success = true, message = "Payment canceled successfully" });
                }

                return BadRequest(new { error = cancelResult.Error });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling payment for bill {BillId}", billId);
                return StatusCode(500, new { error = "Error canceling payment" });
            }
        }

        /// <summary>
        /// Refund a payment
        /// </summary>
        [HttpPost("refund/{billId}")]
        [Authorize]
        public async Task<IActionResult> RefundPayment(string billId, [FromBody] RefundRequestDto request)
        {
            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.BillId == billId);

            if (bill == null)
            {
                return NotFound(new { error = "Bill not found" });
            }

            if (bill.PaymentStatus != PaymentStatus.Paid)
            {
                return BadRequest(new { error = "Can only refund paid bills" });
            }

            var paymentProvider = bill.PaymentProvider ?? PaymentProvider.Amwal;
            var refundAmount = request.Amount ?? bill.TotalPrice ?? 0;

            try
            {
                var gateway = _paymentGatewayFactory.GetGateway(paymentProvider);
                var refundResult = await gateway.RefundPaymentAsync(billId, refundAmount);

                if (refundResult.IsSuccess)
                {
                    bill.PaymentStatus = PaymentStatus.Refunded;
                    await _context.SaveChangesAsync();

                    return Ok(new { success = true, message = "Payment refunded successfully" });
                }

                return BadRequest(new { error = refundResult.Error });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment for bill {BillId}", billId);
                return StatusCode(500, new { error = "Error refunding payment" });
            }
        }

        private string GetProviderDescription(PaymentProvider provider)
        {
            return provider switch
            {
                PaymentProvider.Amwal => "Amwal (PayTabs Iraq) payment gateway",
                PaymentProvider.Cash => "Cash payment",
                _ => "Unknown payment provider"
            };
        }

    }
}

namespace Events.Controllers
{
    /// <summary>
    /// Request body for refund endpoint
    /// </summary>
    public class RefundRequestDto
    {
        public decimal? Amount { get; set; }
    }
}

