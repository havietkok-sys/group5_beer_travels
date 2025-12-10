namespace server;
using MySql.Data.MySqlClient;

class beer_flavors
{
    public record Beer_Flavor_Data(int Id, string Name);

    public static async Task<List<Beer_Flavor_Data>> GetBeerFlavors(Config config)
    {
        List<Beer_Flavor_Data> flavors = new();

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            "SELECT id, name FROM flavors"))
        {
            while (reader.Read())
            {
                flavors.Add(new(
                    reader.GetInt32(0),
                    reader.GetString(1)
                ));
            }
        }

        return flavors;
    }
}