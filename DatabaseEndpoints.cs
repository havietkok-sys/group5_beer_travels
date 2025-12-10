namespace server;

using MySql.Data.MySqlClient;

public static class DatabaseEndpoints
{
    public static void Map(WebApplication app)
    {
        // CITY CRUD
        app.MapPost("/cities/create", CreateCity);
        app.MapDelete("/cities/{id}", DeleteCity);

        // HOTEL CRUD
        app.MapPost("/hotels/create", CreateHotel);

        // PUB CRUD
        app.MapPost("/pubs/create", CreatePub);


        // PUB-BEERS CRUD
        app.MapPost("/pubs/{pubId}/addbeer", AddBeerToPub);
        app.MapDelete("/pubs/{pubId}/beers/{beerId}", RemoveBeerFromPub);
    }


    // 
    //                      CITY CRUD
    // 

    public static Task<IResult> CreateCity(Config config, string name)
    {
        string query = "INSERT INTO cities (name) VALUES (@name)";

        MySqlHelper.ExecuteNonQuery(
            config.ConnectionString,
            query,
            new MySqlParameter("@name", name)
        );

        return Task.FromResult(Results.Ok("City created") as IResult);
    }

    public static Task<IResult> DeleteCity(Config config, int id)
    {
        string query = "DELETE FROM cities WHERE id = @id";

        MySqlHelper.ExecuteNonQuery(
            config.ConnectionString,
            query,
            new MySqlParameter("@id", id)
        );

        return Task.FromResult(Results.Ok("City deleted") as IResult);
    }



    // 
    //                      HOTEL CRUD
    // 

    public record HotelCreate(string CityName, string Name, string Address);

    public static Task<IResult> CreateHotel(Config config, HotelCreate data)
    {
        int? cityId = GetCityId(data.CityName, config);
        if (cityId is null)
            return Task.FromResult(Results.BadRequest("City does not exist") as IResult);

        string query = """
            INSERT INTO hotels (city_id, name, address)
            VALUES (@city, @name, @addr)
        """;

        MySqlHelper.ExecuteNonQuery(
            config.ConnectionString,
            query,
            new("@city", cityId),
            new("@name", data.Name),
            new("@addr", data.Address)
        );

        return Task.FromResult(Results.Ok("Hotel created") as IResult);
    }



    // 
    //                       PUB CRUD
    // 

    public record PubCreate(
        string CityName,
        string Name,
        string Address,
        int Distance,
        string Open,
        string Close
    );

    public static Task<IResult> CreatePub(Config config, PubCreate data)
    {
        int? cityId = GetCityId(data.CityName, config);
        if (cityId is null)
            return Task.FromResult(Results.BadRequest("City does not exist") as IResult);

        string query = """
            INSERT INTO pubs
            (city_id, name, address, distance_to_hotel_m, open_time, close_time)
            VALUES (@city, @name, @addr, @dist, @open, @close)
        """;

        MySqlHelper.ExecuteNonQuery(
            config.ConnectionString,
            query,
            new("@city", cityId),
            new("@name", data.Name),
            new("@addr", data.Address),
            new("@dist", data.Distance),
            new("@open", data.Open),
            new("@close", data.Close)
        );

        return Task.FromResult(Results.Ok("Pub created") as IResult);
    }





    // 
    //                  PUB-BEER RELATION CRUD
    // 

    public record PubBeerCreate(string BeerName, int Price);

    public static Task<IResult> AddBeerToPub(Config config, int pubId, PubBeerCreate data)
    {
        //               FIND BEER              
        string queryBeer = "SELECT id FROM beers WHERE name = @name";
        int? beerId = null;

        using (var reader = MySqlHelper.ExecuteReader(
            config.ConnectionString,
            queryBeer,
            new MySqlParameter("@name", data.BeerName)))
        {
            if (reader.Read())
                beerId = reader.GetInt32(0);
        }

        if (beerId is null)
            return Task.FromResult(Results.BadRequest("Beer does not exist") as IResult);

        //               INSERT RELATION                
        string insert = """
            INSERT INTO pub_beers (pub_id, beer_id, price_per_liter)
            VALUES (@pub, @beer, @price)
        """;

        MySqlHelper.ExecuteNonQuery(
            config.ConnectionString,
            insert,
            new("@pub", pubId),
            new("@beer", beerId),
            new("@price", data.Price)
        );

        return Task.FromResult(Results.Ok("Beer linked to pub") as IResult);
    }


    public static Task<IResult> RemoveBeerFromPub(Config config, int pubId, int beerId)
    {
        string query = """
            DELETE FROM pub_beers
            WHERE pub_id = @pub AND beer_id = @beer
        """;

        MySqlHelper.ExecuteNonQuery(
            config.ConnectionString,
            query,
            new("@pub", pubId),
            new("@beer", beerId)
        );

        return Task.FromResult(Results.Ok("Beer removed from pub") as IResult);
    }



    // 
    //                         HELPERS
    // 

    private static int? GetCityId(string cityName, Config config)
    {
        string query = "SELECT id FROM cities WHERE name = @name";
        int? id = null;

        using (var reader = MySqlHelper.ExecuteReader(
            config.ConnectionString,
            query,
            new MySqlParameter("@name", cityName)))
        {
            if (reader.Read())
                id = reader.GetInt32(0);
        }

        return id;
    }
}
