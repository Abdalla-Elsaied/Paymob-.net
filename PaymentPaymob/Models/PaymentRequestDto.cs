using System.ComponentModel.DataAnnotations;

namespace PaymentPaymob.Models
{
    public class PaymentRequestDto
    {

        [Required]
        public int BookingId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";

    }
}
