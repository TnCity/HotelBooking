

using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Models
{
    public class BookingModel
    {
        public int BookingId { get; set; }
        [Required]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Customer Name is required")]
        public string CustomerName { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }
    }
}