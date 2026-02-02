# Amwal (PayTabs Iraq) Integration Guide

## Overview

This system integrates with Amwal (PayTabs Iraq) as the primary payment provider for processing event ticket payments. PayTabs provides a secure payment gateway specifically designed for the Iraqi market, supporting IQD currency.

## Payment Flow

```
1. User creates booking → System creates payment via PayTabs API
2. User redirected to PayTabs payment page
3. User completes payment (card, bank transfer, etc.)
4. PayTabs processes payment
5. PayTabs sends callback to our server
6. System updates booking status
7. User redirected back to our application
```

## API Endpoints

### Base URL
- **Production**: `https://secure-iraq.paytabs.com`
- **Testing**: Use same URL with test credentials

### 1. Create Payment Order

**Endpoint**: `POST /payment/request`

**Headers**:
```
authorization: YOUR_SERVER_KEY
content-type: application/json
```

**Request Body**:
```json
{
    "profile_id": 174796,
    "tran_type": "sale",
    "tran_class": "ecom",
    "cart_id": "4244b9fd-c7e9-4f16-8d3c-4fe7bf6c48ca",
    "cart_description": "Event Booking - Customer Name",
    "cart_currency": "IQD",
    "cart_amount": 10000,
    "callback": "https://yourdomain.com/api/payment/amwal/callback",
    "return": "https://yourdomain.com/payment/return"
}
```

**Response**:
```json
{
    "tran_ref": "TST2603302432875",
    "cart_id": "4244b9fd-c7e9-4f16-8d3c-4fe7bf6c48ca",
    "redirect_url": "https://secure-iraq.paytabs.com/payment/page/53730C4D...",
    "trace": "PMNT0805.6980C954.00004B2A"
}
```

### 2. Query Transaction Status

**Endpoint**: `POST /payment/query`

**Headers**:
```
authorization: YOUR_SERVER_KEY
content-type: application/json
```

**Request Body**:
```json
{
    "profile_id": 174796,
    "tran_ref": "TST2603302432875"
}
```

**Response**:
```json
{
    "tran_ref": "TST2603302432875",
    "cart_id": "4244b9fd-c7e9-4f16-8d3c-4fe7bf6c48ca",
    "cart_amount": "10000",
    "payment_result": {
        "response_status": "A",
        "response_code": "831000",
        "response_message": "Authorised",
        "transaction_time": "2020-05-28T14:35:38+04:00"
    }
}
```

## Payment Callback

PayTabs sends a POST request to your callback URL after payment processing.

**Your Callback Endpoint**: `POST /api/payment/amwal/callback`

**Callback Payload**:
```json
{
    "tran_ref": "TST2603302432875",
    "cart_id": "4244b9fd-c7e9-4f16-8d3c-4fe7bf6c48ca",
    "cart_amount": "10000",
    "payment_result": {
        "response_status": "A",
        "response_code": "831000",
        "response_message": "Authorised",
        "transaction_time": "2020-05-28T14:35:38+04:00"
    },
    "payment_info": {
        "card_type": "Credit",
        "card_scheme": "Visa",
        "payment_description": "4111 11## #### 1111"
    }
}
```

**Response Status Codes**:
- `A` - Approved (Payment successful)
- `H` - Hold (Payment on hold)
- `P` - Pending (Payment pending)
- `V` - Voided (Payment voided)
- `E` - Error (Payment error)
- `D` - Declined (Payment declined)

**Your Response**: Always return HTTP 200 or 201 to acknowledge receipt.

## Configuration

### Environment Variables

Add these to your `appsettings.json`:

```json
{
  "Amwal": {
    "ProfileId": "174796",
    "ServerKey": "SDJ9G99TG2-JMKZJTK96K-MHK2DG99NR",
    "BaseUrl": "https://secure-iraq.paytabs.com",
    "CallbackUrl": "https://yourdomain.com/api/payment/amwal/callback",
    "ReturnUrl": "https://yourdomain.com/payment/return"
  }
}
```

### Development Configuration

For local testing, update `appsettings.Development.json`:

```json
{
  "Amwal": {
    "ProfileId": "174796",
    "ServerKey": "YOUR_TEST_SERVER_KEY",
    "BaseUrl": "https://secure-iraq.paytabs.com",
    "CallbackUrl": "https://your-ngrok-url.ngrok.io/api/payment/amwal/callback",
    "ReturnUrl": "http://localhost:3000/payment/return"
  }
}
```

**Important**: For local development, use ngrok or similar to expose your callback endpoint publicly.

## Currency Handling

PayTabs Iraq uses **IQD (Iraqi Dinar)** as the primary currency. The system is configured to always use IQD.

**Important Notes**:
- Amounts are in whole IQD (no conversion needed)
- Example: 10000 IQD = 10,000 IQD
- No decimal points for IQD currency

## Error Handling

### HTTP Status Codes

- **2xx** - Success (payment request processed)
- **4xx** - Client error (check request parameters)
- **5xx** - Server error (PayTabs server issue, retry with backoff)

### Common Error Scenarios

1. **Invalid Server Key**
   - Error: `401 Unauthorized`
   - Solution: Verify `ServerKey` in configuration

2. **Invalid Profile ID**
   - Error: `400 Bad Request`
   - Solution: Verify `ProfileId` matches your PayTabs account

3. **Callback Not Received**
   - Cause: Callback URL not accessible
   - Solution: Ensure URL is publicly accessible (HTTPS in production)

4. **Payment Timeout**
   - User doesn't complete payment within time limit
   - Status remains `NotPaid` in database
   - Booking expires after 15 minutes

## Testing

### Test Cards

PayTabs provides test cards for sandbox testing:

**Successful Payment**:
- Card Number: `4111 1111 1111 1111`
- CVV: Any 3 digits
- Expiry: Any future date

**Declined Payment**:
- Card Number: `4000 0000 0000 0002`
- CVV: Any 3 digits
- Expiry: Any future date

### Testing Workflow

1. Create a booking with test credentials
2. Use test card on PayTabs payment page
3. Verify callback is received
4. Check database for payment status update
5. Verify booking is marked as paid

### Local Testing with ngrok

```bash
# Install ngrok
npm install -g ngrok

# Start your application on port 5000
dotnet run

# In another terminal, expose port 5000
ngrok http 5000

# Use the ngrok URL in your CallbackUrl configuration
# Example: https://abc123.ngrok.io/api/payment/amwal/callback
```

## Security Best Practices

### 1. Secure API Keys
- Never commit server keys to source control
- Use environment variables or Azure Key Vault in production
- Rotate keys periodically (every 90 days recommended)

### 2. Callback Validation
- Callback endpoint is publicly accessible (no authentication required)
- PayTabs doesn't sign callbacks (unlike Stripe)
- Validate transaction by querying PayTabs API if needed
- Always verify payment status before marking booking as paid

### 3. HTTPS Required
- Production callback URL MUST use HTTPS
- Self-signed certificates not accepted
- Ensure valid SSL certificate

### 4. PCI Compliance
- Never store credit card numbers
- Never log card details
- PayTabs handles all card data securely
- Only store transaction references

## Refunds and Cancellations

**Important**: PayTabs (Amwal) does not provide direct API for refunds or cancellations.

### Manual Process

1. **Cancellations**: 
   - System marks payment as `Canceled` in database
   - Admin must manually cancel in PayTabs merchant dashboard
   - Log warning indicates manual action required

2. **Refunds**:
   - System marks payment as `Refunded` in database
   - Admin must process refund through PayTabs dashboard
   - Document transaction reference for tracking

### Dashboard Access

- Login: [PayTabs Merchant Dashboard](https://secure-iraq.paytabs.com/merchant)
- Navigate to Transactions → Find transaction → Actions → Refund/Cancel

## Monitoring and Logging

### Key Logs to Monitor

```csharp
// Payment creation
LogInformation("Creating Amwal payment for BookId: {BookId}, Amount: {Amount} IQD")

// Callback received
LogInformation("Amwal callback: TranRef: {TranRef}, Status: {Status}")

// Payment approved
LogInformation("Payment approved: TranRef: {TranRef}")

// Payment failed
LogWarning("Payment not approved: TranRef: {TranRef}, Status: {Status}")

// Cancellation notice
LogInformation("Ticket canceled. Bill {BillId} marked as canceled. Manual cancellation required in PayTabs dashboard.")
```

### Monitoring Checklist

- [ ] Monitor callback endpoint uptime
- [ ] Track payment success rate
- [ ] Alert on failed payments (> 5% failure rate)
- [ ] Monitor callback latency
- [ ] Track manual refund requests

## Troubleshooting

### Issue: Callback Not Received

**Symptoms**: Payment completed but booking not marked as paid

**Solutions**:
1. Check callback URL is publicly accessible
2. Verify URL in PayTabs dashboard matches configuration
3. Check server logs for callback attempts
4. Ensure firewall allows PayTabs IPs
5. Test callback manually with curl

### Issue: Payment Status Stuck

**Symptoms**: Payment shows as pending indefinitely

**Solutions**:
1. Query transaction status via API
2. Check PayTabs dashboard for transaction status
3. Verify callback endpoint returned 200 status
4. Check for exceptions in callback handler

### Issue: Duplicate Payments

**Symptoms**: Multiple charges for single booking

**Solutions**:
1. Implement idempotency checks using `tran_ref`
2. Check for duplicate callback processing
3. Verify booking hold timeout is enforced

## Support and Resources

### PayTabs Support
- Email: support@paytabs.com
- Phone: Check PayTabs dashboard
- Documentation: [PayTabs API Docs](https://support.paytabs.com/en/support/solutions/articles/60000712315)

### Internal Support
- Check application logs: `logs/log.txt`
- Database: Query `Bills` table for payment status
- Dashboard: PayTabs merchant dashboard for transaction details

## Migration Notes

This system was migrated from Sadid payment provider to Amwal (PayTabs). Historical payment data has been updated:

- Previous Sadid payments marked as Amwal in database
- Original provider tracked in `PaymentMetadata` field
- All new payments use Amwal exclusively

See `Migrations/ReplaceProvidersWithAmwal.txt` for migration details.
