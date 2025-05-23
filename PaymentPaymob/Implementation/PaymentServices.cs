using Microsoft.AspNetCore.Identity;
using PaymentPaymob.Interface;
using PaymentPaymob.Models;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace PaymentPaymob.Implementation
{
    public class PaymentServices: IPaymobServices
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PaymentServices> _logger;
        private readonly string? _apiKey;
        private readonly string? _publicKey;
        private readonly string? _secretKey;
        private readonly string? _integrationIdCard;
        private readonly string? _integrationIdKiosk;
        private readonly string? _integrationIdWallet;
        private readonly string? _hmacSecret;

        public PaymentServices(
            HttpClient httpClient,
            IConfiguration configuration,
            IGenericRepository<Booking> Booking,
            UserManager<IdentityUser> userManager,
            ILogger<PaymentServices> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _bookingRepository = Booking;
            _userManager = userManager;
            _logger = logger;

            // Load configuration settings
            _apiKey = _configuration["PaymobSettingkey:APIKey"];
            _publicKey = _configuration["PaymobSettingkey:Publishablekey"];
            _secretKey = _configuration["PaymobSettingkey:Secretkey"];
            _integrationIdCard = _configuration["PaymobSettingkey:IntegrationIdCard"];
            _integrationIdKiosk = _configuration["PaymobSettingkey:IntegrationIdKiosk"];
            _integrationIdWallet = _configuration["PaymobSettingkey:IntegrationIdWallet"];
            _hmacSecret = _configuration["PaymobSettingkey:Hmac"];

        }



        public async Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request)
        {



            // Get booking and user information
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                _logger.LogWarning($"Booking not found: {request.BookingId}");
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = "Booking not found"
                };
            }



            var user = await _userManager.FindByEmailAsync(booking.BuyerEmail);

            // Create payment intention
            var intentionResult = await CreateIntentionAsync(booking, user, request);

            // Generate payment URL for the unified checkout
            var paymentUrl = $"https://accept.paymob.com/unifiedcheckout/?publicKey={_publicKey}&clientSecret={intentionResult.ClientSecret}";



            return new PaymentResponseDto
            {
                Success = true,
                PaymentUrl = paymentUrl,
                TransactionId = intentionResult.SpecialReference,
                Message = "Payment initiated successfully"
            };
        }



        private async Task<(string ClientSecret, string SpecialReference)> CreateIntentionAsync(Booking booking, IdentityUser user, PaymentRequestDto request)
        {
            // Generate a more robust special reference with booking ID and timestamp
            var specialReference = $"booking-{booking.Id}-{DateTime.UtcNow.Ticks}";

            // Handle payment method types 
            string integrationId = DetermineIntegrationId(request.PaymentMethod);

            // Prepare billing data
            var billingData = CreateBillingData(booking, user);

            // Prepare request payload according to Paymob documentation
            var payload = new
            {
                amount = (int)(booking.TotalCost * 100), // Convert to cents
                currency = "EGP",
                payment_methods = new object[] { int.Parse(integrationId) }, // Use integration ID from config
                billing_data = billingData,
                items = new[] {
                new {
                    name = $"Booking #{booking.Id}",
                    amount = (int)(booking.TotalCost * 100), // Convert to cents
                    description = $"Travel Booking Payment for booking #{booking.Id}",
                    quantity = 1
                }
            },
                customer = new
                {
                    first_name = billingData.first_name,
                    last_name = billingData.last_name,
                    email = billingData.email,
                    extras = new { bookingId = booking.Id }
                },
                extras = new
                {
                    bookingId = booking.Id,
                    customerId = user?.Id ?? "guest"
                },
                special_reference = specialReference,
                expiration = 3600, // 1 hour expiration
                notification_url = _configuration["PaymobSettingkey:NotificationUrl"],
                redirection_url = _configuration["PaymobSettingkey:RedirectionUrl"]
            };

            // Create HTTP request for Paymob's intention API
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://accept.paymob.com/v1/intention/");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Token", _secretKey); // Use Token prefix and your API key
            requestMessage.Content = JsonContent.Create(payload);

            // Send the request and process response
            var response = await _httpClient.SendAsync(requestMessage);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Paymob API error: {response.StatusCode}, Response: {responseContent}");
                throw new Exception($"Paymob Intention API call failed with status {response.StatusCode}: {responseContent}");
            }



            // Parse the response to get client_secret
            var resultJson = JsonDocument.Parse(responseContent);
            var clientSecret = resultJson.RootElement.GetProperty("client_secret").GetString();

            return (clientSecret, specialReference);
        }

        private string DetermineIntegrationId(string paymentMethod)
        {
            return paymentMethod?.ToLower() switch
            {
                "card" => _integrationIdCard ?? throw new ArgumentException("Card integration ID not configured"),
                "kiosk" => _integrationIdKiosk ?? throw new ArgumentException("Kiosk integration ID not configured"),
                "wallet" => _integrationIdWallet ?? throw new ArgumentException("Wallet integration ID not configured"),
                _ => throw new ArgumentException($"Invalid payment method: {paymentMethod}")
            };
        }

        private dynamic CreateBillingData(Booking booking, IdentityUser user)
        {
            return new
            {
                apartment = "N/A",
                first_name = user?.UserName ?? booking.BuyerEmail.Split('@')[0],
                last_name = "Customer",
                street = "N/A",
                building = "N/A",
                phone_number = user?.PhoneNumber ?? "+201000000000",
                country = "EGP",
                email = user?.Email ?? booking.BuyerEmail,
                floor = "N/A",
                state = "N/A",
                city = "N/A"
            };
        }

        public async Task<PaymentCallbackDto?> ProcessPaymentCallbackAsync(string payload, string hmacReceived)
        {
            _logger.LogInformation($"Processing payment callback: {payload}");

            // Parse the callback payload
            var jsonDocument = JsonDocument.Parse(payload);
            var root = jsonDocument.RootElement;

            if (!root.TryGetProperty("obj", out var objElement))
            {
                _logger.LogWarning("Root does not contain 'obj' field.");
                return null;
            }
            // Extract transaction and order details
            //bool success = objElement.TryGetProperty("success", out var successElement);
            bool isSuccess = false;
            if (objElement.TryGetProperty("success", out JsonElement successElement))
            {
                isSuccess = successElement.GetBoolean();
                Console.WriteLine($"Success: {isSuccess}");
            }
            else
            {
                Console.WriteLine("Could not find the 'success' property.");
            }

            // Check if necessary properties exist
            if (!objElement.TryGetProperty("order", out var orderElement))
            {
                _logger.LogWarning("Order element not found in callback payload");
                return null;
            }

            string? merchantOrderId = null;
            if (orderElement.TryGetProperty("merchant_order_id", out var merchantOrderIdElement))
            {
                merchantOrderId = merchantOrderIdElement.ToString();
            }
            else
            {
                // Try to extract booking ID from special_reference if available
                if (objElement.TryGetProperty("special_reference", out var specialRefElement))
                {
                    var specialRef = specialRefElement.GetString();
                    if (specialRef != null && specialRef.StartsWith("booking-"))
                    {
                        var parts = specialRef.Split('-');
                        if (parts.Length > 1)
                        {
                            merchantOrderId = parts[1];
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(merchantOrderId))
            {
                _logger.LogWarning("Could not determine booking ID from callback payload");
                return null;
            }

            // Extract amount and transaction ID
            int amountCents = 0;
            if (objElement.TryGetProperty("amount_cents", out var amountElement))
            {
                amountCents = amountElement.GetInt32();
            }

            string transactionId = "unknown";
            if (objElement.TryGetProperty("id", out var idElement))
            {
                transactionId = idElement.ToString();
            }
            var bookingId = 0;
            if (objElement.TryGetProperty("payment_key_claims", out var claimsElement) &&
               claimsElement.TryGetProperty("extra", out var extraElement) &&
                extraElement.TryGetProperty("bookingId", out var bookingIdElement))
            {
                bookingId = bookingIdElement.GetInt32();
            }
            // Create callback data object
            var callbackData = new PaymentCallbackDto
            {
                Success = isSuccess,
                BookingId = bookingId,
                Amount = amountCents / 100.0m, // Convert cents to currency units
                TransactionId = transactionId,
                TransactionDate = DateTime.UtcNow,
                RawData = payload
            };

            _logger.LogInformation($"Parsed payment callback - Success: {isSuccess}, BookingId: {callbackData.BookingId}, Amount: {callbackData.Amount}, TransactionId: {transactionId}");

            if (string.IsNullOrEmpty(_hmacSecret))
            {
                _logger.LogWarning("HMAC secret not configured, skipping verification");
                return callbackData;
            }

            var hmacSecret = _hmacSecret; // from appsettings

            //Convert JSON to Dictionary<string, object>
            var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(objElement.ToString());

            if (!await ValidateHmac(dataDict!, hmacReceived, hmacSecret))
                return null;
            return callbackData;
        }

        private async Task<bool> ValidateHmac(Dictionary<string, object> data, string receivedHmac, string secretKey)
        {
            var fields = new[]
            {
        "amount_cents", "created_at", "currency", "error_occured", "has_parent_transaction", "id",
        "integration_id", "is_3d_secure", "is_auth", "is_capture", "is_refunded", "is_standalone_payment",
        "is_voided", "order.id", "owner", "pending", "source_data.pan", "source_data.sub_type",
        "source_data.type", "success"
    };

            var values = new List<string>();
            foreach (var field in fields)
            {
                string[] path = field.Split('.');
                object? current = data;
                if (current is Dictionary<string, object> dict && dict.TryGetValue(path[0], out var next))
                {
                    if (path.Length > 1)
                    {

                        var help = JsonSerializer.Deserialize<Dictionary<string, object>>((JsonElement)next);
                        help.TryGetValue(path[1], out var next2);
                        if (next2.ToString() == "True" || next2.ToString() == "False")
                        {
                            next2 = next2.ToString().ToLower();
                        }
                        current = next2;
                    }
                    else
                    {
                        if (next.ToString() == "True" || next.ToString() == "False")
                        {
                            next = next.ToString().ToLower();
                        }
                        current = next;
                    }
                }
                else
                {
                    current = null;
                    break;
                }

                values.Add(current?.ToString() ?? string.Empty);
            }

            string concatenated = string.Concat(values);

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenated));
            var computedHmac = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            return (computedHmac == receivedHmac.ToLowerInvariant());
        }
    }
}
