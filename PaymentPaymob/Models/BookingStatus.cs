using System.Runtime.Serialization;

namespace PaymentPaymob.Models
{
    public enum BookingStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,
        [EnumMember(Value = "Payment Successed")]
        PaymentSuccessed,
        [EnumMember(Value = "Payment Failed")]
        PaymentFailed,
    }
}
