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
}
