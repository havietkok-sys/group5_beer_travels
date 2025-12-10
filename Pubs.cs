namespace server;

public static class Pubs
{
    public record PubCreate(
        string CityName,
        string Name,
        string Address,
        int Distance,
        string Open,
        string Close
    );

    public static async Task<IResult>
    CreatePub(Config config, PubCreate data)
    {
        int? cityId = await GetCityId(config, data.CityName);
        if (cityId is null)
            return Results.BadRequest("City does not exist");

        string query = """
            INSERT INTO pubs
            (city_id, name, address, distance_to_hotel_m, open_time, close_time)
            VALUES (@city, @name, @addr, @dist, @open, @close)
        """;

        var parameters = new MySqlParameter[]
        {
            new("@city", cityId),
            new("@name", data.Name),
            new("@addr", data.Address),
            new("@dist", data.Distance),
            new("@open", data.Open),
            new("@close", data.Close)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);

        return Results.Ok("Pub created");
    }

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
