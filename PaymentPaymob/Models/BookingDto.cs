using System.ComponentModel.DataAnnotations;

namespace PaymentPaymob.Models
{
    public class BookingDto
    {
        [Required]
        public int TravelId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "At least one ticket must be booked")]
        public int Quantity { get; set; }
    }
}
