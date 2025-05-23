namespace PaymentPaymob.Models
{
    public class Booking
    {
        public Booking()
        {

        }
        public Booking(string buyerEmail, BookingItem item, decimal totalCost)
        {
            BuyerEmail = buyerEmail;
            Item = item;
            TotalCost = totalCost;
        }
        public int Id { get; set; }
        public string BuyerEmail { get; set; }
        public DateTimeOffset BookingDate { get; set; } = DateTimeOffset.UtcNow;
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public BookingItem Item { get; set; }   //will  be 1 - 1 relationship  own   >> map in the same table 
        public decimal TotalCost { get; set; }
        public string PaymentIntentId { get; set; } = " ";
    }
}
