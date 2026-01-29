namespace HotelUjutManager.Models
{
    public class BookingService
    {
        public int BookingId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtTime { get; set; }
        public virtual Booking Booking { get; set; }
        public virtual Service Service { get; set; }
    }
}