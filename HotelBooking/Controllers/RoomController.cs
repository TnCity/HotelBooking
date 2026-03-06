using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using HotelBooking.Models;

public class RoomController : Controller
{
    private readonly IConfiguration _configuration;

    public RoomController(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    // 1️⃣ ROOM STATUS PAGE   
    // ==============================================================================================
    public IActionResult Index()
    {
        List<RoomStatusModel> roomList = new List<RoomStatusModel>();

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("GetRoomStatus", con);
            cmd.CommandType = CommandType.StoredProcedure;

            con.Open();

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                roomList.Add(new RoomStatusModel
                {
                    RoomNumber = reader["RoomNumber"].ToString(),
                    Status = reader["Status"].ToString()
                });
            }
        }

        return View(roomList);
    }

    //--------------------------------------------------------------------------------------------------------------
    //  BOOKING FORM (GET)

    [HttpGet]
    public IActionResult Book(int? roomId, DateTime? checkIn)
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
        else
        {
            model.CheckInDate = DateTime.Today;
            model.CheckOutDate = DateTime.Today.AddDays(1);
        }

        ViewBag.Rooms = GetRooms();

        return View(model);
    }

    // 3️⃣ ADD BOOKING (POST)
    [HttpPost]
    public IActionResult Book(BookingModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Rooms = GetRooms();
            return View(model);
        }

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("AddBooking", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@RoomId", model.RoomId);
                cmd.Parameters.AddWithValue("@CustomerName", model.CustomerName);
                cmd.Parameters.AddWithValue("@CheckInDate", model.CheckInDate);
                cmd.Parameters.AddWithValue("@CheckOutDate", model.CheckOutDate);

                con.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            ViewBag.Rooms = GetRooms();
            return View(model);
        }
    }

    //----------------------------------------------------------------------------------------------------------------

    // 4️⃣ LOAD ROOMS FOR DROPDOWN

    private List<SelectListItem> GetRooms()
    {
        List<SelectListItem> rooms = new List<SelectListItem>();

        string conn = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection con = new SqlConnection(conn))
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
}