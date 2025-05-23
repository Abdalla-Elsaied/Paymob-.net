💳 Paymob Integration with .NET Web API
Seamless payment integration with Paymob using .NET 7+ and the Unified Checkout API.

🚀 Features
Initiate payment through Paymob Unified Checkout

Secure callback handling with HMAC validation

Dynamic integration ID handling (Card, Wallet, Kiosk)

Booking confirmation after successful payment

📦 Tech Stack
.NET 7 / ASP.NET Core Web API

Identity (UserManager)

HttpClient

Paymob Unified Intention API

HMAC SHA512 validation

⚙️ Configuration
Update your appsettings.json with your Paymob credentials:

json
Copy
Edit
"PaymobSettingkey": {
  "APIKey": "your_api_key",
  "Publishablekey": "your_public_key",
  "Secretkey": "your_secret_key",
  "IntegrationIdCard": "card_integration_id",
  "IntegrationIdKiosk": "kiosk_integration_id",
  "IntegrationIdWallet": "wallet_integration_id",
  "Hmac": "your_hmac_secret",
  "NotificationUrl": "https://yourdomain.com/api/payment/callback",
  "RedirectionUrl": "https://yourdomain.com/confirmation"
}
📤 Payment Flow Overview
1. Initiate Payment
POST /api/payment/initiate

Request Body:

json
Copy
Edit
{
  "bookingId": 123,
  "paymentMethod": "card" // or "wallet", "kiosk"
}
Response:

json
Copy
Edit
{
  "success": true,
  "paymentUrl": "https://accept.paymob.com/unifiedcheckout/?publicKey=...&clientSecret=...",
  "transactionId": "booking-123-638482..."
}
2. Callback Handling
Paymob will call:

POST /api/payment/callback?hmac=...

Parses and validates the callback data

Extracts bookingId from special_reference

Confirms booking payment if success = true

Validates HMAC signature for security

🔒 HMAC Validation
To validate Paymob callback authenticity:

Extract obj from the callback JSON.

Flatten and concatenate required fields (as per Paymob docs).

Hash with HMAC-SHA512 using your Hmac secret.

Compare with the received hmac in query string.

If the HMACs match, the callback is valid ✅

📌 Important Tips
Always validate ModelState before initiating payment.

Use special_reference to uniquely track bookings.

Log all Paymob responses (especially in failure cases).

Don't skip HMAC validation — it's critical for security.

🧠 Example Use Case
Used in a travel booking app:

User books a trip → system sends a Paymob payment link.

On successful payment, Paymob calls the callback endpoint.

The system verifies and marks the booking as confirmed.

🤝 Contributions
Feel free to fork, improve, or suggest better handling for payment statuses, error codes, or retry logic.

📬 Contact
For questions, feel free to reach out on LinkedIn or open an issue.

