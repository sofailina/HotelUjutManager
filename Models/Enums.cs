namespace HotelUjutManager.Models
{
    public static class Enums
    {
        public enum RoomStatus
        {
            Clean,
            Dirty,
            Occupied,
            Reserved,
            Maintenance
        }
        public enum BookingStatus
        {
            Confirmed,
            Active,
            Completed,
            Cancelled
        }
        public enum PaymentStatus
        {
            Paid,
            Unpaid,
            PartiallyPaid,
            Refunded
        }
        public enum CleaningStatus
        {
            Assigned,
            InProgress,
            Completed,
            Cancelled
        }
    }
}