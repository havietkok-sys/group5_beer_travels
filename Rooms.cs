namespace server;

using MySql.Data.MySqlClient;

public static class Rooms
{
    // GET /hotels/{hotelId}/rooms - Hämta alla rumstyper för ett hotell
    public record RoomData(int Id, string RoomType, int Capacity, decimal PricePerNight, int TotalRooms);
    
    public static async Task<IResult> GetRoomsForHotel(int hotelId, Config config)
    {
        List<RoomData> result = new();
        string query = """
            SELECT id, room_type, capacity, price_per_night, total_rooms 
            FROM rooms WHERE hotel_id = @hotelId
        """;

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString, query, new MySqlParameter("@hotelId", hotelId)))
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
        return Results.Ok(result);
    }

    // GET /rooms/{roomId}/availability?checkIn=2025-06-15&checkOut=2025-06-20
    public record AvailabilityData(int RoomId, string RoomType, int TotalRooms, int BookedRooms, int Available);
    
    public static async Task<IResult> GetAvailability(int roomId, string checkIn, string checkOut, Config config)
    {
        // Hämta ruminfo och räkna bokade rum för perioden
        string query = """
            SELECT r.id, r.room_type, r.total_rooms,
                COALESCE(SUM(br.quantity), 0) AS booked
            FROM rooms r
            LEFT JOIN booking_rooms br ON r.id = br.room_id
            LEFT JOIN bookings b ON br.booking_id = b.id
                AND b.status != 'cancelled'
                AND b.check_in < @checkOut
                AND b.check_out > @checkIn
            WHERE r.id = @roomId
            GROUP BY r.id
        """;

        var parameters = new MySqlParameter[]
        {
            new("@roomId", roomId),
            new("@checkIn", checkIn),
            new("@checkOut", checkOut)
        };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                int total = reader.GetInt32(2);
                int booked = reader.GetInt32(3);
                return Results.Ok(new AvailabilityData(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    total,
                    booked,
                    total - booked
                ));
            }
        }
        return Results.NotFound("Rummet finns inte");
    }

    // POST /hotels/{hotelId}/rooms - Skapa rumstyp (admin)
    public record CreateRoomData(string RoomType, int Capacity, decimal PricePerNight, int TotalRooms);
    
    public static async Task<IResult> CreateRoom(int hotelId, CreateRoomData data, Config config, HttpContext ctx)
    {
        var authCheck = await Auth.RequireAdmin(config, ctx);
        if (authCheck is not null) return authCheck;

        string query = """
            INSERT INTO rooms (hotel_id, room_type, capacity, price_per_night, total_rooms)
            VALUES (@hotelId, @type, @capacity, @price, @total)
        """;

        var parameters = new MySqlParameter[]
        {
            new("@hotelId", hotelId),
            new("@type", data.RoomType),
            new("@capacity", data.Capacity),
            new("@price", data.PricePerNight),
            new("@total", data.TotalRooms)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok("Rumstyp skapad");
    }
}
