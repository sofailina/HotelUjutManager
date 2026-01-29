using System;

namespace HotelUjutManager.Models
{
    public class Cleaning
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime CleaningDate { get; set; }
        public string Status { get; set; }
        public virtual Room Room { get; set; }
        public virtual Employee Employee { get; set; }
    }
}