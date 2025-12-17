namespace server;

using MySql.Data.MySqlClient;

public static class Bookings
{
    // GET /bookings - Hämta inloggad användares bokningar
    public record BookingListItem(int Id, string HotelName, string CheckIn, string CheckOut, int Guests, decimal TotalPrice, string Status);
    
    public static async Task<IResult> GetMyBookings(Config config, HttpContext ctx)
    {
        var loginCheck = Auth.RequireLogin(ctx);
        if (loginCheck is not null) return loginCheck;

        int userId = Auth.GetUserId(ctx)!.Value;

        List<BookingListItem> result = new();
        string query = """
            SELECT b.id, h.name, b.check_in, b.check_out, b.guests, b.total_price, b.status
            FROM bookings b
            JOIN hotels h ON b.hotel_id = h.id
            WHERE b.user_id = @userId
            ORDER BY b.check_in DESC
        """;

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString, query, new MySqlParameter("@userId", userId)))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                    reader.GetDateTime(3).ToString("yyyy-MM-dd"),
                    reader.GetInt32(4),
                    reader.GetDecimal(5),
                    reader.GetString(6)
                ));
            }
        }
        return Results.Ok(result);
    }

    // GET /bookings/{id} - Hämta bokningsdetaljer
    public record BookingDetail(int Id, string HotelName, string HotelAddress, string CheckIn, string CheckOut, int Guests, decimal TotalPrice, string Status, List<BookedRoom> Rooms);
    public record BookedRoom(string RoomType, int Quantity, decimal PricePerNight);
    
    public static async Task<IResult> GetBooking(int id, Config config, HttpContext ctx)
    {
        var loginCheck = Auth.RequireLogin(ctx);
        if (loginCheck is not null) return loginCheck;

        int userId = Auth.GetUserId(ctx)!.Value;
        string? role = await Auth.GetUserRole(config, ctx);

        // Hämta bokning
        string query = """
            SELECT b.id, h.name, h.address, b.check_in, b.check_out, b.guests, b.total_price, b.status, b.user_id
            FROM bookings b
            JOIN hotels h ON b.hotel_id = h.id
            WHERE b.id = @id
        """;

        int bookingUserId;
        BookingDetail? booking = null;

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString, query, new MySqlParameter("@id", id)))
        {
            if (!reader.Read())
                return Results.NotFound("Bokningen finns inte");

            bookingUserId = reader.GetInt32(8);
            
            // Kolla behörighet: egen bokning eller admin
            if (role != "admin" && bookingUserId != userId)
                return Results.Unauthorized();

            string? address = reader.IsDBNull(2) ? null : reader.GetString(2);
            booking = new(
                reader.GetInt32(0),
                reader.GetString(1),
                address ?? "",
                reader.GetDateTime(3).ToString("yyyy-MM-dd"),
                reader.GetDateTime(4).ToString("yyyy-MM-dd"),
                reader.GetInt32(5),
                reader.GetDecimal(6),
                reader.GetString(7),
                new List<BookedRoom>()
            );
        }

        // Hämta rum i bokningen
        string roomQuery = """
            SELECT r.room_type, br.quantity, r.price_per_night
            FROM booking_rooms br
            JOIN rooms r ON br.room_id = r.id
            WHERE br.booking_id = @id
        """;

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString, roomQuery, new MySqlParameter("@id", id)))
        {
            while (reader.Read())
            {
                booking.Rooms.Add(new(
                    reader.GetString(0),
                    reader.GetInt32(1),
                    reader.GetDecimal(2)
                ));
            }
        }

        return Results.Ok(booking);
    }

    // POST /bookings - Skapa bokning
    public record CreateBookingData(int HotelId, string CheckIn, string CheckOut, int Guests, List<RoomBooking> Rooms);
    public record RoomBooking(int RoomId, int Quantity);
    
    public static async Task<IResult> CreateBooking(CreateBookingData data, Config config, HttpContext ctx)
    {
        var loginCheck = Auth.RequireLogin(ctx);
        if (loginCheck is not null) return loginCheck;

        int userId = Auth.GetUserId(ctx)!.Value;

        // Beräkna antal nätter
        var checkIn = DateTime.Parse(data.CheckIn);
        var checkOut = DateTime.Parse(data.CheckOut);
        int nights = (checkOut - checkIn).Days;

        if (nights < 1)
            return Results.BadRequest("Utcheckning måste vara efter incheckning");

        // Kolla tillgänglighet och beräkna pris
        decimal totalPrice = 0;

        foreach (var room in data.Rooms)
        {
            // Kolla tillgänglighet
            string availQuery = """
                SELECT r.total_rooms - COALESCE(SUM(br.quantity), 0) AS available, r.price_per_night
                FROM rooms r
                LEFT JOIN booking_rooms br ON r.id = br.room_id
                LEFT JOIN bookings b ON br.booking_id = b.id
                    AND b.status != 'cancelled'
                    AND b.check_in < @checkOut
                    AND b.check_out > @checkIn
                WHERE r.id = @roomId AND r.hotel_id = @hotelId
                GROUP BY r.id
            """;

            var availParams = new MySqlParameter[]
            {
                new("@roomId", room.RoomId),
                new("@hotelId", data.HotelId),
                new("@checkIn", data.CheckIn),
                new("@checkOut", data.CheckOut)
            };

            using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, availQuery, availParams))
            {
                if (!reader.Read())
                    return Results.BadRequest($"Rumstyp {room.RoomId} finns inte på hotellet");

                int available = reader.GetInt32(0);
                if (available < room.Quantity)
                    return Results.BadRequest($"Endast {available} rum tillgängliga av typ {room.RoomId}");

                decimal pricePerNight = reader.GetDecimal(1);
                totalPrice += pricePerNight * room.Quantity * nights;
            }
        }

        // Skapa bokning
        string insertBooking = """
            INSERT INTO bookings (user_id, hotel_id, check_in, check_out, guests, total_price, status)
            VALUES (@userId, @hotelId, @checkIn, @checkOut, @guests, @totalPrice, 'confirmed')
        """;

        var bookingParams = new MySqlParameter[]
        {
            new("@userId", userId),
            new("@hotelId", data.HotelId),
            new("@checkIn", data.CheckIn),
            new("@checkOut", data.CheckOut),
            new("@guests", data.Guests),
            new("@totalPrice", totalPrice)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertBooking, bookingParams);

        // Hämta booking ID
        int bookingId = 0;
        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, "SELECT LAST_INSERT_ID()"))
        {
            if (reader.Read())
                bookingId = reader.GetInt32(0);
        }

        // Lägg till rum i bokningen
        foreach (var room in data.Rooms)
        {
            string insertRoom = """
                INSERT INTO booking_rooms (booking_id, room_id, quantity)
                VALUES (@bookingId, @roomId, @quantity)
            """;

            var roomParams = new MySqlParameter[]
            {
                new("@bookingId", bookingId),
                new("@roomId", room.RoomId),
                new("@quantity", room.Quantity)
            };

            await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, insertRoom, roomParams);
        }

        return Results.Ok(new { BookingId = bookingId, TotalPrice = totalPrice, Status = "confirmed" });
    }

    // DELETE /bookings/{id} - Avboka (sätt status till cancelled)
    public static async Task<IResult> CancelBooking(int id, Config config, HttpContext ctx)
    {
        var loginCheck = Auth.RequireLogin(ctx);
        if (loginCheck is not null) return loginCheck;

        int userId = Auth.GetUserId(ctx)!.Value;
        string? role = await Auth.GetUserRole(config, ctx);

        // Kolla att bokningen tillhör användaren eller är admin
        string checkQuery = "SELECT user_id FROM bookings WHERE id = @id";
        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString, checkQuery, new MySqlParameter("@id", id)))
        {
            if (!reader.Read())
                return Results.NotFound("Bokningen finns inte");

            int bookingUserId = reader.GetInt32(0);
            if (role != "admin" && bookingUserId != userId)
                return Results.Unauthorized();
        }

        string query = "UPDATE bookings SET status = 'cancelled' WHERE id = @id";
        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, new MySqlParameter("@id", id));
        
        return Results.Ok("Bokningen avbokad");
    }
}
