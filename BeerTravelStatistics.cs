namespace server;

using MySql.Data.MySqlClient;

public static class BeerTravelStatistics
{
    //  DATA MODELL FÖR RETURN

    public record CityAveragePrice(
        string City,
        int PubCount,
        int BeerPricesCollected,
        double AveragePricePerLiter
    );

    // DATA MODELL FÖR BILLIGASTE ÖLEN
    public record CheapestBeerData(
        string City,
        string PubName,
        string BeerName,
        string BeerType,
        int PricePerLiter
    );

    //  HÄMTA GENOMSNITTLIGT LITERPRIS PER STAD

    public static async Task<IResult>
    GetAverageBeerPricePerCity(Config config)
    {
        List<CityAveragePrice> result = new();

        string query = """
            SELECT 
                c.name AS city,
                COUNT(DISTINCT p.id) AS pubCount,
                COUNT(pb.price_per_liter) AS beerPricesCollected,
                AVG(pb.price_per_liter) AS avgPrice
            FROM cities c
            JOIN hotels h ON h.city_id = c.id
            JOIN pubs p ON p.city_id = c.id
            JOIN pub_beers pb ON pb.pub_id = p.id
            GROUP BY c.id, c.name
            ORDER BY avgPrice ASC;
        """;

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString, query))
        {
            while (reader.Read())
            {
                result.Add(new CityAveragePrice(
                    reader.GetString("city"),
                    reader.GetInt32("pubCount"),
                    reader.GetInt32("beerPricesCollected"),
                    reader.GetDouble("avgPrice")
                ));
            }
        }

        return Results.Ok(result);
    }

    // HÄMTAR DOM 10 BILLIGASTE ÖLEN
    public static async Task<List<CheapestBeerData>> GetTop10Cheapest(Config config)
    {
        List<CheapestBeerData> result = new();

        string query = """
            SELECT 
                c.name AS city,
                p.name AS pub_name,
                b.name AS beer_name,
                b.type AS beer_type,
                pb.price_per_liter
            FROM pub_beers pb
            JOIN pubs p ON pb.pub_id = p.id
            JOIN cities c ON p.city_id = c.id
            JOIN beers b ON pb.beer_id = b.id
            ORDER BY pb.price_per_liter ASC
            LIMIT 10;
            """;

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            query))
        {
            while (reader.Read())
            {
                result.Add(new CheapestBeerData(
                    reader.GetString("city"),
                    reader.GetString("pub_name"),
                    reader.GetString("beer_name"),
                    reader.GetString("beer_type"),
                    reader.GetInt32("price_per_liter")
                ));
            }
        }

        return result;
    }
}
