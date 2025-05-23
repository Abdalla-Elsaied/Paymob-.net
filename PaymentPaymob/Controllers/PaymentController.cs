using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentPaymob.Interface;
using PaymentPaymob.Models;

namespace PaymentPaymob.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymobServices _paymobService;
        private readonly IBookingService _bookingService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymobServices paymobService,
            IBookingService bookingService,
            ILogger<PaymentController> logger)
        {
            _paymobService = paymobService;
            _bookingService = bookingService;
            _logger = logger;
        }
        [Authorize]
        /// <summary>
        /// Initiate a payment request and return the payment URL.
        /// </summary>
        [HttpPost("initiate")]
        public async Task<ActionResult<PaymentResponseDto>> InitiatePayment([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _paymobService.InitiatePaymentAsync(request);
            if (!response.Success)
            {
                _logger.LogError($"Payment initiation failed: {response.Message}");
                return BadRequest(response);
            }

            return Ok(response);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        /// <summary>
        /// Callback endpoint for Paymob webhook notification
        /// </summary>
        [HttpPost("callback")]
        public async Task<IActionResult> Callback()
        {

            var hmacReceived = HttpContext.Request.Query["hmac"].ToString();
            // Read the request body
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            _logger.LogInformation($"Received Paymob callback: {payload}");

            // Process and validate the callback data
            var paymentData = await _paymobService.ProcessPaymentCallbackAsync(payload, hmacReceived);


            if (paymentData == null)
            {
                _logger.LogError("Failed to parse payment callback data");
                return BadRequest("Invalid callback data");
            }
            //handle Hmac verfication

            // Process successful payment
            if (paymentData.Success && paymentData.BookingId > 0)
            {
                // Update booking status
                var confirmed = await _bookingService.ConfirmBookingPaymentAsync(paymentData.BookingId);

                if (confirmed)
                {
                    _logger.LogInformation($"Payment confirmed for booking {paymentData.BookingId}");
                    return Ok(new { message = "Payment processed successfully", bookingId = paymentData.BookingId });
                }
                else
                {
                    _logger.LogWarning($"Booking confirmation failed for booking {paymentData.BookingId}");
                    return BadRequest("Booking confirmation failed");
                }
            }
            else
            {
                _logger.LogWarning($"Payment not successful: {payload}");
                return Ok(new { message = "Payment not successful" });
            }
        }
    }
}
