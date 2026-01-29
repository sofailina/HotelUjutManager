using System.Collections.Generic;

namespace HotelUjutManager.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string Floor { get; set; }
        public int RoomTypeId { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
        public virtual RoomType RoomType { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Cleaning> Cleanings { get; set; }
    }
}