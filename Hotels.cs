namespace server;

using MySql.Data.MySqlClient;

public static class Hotels
{
    public record HotelCreate(string CityName, string Name, string Address);

    // POST /hotels/create - Skapa hotell (admin only)
    public static async Task<IResult> CreateHotel(Config config, HotelCreate data, HttpContext ctx)
    {
        // Kolla att användaren är admin
        var authCheck = await Auth.RequireAdmin(config, ctx);
        if (authCheck is not null)
            return authCheck;

        // Hämta CityId
        int? cityId = await GetCityId(config, data.CityName);
        if (cityId is null)
            return Results.BadRequest("Staden finns inte");

        string query = """
            INSERT INTO hotels (city_id, name, address)
            VALUES (@city, @name, @addr)
        """;

        var parameters = new MySqlParameter[]
        {
            new("@city", cityId),
            new("@name", data.Name),
            new("@addr", data.Address)
        };

        try
        {
            await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
            return Results.Ok("Hotell skapat");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Results.BadRequest("Det finns redan ett hotell i denna stad");
        }
    }

    // GET /cities/{city}/hotel - Hämta hotell i en stad
    public record Hotel_Data(int Id, string Name, string? Address);
    public static async Task<Hotel_Data?> GetHotel(string city, Config config)
    {
        int? cityId = await GetCityId(config, city);
        if (cityId is null)
            return null;

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            "SELECT id, name, address FROM hotels WHERE city_id = @id",
            new MySqlParameter("@id", cityId)))
        {
            if (reader.Read())
            {
                string? address = reader.IsDBNull(2) ? null : reader.GetString(2);
                return new(reader.GetInt32(0), reader.GetString(1), address);
            }
        }

        return null;
    }


    // Helper: GetCityId
    private static async Task<int?> GetCityId(Config config, string cityName)
    {
        string query = "SELECT id FROM cities WHERE name = @name";
        var parameters = new MySqlParameter[] { new("@name", cityName) };

        int? id = null;

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString, query, parameters))
        {
            if (reader.Read())
                id = reader.GetInt32(0);
        }

        return id;
    }

