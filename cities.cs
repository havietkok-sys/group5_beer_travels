namespace server;

class Cities
{
    // ===== PUBS =====
    public record Pub_Data(int Id, string Name, string Address);

    public static async Task<List<Pub_Data>> GetPubs(string cityName, Config config)
    {
        // 1. hitta city_id
        int? cityId = await GetCityId(cityName, config);
        if (cityId is null)
            return new();

        // 2. hämta pubbar
        List<Pub_Data> pubs = new();

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            "SELECT id, name, address FROM pubs WHERE city_id = @id",
            new MySqlParameter("@id", cityId)))
        {
            while (reader.Read())
            {
                pubs.Add(new(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2)
                ));
            }
        }

        return pubs;
    }



    // ===== HOTEL =====
    public record Hotel_Data(int Id, string Name);

    public static async Task<Hotel_Data?> GetHotel(string cityName, Config config)
    {
        int? cityId = await GetCityId(cityName, config);
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



    // ===== HELPER =====
    private static async Task<int?> GetCityId(string cityName, Config config)
    {
        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            "SELECT id FROM cities WHERE name = @name",
            new MySqlParameter("@name", cityName)))
        {
            if (reader.Read())
                return reader.GetInt32(0);
        }

        return null;
    }
}
