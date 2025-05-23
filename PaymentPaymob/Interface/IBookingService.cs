using PaymentPaymob.Models;

namespace PaymentPaymob.Interface
{
    public interface IBookingService
    {
        Task<Booking?> CreateBookingAsync(string buyerEmail, BookingDto bookingDto);

        Task<IReadOnlyCollection<Booking>?> GetBookingForUserById(string buyerEmail);

        Task<Booking?> UpdateBookingAsync(int id, string buyerEmail, BookingDto bookingDto);
        Task<bool> DeleteBookingAsync(int id, string buyerEmail);

        Task<bool> ConfirmBookingPaymentAsync(int bookingId);
    }
}
