namespace server;

using MySql.Data.MySqlClient;

public static class Hotels
{
    public record HotelCreate(string CityName, string Name, string Address);

    public static async Task<IResult>
    CreateHotel(Config config, HotelCreate data)
    {
        //
        // Hämta CityId — exakt som originalet men async
        //
        int? cityId = await GetCityId(config, data.CityName);
        if (cityId is null)
            return Results.BadRequest("City does not exist");

        //
        // Insert hotel
        //
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

        await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            query,
            parameters
        );

        return Results.Ok("Hotel created");
    }
    public record Hotel_Data(int Id, string Name);

    public static async Task<Hotel_Data?>
    GetHotel(string cityName, Config config)
    {
        int? cityId = await GetCityId(config, cityName);
        if (cityId is null)
            return null;

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            "SELECT id, name FROM hotels WHERE city_id = @id",
            new MySqlParameter("@id", cityId)))
        {
            if (reader.Read())
            {
                return new(
                    reader.GetInt32(0),
                    reader.GetString(1)
                );
            }
        }

        return null;
    }


    //
    //  Helper: GetCityId (async)
    //
    private static async Task<int?>
    GetCityId(Config config, string cityName)
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
}
