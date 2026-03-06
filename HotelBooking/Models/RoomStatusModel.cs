namespace HotelBooking.Models
{
    public class RoomStatusModel
    {
        public string RoomNumber {  get; set; }
        public string Status { get; set; }
        public int RoomId { get; internal set; }
    }
}
