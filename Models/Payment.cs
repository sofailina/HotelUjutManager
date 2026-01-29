using System;

namespace HotelUjutManager.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Status { get; set; }
        public virtual Booking Booking { get; set; }
    }
}