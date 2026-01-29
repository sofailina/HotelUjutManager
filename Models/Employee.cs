using System.Collections.Generic;

namespace HotelUjutManager.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public virtual ICollection<Cleaning> Cleanings { get; set; }
    }
}