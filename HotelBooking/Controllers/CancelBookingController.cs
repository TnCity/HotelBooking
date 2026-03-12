using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HotelBooking.Controllers
{
    public class CancelBookingController : Controller
    {
        private readonly IConfiguration _configuration;

        public CancelBookingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /*
        PSEUDOCODE / PLAN (detailed):
        1. Accept an optional integer id parameter (int? id) so model binding won't fail if missing.
        2. If id is null:
           a. Attempt to read from route values (RouteData.Values["id"]).
           b. If still null, attempt to read from query string (Request.Query["id"]).
        3. If id is still null after these attempts, return HTTP 400 Bad Request with a helpful message.
        4. If id is present:
           a. Parse/convert to int safely.
           b. Open a SQL connection using the configured connection string.
           c. Use a parameterized DELETE command to remove the booking with the given id.
           d. ExecuteNonQuery and check affected rows:
              - If > 0, return JSON { success = true }.
              - If 0, return HTTP 404 Not Found with { success = false, message = "Booking not found" }.
        5. Wrap DB operations in try/catch and return HTTP 500 on unexpected errors.
        6. This approach allows id to be supplied via route (/CancelBooking/123), query (?id=123),
           or as a route value from client-side calls that post the id into the URL.
        */

        [HttpGet]
        [Route("CancelBooking")]
        [Route("CancelBooking/{id}")]
        public IActionResult CancelBooking(int? id)
        {
            // Try route values and query string if model binding didn't supply id
            if (!id.HasValue)
            {
                if (RouteData.Values.TryGetValue("id", out var routeId) && routeId != null)
                {
                    if (int.TryParse(routeId.ToString(), out var parsedRouteId))
                        id = parsedRouteId;
                }
            }

            if (!id.HasValue)
            {
                if (int.TryParse(Request.Query["id"], out var parsedQueryId))
                    id = parsedQueryId;
            }

            if (!id.HasValue)
            {
                return BadRequest(new { success = false, message = "Missing booking id. Provide id as route (/CancelBooking/{id}) or query (?id=123)." });
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                int affectedRows = 0;

                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Booking WHERE BookingId = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", id.Value);
                    con.Open();
                    affectedRows = cmd.ExecuteNonQuery();
                }

                if (affectedRows > 0)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return NotFound(new { success = false, message = "Booking not found." });
                }
            }
            catch (Exception ex)
            {
                // Log exception as appropriate (not shown here).
                return StatusCode(500, new { success = false, message = "An error occurred while cancelling the booking.", detail = ex.Message });
            }
        }
    }
}