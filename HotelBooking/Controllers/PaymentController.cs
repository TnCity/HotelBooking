using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Microsoft.Data.SqlClient;
using System.Data;

public class PaymentController : Controller
{
    private readonly IConfiguration _configuration;

    public PaymentController(IConfiguration configuration)
    {
        _configuration = configuration;

        // Set Stripe secret key
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    // Stripe Payment Page
    public IActionResult Pay()
    {
        // Read booking info from TempData (use Peek so values are not consumed)
        var roomIdObj = TempData.Peek("RoomId");
        var customerNameObj = TempData.Peek("CustomerName");
        var checkInObj = TempData.Peek("CheckInDate");
        var checkOutObj = TempData.Peek("CheckOutDate");

        // Build success and cancel URLs including booking data so Stripe can redirect back with it
        string successUrl = Url.Action("Success", "Payment", new
        {
            roomId = roomIdObj,
            customerName = customerNameObj,
            checkIn = checkInObj,
            checkOut = checkOutObj
        }, Request.Scheme);

        string cancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme);

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },

            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = 10000, // ₹100 (Stripe uses paise)
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Hotel Room Booking"
                        }
                    },
                    Quantity = 1
                }
            },

            Mode = "payment",

            SuccessUrl = successUrl,    
            CancelUrl = cancelUrl
        };

        var service = new SessionService();
        Session session = service.Create(options);

        return Redirect(session.Url);
    }

    // Payment Success → Save Booking
    public IActionResult Success(int roomId, string customerName, DateTime checkIn, DateTime checkOut)
    {
        // Save booking to DB using data returned in query string
        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("AddBooking", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@RoomId", roomId);
            cmd.Parameters.AddWithValue("@CustomerName", customerName);
            cmd.Parameters.AddWithValue("@CheckInDate", checkIn);
            cmd.Parameters.AddWithValue("@CheckOutDate", checkOut);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // Set a success message to be displayed on the booking page
        TempData["Success"] = "Payment successful. Booking confirmed.";

        return RedirectToAction("Create", "Booking");
    }

    // Payment Cancel
    public IActionResult Cancel()
    {
        return Content("Payment Cancelled");
    }
}