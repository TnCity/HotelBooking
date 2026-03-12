using System;
using System.Collections.Generic;

namespace HotelBooking.Models
{
    public class BookingHistoryVM
    {
        public List<RoomStatusModel> Rooms { get; set; }
        public List<DateTime> Dates { get; set; }
        public List<BookingModel> Booking { get; set; }
    }
}