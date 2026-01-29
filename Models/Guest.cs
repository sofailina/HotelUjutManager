using System;
using System.Collections.Generic;

namespace HotelUjutManager.Models
{
    public class Guest
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Passport { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}