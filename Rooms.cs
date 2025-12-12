namespace server;

using MySql.Data.MySqlClient;

public static class Rooms
{
    // Records för data
    public record RoomData(int Id, int HotelId, string RoomType, int Capacity, decimal PricePerNight, int TotalRooms);
    public record RoomCreate(int HotelId, string RoomType, int Capacity, decimal PricePerNight, int TotalRooms);
    public record RoomAvailability(int Id, string RoomType, int Capacity, decimal PricePerNight, int AvailableRooms);

    // POST /rooms/create - Skapa rumstyp (admin only)
    public static async Task<IResult> CreateRoom(Config config, RoomCreate data, HttpContext ctx)
    {
        var authCheck = await Auth.RequireAdmin(config, ctx);
        if (authCheck is not null)
            return authCheck;

        if (string.IsNullOrWhiteSpace(data.RoomType))
            return Results.BadRequest("Rumstyp krävs");

        if (data.Capacity < 1)
            return Results.BadRequest("Kapacitet måste vara minst 1");

        if (data.PricePerNight <= 0)
            return Results.BadRequest("Pris måste vara större än 0");

        if (data.TotalRooms < 1)
            return Results.BadRequest("Antal rum måste vara minst 1");

        string query = """
            INSERT INTO rooms (hotel_id, room_type, capacity, price_per_night, total_rooms)
            VALUES (@hotelId, @roomType, @capacity, @price, @total)
        """;

        var parameters = new MySqlParameter[]
        {
            new("@hotelId", data.HotelId),
            new("@roomType", data.RoomType),
            new("@capacity", data.Capacity),
            new("@price", data.PricePerNight),
            new("@total", data.TotalRooms)
        };

        try
        {
            await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
            return Results.Ok("Rumstyp skapad");
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return Results.BadRequest("Hotellet finns inte");
        }
    }

    // GET /hotels/{hotelId}/rooms - Hämta alla rumstyper för ett hotell
    public static async Task<List<RoomData>> GetRoomsByHotel(int hotelId, Config config)
    {
        List<RoomData> result = new();

        string query = """
            SELECT id, hotel_id, room_type, capacity, price_per_night, total_rooms
            FROM rooms
            WHERE hotel_id = @hotelId
            ORDER BY price_per_night ASC
        """;

        var parameters = new MySqlParameter[] { new("@hotelId", hotelId) };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetInt32(3),
                    reader.GetDecimal(4),
                    reader.GetInt32(5)
                ));
            }
        }

        return result;
    }

    // GET /rooms/{id} - Hämta en rumstyp
    public static async Task<RoomData?> GetRoom(int id, Config config)
    {
        string query = """
            SELECT id, hotel_id, room_type, capacity, price_per_night, total_rooms
            FROM rooms
            WHERE id = @id
        """;

        var parameters = new MySqlParameter[] { new("@id", id) };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                return new(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetInt32(3),
                    reader.GetDecimal(4),
                    reader.GetInt32(5)
                );
            }
        }

        return null;
    }

    // GET /hotels/{hotelId}/rooms/available?checkIn=X&checkOut=Y - Hämta lediga rum för datum
    public static async Task<List<RoomAvailability>> GetAvailableRooms(
        int hotelId, 
        DateOnly checkIn, 
        DateOnly checkOut, 
        Config config)
    {
        List<RoomAvailability> result = new();

        // Hämta rumstyper och beräkna hur många som är bokade för dessa datum
        string query = """
            SELECT 
                r.id,
                r.room_type,
                r.capacity,
                r.price_per_night,
                r.total_rooms - COALESCE(
                    (SELECT SUM(br.quantity)
                     FROM booking_rooms br
                     JOIN bookings b ON br.booking_id = b.id
                     WHERE br.room_id = r.id
                       AND b.status != 'cancelled'
                       AND b.check_in < @checkOut
                       AND b.check_out > @checkIn
                    ), 0
                ) AS available_rooms
            FROM rooms r
            WHERE r.hotel_id = @hotelId
            HAVING available_rooms > 0
            ORDER BY r.price_per_night ASC
        """;

        var parameters = new MySqlParameter[]
        {
            new("@hotelId", hotelId),
            new("@checkIn", checkIn.ToString("yyyy-MM-dd")),
            new("@checkOut", checkOut.ToString("yyyy-MM-dd"))
        };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            while (reader.Read())
            {
                result.Add(new(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    reader.GetDecimal(3),
                    reader.GetInt32(4)
                ));
            }
        }

        return result;
    }

    // DELETE /rooms/{id} - Ta bort rumstyp (admin only)
    public static async Task<IResult> DeleteRoom(int id, Config config, HttpContext ctx)
    {
        var authCheck = await Auth.RequireAdmin(config, ctx);
        if (authCheck is not null)
            return authCheck;

        string query = "DELETE FROM rooms WHERE id = @id";
        var parameters = new MySqlParameter[] { new("@id", id) };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok("Rumstyp borttagen");
    }

    // PUT /rooms/{id} - Uppdatera rumstyp (admin only)
    public static async Task<IResult> UpdateRoom(int id, RoomCreate data, Config config, HttpContext ctx)
    {
        var authCheck = await Auth.RequireAdmin(config, ctx);
        if (authCheck is not null)
            return authCheck;

        string query = """
            UPDATE rooms
            SET room_type = @roomType,
                capacity = @capacity,
                price_per_night = @price,
                total_rooms = @total
            WHERE id = @id
        """;

        var parameters = new MySqlParameter[]
        {
            new("@id", id),
            new("@roomType", data.RoomType),
            new("@capacity", data.Capacity),
            new("@price", data.PricePerNight),
            new("@total", data.TotalRooms)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok("Rumstyp uppdaterad");
    }
}
