namespace PaymentPaymob.Models
{
    public class TravelItemBooked
    {
        public TravelItemBooked() { }
        public TravelItemBooked(int travelId, string title, string travelProfileUrl)
        {
            TravelId = travelId;
            Title = title;
            TravelProfileUrl = travelProfileUrl;
        }

        public int TravelId { get; set; }
        public string Title { get; set; }
        public string TravelProfileUrl { get; set; }
    }
}
