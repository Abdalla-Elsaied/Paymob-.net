# Paymob Integration with ASP.NET Core Web API

A simple, open-source starter template for integrating **Paymob Unified Checkout** with ASP.NET Core Web API.

## 📌 Purpose

This repository is intended to help developers integrate Paymob’s payment gateway more easily into their .NET backends. It demonstrates the full flow — from creating a payment link to verifying payment via callback, with clean and modular code.

> ⚠️ This is an initial version and might be missing some error handling or edge case support. The code will be improved over time. Contributions and suggestions are welcome.

---

## 🔧 Features

* ✅ Integration with Paymob’s Unified Checkout API
* 🔐 HMAC verification to secure callback data
* 🔁 Automatic update of booking status after payment confirmation
* 🔄 Support for different payment methods (cards, wallets, cash)
* 🧩 Flexible structure to adapt to your application logic

---

## 📁 Project Structure

```
/Controllers
  └── PaymentController.cs
/DTOs
  └── PaymobRequestDto.cs
  └── CallbackDto.cs
/Services
  └── PaymobService.cs
  └── HmacService.cs
Helpers
  └── ApiCaller.cs
```

---

## 🚀 How It Works

1. **Create Payment Intent**
   The backend generates a payment link using your Paymob credentials.

2. **Customer Pays**
   The user is redirected to the Paymob page and completes the payment.

3. **Paymob Callback**
   Paymob calls your `/api/payment/callback` endpoint after payment with transaction details.

4. **Verify & Update**
   The system verifies the HMAC, checks payment success, and updates booking/payment status.

---

## 🛠️ Setup Instructions

1. Clone the repository:

   ```bash
   git clone https://github.com/Abdalla-Elsaied/Paymob-.net
   cd Paymob-.net
   ```

2. Configure the following settings in `appsettings.json`:

   ```json
   {
     "Paymob": {
       "ApiKey": "YOUR_API_KEY",
       "IframeId": "YOUR_IFRAME_ID",
       "IntegrationId": "YOUR_INTEGRATION_ID",
       "HmacSecret": "YOUR_HMAC_SECRET"
     }
   }
   ```

3. Run the application:

   ```bash
   dotnet run
   ```

---

## 🧪 Testing Callback

Use tools like [ngrok](https://ngrok.com/) to expose your local server to receive Paymob's callback.

---

## 💬 Feedback & Contributions

If you find a bug or want to suggest improvements, feel free to open an issue or submit a pull request. Every bit of feedback helps improve the integration!

---

## 📄 License

This project is open-source and available under the [MIT License](LICENSE).

---

