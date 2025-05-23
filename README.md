💳 Paymob Payment Integration with Web API
A flexible and secure integration with Paymob's Unified Checkout using Web API.

🚀 Features
Generate direct payment links through Paymob

Automatically confirm bookings after successful payment

Support for multiple payment methods: card, wallet, kiosk

Secure Paymob callbacks using HMAC verification

Dynamic Integration ID handling based on payment method

🛠️ Technologies Used
ASP.NET Core Web API

Paymob Unified Intention API

HMAC for data validation

User management via UserManager

HttpClient for API communication with Paymob

🔄 Payment Flow
1. Initiate Payment
The user sends a request to start a new payment, specifying the booking and the payment method.

2. Generate Payment Link
Your system sends the data to Paymob, receives a payment link, and returns it to the user.

3. Complete Payment
Once the user completes the payment, Paymob sends a callback to your system to confirm the transaction.
