using System;
using System.Collections.Generic;

namespace HotelUjutManager.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int GuestId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; }
        public virtual Guest Guest { get; set; }
        public virtual Room Room { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<BookingService> BookingServices { get; set; }
    }
}