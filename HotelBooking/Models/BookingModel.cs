

using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Models
{
    public class BookingModel
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }
    }
}