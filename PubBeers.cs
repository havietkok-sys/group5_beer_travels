namespace server;

using MySql.Data.MySqlClient;

public static class PubBeers
{
    public record PubBeerCreate(string BeerName, int Price);

    public static async Task<IResult>
    AddBeerToPub(Config config, int pubId, PubBeerCreate data)
    {
        //
        //  HÄMTA BEER-ID (SAME AS ORIGINAL, FAST ASYNC)
        //
        string queryBeer = "SELECT id FROM beers WHERE name = @name";
        int? beerId = null;

        var beerParams = new MySqlParameter[]
        {
            new("@name", data.BeerName)
        };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            queryBeer,
            beerParams
        ))
        {
            if (reader.Read())
                beerId = reader.GetInt32(0);
        }

        if (beerId is null)
            return Results.BadRequest("Beer does not exist");

        //
        //  SKAPA KOPPLING PUB <-> BEER
        //
        string insert = """
            INSERT INTO pub_beers (pub_id, beer_id, price_per_liter)
            VALUES (@pub, @beer, @price)
        """;

        var insertParams = new MySqlParameter[]
        {
            new("@pub", pubId),
            new("@beer", beerId),
            new("@price", data.Price)
        };

        await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            insert,
            insertParams
        );

        return Results.Ok("Beer linked to pub");
    }


    public static async Task<IResult>
    RemoveBeerFromPub(Config config, int pubId, int beerId)
    {
        string query = """
            DELETE FROM pub_beers
            WHERE pub_id = @pub AND beer_id = @beer
        """;

        var parameters = new MySqlParameter[]
        {
            new("@pub", pubId),
            new("@beer", beerId)
        };

        await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            query,
            parameters
        );

        return Results.Ok("Beer removed from pub");
    }
}
