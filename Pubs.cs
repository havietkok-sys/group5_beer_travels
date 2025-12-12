namespace server;

using MySql.Data.MySqlClient;

public static class Pubs
{

    //      RECORDS

    public record PubCreate(
        string CityName,
        string Name,
        string Address,
        int Distance,
        string Open,
        string Close
    );

    public record Pub_Data(int Id, string Name, string Address);

    public record PubBeer_Data(
        int Id,
        string Name,
        string Type,
        decimal PricePerLiter
    );



    // POST /pubs/create - Skapa pub (admin only)
    public static async Task<IResult> CreatePub(Config config, PubCreate data, HttpContext ctx)
    {
        // Kolla att användaren är admin
        var authCheck = await Auth.RequireAdmin(config, ctx);
        if (authCheck is not null)
            return authCheck;

        int? cityId = await GetCityId(config, data.CityName);
        if (cityId is null)
            return Results.BadRequest("Staden finns inte");



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



    //   GET PUBS IN CITY 


    public static async Task<List<Pub_Data>>
    GetPubs(string cityName, Config config)
    {
        int? cityId = await GetCityId(config, cityName);
        if (cityId is null)
            return new();

        List<Pub_Data> pubs = new();

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            "SELECT id, name, address FROM pubs WHERE city_id = @id",
            new MySqlParameter("@id", cityId)))
        {
            while (reader.Read())
            {
                pubs.Add(new Pub_Data(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2)
                ));
            }
        }

        return pubs;
    }


    // 
    //   GET BEERS FOR PUB
    // 

    public static async Task<List<PubBeer_Data>>
    GetBeersForPub(int pubId, Config config)
    {
        List<PubBeer_Data> beers = new();

        string query = """
SELECT 
    b.id,
    b.name,
    b.type,
    pb.price_per_liter
FROM pub_beers pb
JOIN beers b ON pb.beer_id = b.id
WHERE pb.pub_id = @pub
""";

        var parameters = new MySqlParameter[]
        {
            new("@pub", pubId)
        };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            query,
            parameters
        ))
        {
            while (reader.Read())
            {
                beers.Add(new PubBeer_Data(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDecimal(3)
                ));
            }
        }

        return beers;
    }



    //        HELPER (hjälpmetod att hämta ut id från stads namn)


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