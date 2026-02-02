# Payment System Documentation

## Overview

The payment system uses Amwal (PayTabs Iraq) as the primary payment provider. The system is built with a flexible, extensible architecture that standardizes payment operations through abstraction.

## Architecture

### Key Components

1. **Payment Gateway Interface (`IPaymentGateway`)**
   - Common interface for all payment providers
   - Standardizes payment operations across different gateways

2. **Payment Gateway Factory (`IPaymentGatewayFactory`)**
   - Factory pattern for selecting the appropriate payment gateway
   - Manages payment provider instances

3. **Payment Gateway**
   - `AmwalPaymentGateway` - Amwal (PayTabs Iraq) payment integration
   - Supports IQD currency
   - Easy to add more providers if needed

### Directory Structure

```
Services/Payment/
├── IPaymentGateway.cs              # Payment gateway interface
├── PaymentGatewayFactory.cs        # Factory for gateway selection
└── AmwalPaymentGateway.cs          # Amwal (PayTabs) implementation

DATA/DTOs/Payment/
├── PaymentRequest.cs               # Unified payment request
├── PaymentResponse.cs              # Unified payment response
├── PaymentStatusResponse.cs        # Payment status response
└── PaymentMethodDto.cs             # Available payment methods

DATA/DTOs/Amwal/
├── AmwalCreateOrderRequest.cs      # PayTabs order creation
├── AmwalCreateOrderResponse.cs     # PayTabs order response
├── AmwalCallbackDto.cs             # PayTabs callback payload
├── AmwalQueryRequest.cs            # PayTabs status query
└── AmwalQueryResponse.cs           # PayTabs query response

Controllers/
└── PaymentController.cs            # Payment endpoints and callbacks
```

## API Endpoints

### Get Available Payment Methods
```
GET /api/payment/methods
```

Returns a list of all enabled payment providers.

**Response:**
```json
[
  {
    "provider": 0,
    "name": "Amwal",
    "displayName": "Amwal",
    "isEnabled": true,
    "description": "Amwal (PayTabs Iraq) payment gateway"
  },
  {
    "provider": 3,
    "name": "Cash",
    "displayName": "Cash",
    "isEnabled": true,
    "description": "Cash payment"
  }
]
```

### Create a Booking with Payment Method
```
POST /api/book/{eventId}
```

**Request Body:**
```json
{
  "objects": ["A-1", "A-2"],
  "fullName": "John Doe",
  "phoneNumber": "+1234567890",
  "discount": 0,
  "preferredPaymentMethod": 0
}
```

**Payment Method Values:**
- `0` - Amwal (default) - PayTabs Iraq payment gateway
- `3` - Cash

If `preferredPaymentMethod` is not provided, it defaults to Amwal.

### Check Payment Status
```
GET /api/payment/status/{billId}
```

Returns the current status of a payment.

**Response:**
```json
{
  "billId": "TST2603302432875",
  "status": "Paid",
  "paymentProvider": "Amwal",
  "amount": 10000,
  "paymentDate": "2025-11-18T10:30:00Z"
}
```

### Cancel Payment
```
POST /api/payment/cancel/{billId}
Authorization: Bearer {token}
```

Marks payment as canceled in database. Cannot cancel paid bills.

**Note**: Amwal (PayTabs) cancellations must be processed manually through the PayTabs merchant dashboard.

### Refund Payment
```
POST /api/payment/refund/{billId}
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "amount": 10000
}
```

Marks payment as refunded in database. If amount is not provided, full refund amount is recorded.

**Note**: Amwal (PayTabs) refunds must be processed manually through the PayTabs merchant dashboard.

## Callback Endpoints

### Amwal (PayTabs) Callback
```
POST /api/payment/amwal/callback
```

Receives payment notifications from PayTabs after payment processing.

**Important**: This endpoint must be publicly accessible (HTTPS in production) and configured in your PayTabs account.

**Callback Payload:**
- `tran_ref` - PayTabs transaction reference
- `cart_id` - Your booking ID
- `payment_result.response_status` - Payment status code
  - `A` - Approved (successful)
  - `D` - Declined (failed)
  - `E` - Error (failed)
  - `V` - Voided (canceled)
  - `H` - Hold (pending)
  - `P` - Pending

**Response**: Always return HTTP 200/201 to acknowledge receipt.

## Configuration

### appsettings.json

```json
{
  "Amwal": {
    "ProfileId": "174796",
    "ServerKey": "YOUR_SERVER_KEY",
    "BaseUrl": "https://secure-iraq.paytabs.com",
    "CallbackUrl": "https://yourdomain.com/api/payment/amwal/callback",
    "ReturnUrl": "https://yourdomain.com/payment/return"
  }
}
```

### appsettings.Development.json

```json
{
  "Amwal": {
    "ProfileId": "174796",
    "ServerKey": "YOUR_TEST_SERVER_KEY",
    "BaseUrl": "https://secure-iraq.paytabs.com",
    "CallbackUrl": "http://localhost:5000/api/payment/amwal/callback",
    "ReturnUrl": "http://localhost:3000/payment/return"
  }
}
```

**Note:** For local development, use ngrok or similar to expose your callback URL publicly.

## Database Schema

### Bill Entity Updates

New fields added:
- `PaymentProvider` (int, nullable) - Payment provider used
- `PaymentProviderId` (string, nullable) - External payment ID
- `PaymentMetadata` (string, nullable) - JSON metadata

### Payment Status Enum

```csharp
public enum PaymentStatus
{
    NotPaid = 0,
    Paid = 1,
    Canceled = 2,
    Refunded = 3,
    Failed = 4
}
```

### Payment Provider Enum

```csharp
public enum PaymentProvider
{
    Amwal = 0,
    Cash = 3
}
```

## Adding a New Payment Provider

The system is designed to support multiple payment providers. To add a new provider:

### 1. Create Gateway Implementation

```csharp
// Services/Payment/NewProviderPaymentGateway.cs
public class NewProviderPaymentGateway : IPaymentGateway
{
    public PaymentProvider GetProviderType() => PaymentProvider.NewProvider;
    
    public string GetProviderName() => "New Provider";
    
    public bool IsEnabled()
    {
        return !string.IsNullOrEmpty(_apiKey);
    }
    
    // Implement all IPaymentGateway methods...
}
```

### 2. Add to PaymentProvider Enum

```csharp
// Entities/Bill.cs
public enum PaymentProvider
{
    Amwal = 0,
    NewProvider = 1,  // Add new provider
    Cash = 3
}
```

### 3. Register in DI Container

```csharp
// Extensions/ApplicationServicesExtension.cs
services.AddHttpClient<NewProviderPaymentGateway>();
services.AddScoped<IPaymentGateway, NewProviderPaymentGateway>();
```

### 4. Add Configuration

```json
{
  "NewProvider": {
    "ApiKey": "your_api_key",
    "SecretKey": "your_secret_key"
  }
}
```

## Flow Diagrams

### Booking Creation Flow

```
User → BookController
  → BookService.CreateBook()
    → Validate event and seats
    → Determine payment provider (from request or default)
    → PaymentGatewayFactory.GetGateway(provider)
    → Gateway.CreatePaymentAsync()
    → Save Bill with payment details
    → Return booking info to user
```

### Payment Callback Flow

```
PayTabs → PaymentController.AmwalCallback
  → Verify transaction reference exists
  → Check payment_result.response_status
  → Update Bill status in database
  → Update Book.IsPaid if payment approved (status = "A")
  → Return 200 OK
```

## Testing

### Test Amwal (PayTabs) Integration

Use PayTabs test cards:
- **Success**: `4111 1111 1111 1111`
- **Decline**: `4000 0000 0000 0002`
- **CVV**: Any 3 digits
- **Expiry**: Any future date

### Local Testing

For local development, use ngrok to expose your callback endpoint:

```bash
ngrok http 5000
```

Use the ngrok URL as your `CallbackUrl` in appsettings.Development.json.

## Migration

The system has been migrated from Sadid to Amwal. To apply database migration:

```bash
cd Events
dotnet ef migrations add ReplaceProvidersWithAmwal
dotnet ef database update
```

Or run the SQL scripts in `Migrations/ReplaceProvidersWithAmwal.txt`.

## Migration Notes

- Legacy Sadid and Stripe payment providers have been replaced with Amwal
- Existing bills have been migrated to Amwal provider
- Original provider information preserved in `PaymentMetadata` for audit
- Point of Sale users continue to auto-confirm payments
- All new payments use Amwal (PayTabs Iraq)

## Security Notes

1. **Callback Security**
   - PayTabs callbacks don't include signatures
   - Validate transaction by querying PayTabs API if suspicious
   - Always verify payment status before marking as paid

2. **API Keys**
   - Never commit server keys to source control
   - Use environment variables or Azure Key Vault in production
   - Rotate keys every 90 days

3. **PCI Compliance**
   - Never store credit card numbers
   - PayTabs handles all card data securely
   - Only store transaction references (tran_ref)

4. **HTTPS Requirements**
   - Production callback URL must use HTTPS
   - Valid SSL certificate required
   - Self-signed certificates not accepted

## Troubleshooting

### Payment Provider Not Available

**Error:** "Payment provider X is not available"

**Solution:** Check that the provider's API key is configured in appsettings.json

### Callback Not Receiving Events

**Solution:**
1. Verify callback URL is publicly accessible (use ngrok for local testing)
2. Check URL configured in PayTabs dashboard matches your configuration
3. Verify SSL certificate is valid (production only)
4. Check PayTabs dashboard for callback attempt logs
5. Ensure your server returns HTTP 200/201

### Payment Confirmation Fails

**Solution:**
1. Check payment provider API status
2. Verify API credentials are correct
3. Check logs for detailed error messages
4. Ensure sufficient permissions on API key

## Support

For issues or questions:
1. Check logs in `logs/log.txt`
2. Review payment provider dashboards
3. Test with sandbox/test credentials first
4. Contact payment provider support if needed

