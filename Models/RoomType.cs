using System.Collections.Generic;

namespace HotelUjutManager.Models
{
    public class RoomType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PricePerNight { get; set; }
        public virtual ICollection<Room> Rooms { get; set; }
    }
}