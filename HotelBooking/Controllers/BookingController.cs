using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using HotelBooking.Models;

public class BookingController : Controller
{
    private readonly IConfiguration _configuration;

    public BookingController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // ==================================================================================================================
    // BOOK ROOM (GET)
    // ==========================
    [HttpGet]
    public IActionResult Create(int? roomId, DateTime? checkIn)
    {
        BookingModel model = new BookingModel();

        if (roomId != null)
        {
            model.RoomId = roomId.Value;
        }

        if (checkIn != null)
        {
            model.CheckInDate = checkIn.Value;
            model.CheckOutDate = checkIn.Value.AddDays(1);
        }

        ViewBag.Rooms = GetRooms();

        // Show success message after payment redirect
        if (TempData.ContainsKey("Success"))
        {
            ViewBag.Success = TempData["Success"].ToString();
        }

        return View(model);
    }

    // ==================================================================================================================
    // BOOK ROOM (POST)
    // ==============================//its ok but for payment chenge it.
    //[HttpPost]  
    //public IActionResult Create(BookingModel model)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        ViewBag.Rooms = GetRooms();
    //        return View(model);
    //    }

    //    string connectionString = _configuration.GetConnectionString("DefaultConnection");

    //    using (SqlConnection con = new SqlConnection(connectionString))
    //    {
    //        SqlCommand cmd = new SqlCommand("AddBooking", con);
    //        cmd.CommandType = CommandType.StoredProcedure;

    //        cmd.Parameters.AddWithValue("@RoomId", model.RoomId);
    //        cmd.Parameters.AddWithValue("@CustomerName", model.CustomerName);
    //        cmd.Parameters.AddWithValue("@CheckInDate", model.CheckInDate);
    //        cmd.Parameters.AddWithValue("@CheckOutDate", model.CheckOutDate);

    //        con.Open();
    //        cmd.ExecuteNonQuery();
    //    }


    //    return RedirectToAction("History");
    //}
    [HttpPost]
    public IActionResult Create(BookingModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Rooms = GetRooms();
            return View(model);
        }

        // Store booking data temporarily for payment
        TempData["RoomId"] = model.RoomId;
        TempData["CustomerName"] = model.CustomerName;
        TempData["CheckInDate"] = model.CheckInDate;
        TempData["CheckOutDate"] = model.CheckOutDate;

        // Keep TempData for next request
        TempData.Keep();

        // Redirect to Stripe payment
        return RedirectToAction("Pay", "Payment");
    }

    // ==================================================================================================================
    // ALL BOOKINGS LIST
    // =============================
    public IActionResult List()
    {
        return View(GetBookings("GetAllBookings"));
    }

    public IActionResult FromToday()
    {
        return View("List", GetBookings("GetBookingsFromToday"));
    }

    private List<BookingListModel> GetBookings(string procedureName)
    {
        List<BookingListModel> bookingList = new List<BookingListModel>();

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand(procedureName, con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                bookingList.Add(new BookingListModel
                {
                    BookingId = Convert.ToInt32(reader["BookingId"]),
                    RoomNumber = reader["RoomNumber"].ToString(),
                    CustomerName = reader["CustomerName"].ToString(),
                    CheckInDate = Convert.ToDateTime(reader["CheckInDate"]),
                    CheckOutDate = Convert.ToDateTime(reader["CheckOutDate"])
                });
            }
        }

        return bookingList;
    }

    // ==================================================================================================================
    // BOOKING HISTORY GRID
    // ==================================
    public IActionResult History(DateTime? fromDate, DateTime? toDate)
    {
        DateTime startDate = fromDate ?? DateTime.Today;
        DateTime endDate = toDate ?? startDate.AddMonths(1);

        if (endDate < startDate)
        {
            endDate = startDate.AddDays(7);
        }

        List<DateTime> dates = new List<DateTime>();

        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        {
            dates.Add(date);
        }

        var rooms = GetAllRooms();
        var bookings = GetAllBookingsRaw();

        BookingHistoryVM model = new BookingHistoryVM
        {
            Rooms = rooms,
            Dates = dates,
            Booking = bookings
        };

        ViewBag.FromDate = startDate.ToString("yyyy-MM-dd");
        ViewBag.ToDate = endDate.ToString("yyyy-MM-dd");

        return View(model);
    }

    // ==================================================================================================================
    // LOAD ROOMS FOR DROPDOWN
    // ==================================
    private List<SelectListItem> GetRooms()
    {
        List<SelectListItem> rooms = new List<SelectListItem>();

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("SELECT RoomId, RoomNumber FROM Rooms", con);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                rooms.Add(new SelectListItem
                {
                    Value = reader["RoomId"].ToString(),
                    Text = reader["RoomNumber"].ToString()
                });
            }
        }

        return rooms;
    }

    // ==================================================================================================================
    // GET ALL ROOMS (FOR HISTORY GRID)
    // =======================================
    private List<RoomStatusModel> GetAllRooms()
    {
        List<RoomStatusModel> rooms = new List<RoomStatusModel>();

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("SELECT RoomId, RoomNumber FROM Rooms", con);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                rooms.Add(new RoomStatusModel
                {
                    RoomId = Convert.ToInt32(reader["RoomId"]),
                    RoomNumber = reader["RoomNumber"].ToString()
                });
            }
        }

        return rooms;
    }

    // ==================================================================================================================
    // GET ALL BOOKINGS FOR GRID CHECK
    // ========================================
    private List<BookingModel> GetAllBookingsRaw()
    {
        List<BookingModel> booking = new List<BookingModel>();

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("SELECT RoomId, CustomerName, CheckInDate, CheckOutDate FROM Booking", con);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                booking.Add(new BookingModel
                {
                    RoomId = Convert.ToInt32(reader["RoomId"]),
                    CustomerName = reader["CustomerName"].ToString(),
                    CheckInDate = Convert.ToDateTime(reader["CheckInDate"]),
                    CheckOutDate = Convert.ToDateTime(reader["CheckOutDate"])
                });
            }
        }

        return booking;
    }

    // ============================================================
    // SAVE GRID CHANGES
    // ============================================================
    [HttpPost]
    public IActionResult SaveAvailability([FromBody] List<AvailabilityModel> changes)
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            foreach (var item in changes)
            {
                if (item.Status == "Booked")
                {
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Booking(RoomId,CustomerName,CheckInDate,CheckOutDate) VALUES(@RoomId,'AdminBlock',@Date,DATEADD(day,1,@Date))",
                        con);

                    cmd.Parameters.AddWithValue("@RoomId", item.RoomId);
                    cmd.Parameters.AddWithValue("@Date", item.Date);

                    cmd.ExecuteNonQuery();
                }
                else
                {
                    SqlCommand cmd = new SqlCommand(
                        "DELETE FROM Booking WHERE RoomId=@RoomId AND @Date >= CheckInDate AND @Date < CheckOutDate",
                        con);

                    cmd.Parameters.AddWithValue("@RoomId", item.RoomId);
                    cmd.Parameters.AddWithValue("@Date", item.Date);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        return Json(new { success = true });
    }
}