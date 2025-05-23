namespace PaymentPaymob.Models
{
    public class PaymentCallbackDto
    {
        public bool Success { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string RawData { get; set; }
    }
}
