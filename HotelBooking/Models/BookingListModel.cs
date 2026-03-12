namespace HotelBooking.Models
{
    public class BookingListModel
    {
        public int BookingId { get; set; }
        public string RoomNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}