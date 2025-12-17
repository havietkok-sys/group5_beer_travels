namespace server;

using MySql.Data.MySqlClient;

class beer_flavors
{
    // Record för att lagra smakdata
    public record Beer_Flavor_Data(int Id, string Name);

    // Hämta alla smaker
    public static async Task<List<Beer_Flavor_Data>> GetAllFlavors(Config config)
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

    // Skapa ny smak
    public static async Task<Beer_Flavor_Data?> CreateFlavor(Config config, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            "INSERT INTO flavors (name) VALUES (@name)",
            new MySqlParameter("@name", name));

        var result = await MySqlHelper.ExecuteScalarAsync(
            config.ConnectionString,
            "SELECT LAST_INSERT_ID()");

        int newId = Convert.ToInt32(result);
        return new Beer_Flavor_Data(newId, name);
    }

    // Ta bort smak
    public static async Task<bool> DeleteFlavor(Config config, int id)
    {
        // Ta bort kopplingar först
        await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            "DELETE FROM beer_flavors WHERE flavor_id = @id",
            new MySqlParameter("@id", id));

        // Ta bort smaken
        var rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            "DELETE FROM flavors WHERE id = @id",
            new MySqlParameter("@id", id));

        return rowsAffected > 0;
    }

    // Lägg till smak till öl
    public static async Task<bool> AddflavorTobeer(Config config, int beerId, int flavorId)
    {
        try
        {
            var exists = await MySqlHelper.ExecuteScalarAsync(
                config.ConnectionString,
                "SELECT COUNT(*) FROM beer_flavors WHERE beer_id = @beerId AND flavor_id = @flavorId",
                new MySqlParameter("@beerId", beerId),
                new MySqlParameter("@flavorId", flavorId));

            if (Convert.ToInt32(exists) > 0)
                return false;

            await MySqlHelper.ExecuteNonQueryAsync(
                config.ConnectionString,
                "INSERT INTO beer_flavors (beer_id, flavor_id) VALUES (@beerId, @flavorId)",
                new MySqlParameter("@beerId", beerId),
                new MySqlParameter("@flavorId", flavorId));

            return true;
        }
        catch
        {
            return false;
        }
    }

    // Ta bort smak från öl
    public static async Task<bool> Removeflavorfrombeer(Config config, int beerId, int flavorId)
    {
        var rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            "DELETE FROM beer_flavors WHERE beer_id = @beerId AND flavor_id = @flavorId",
            new MySqlParameter("@beerId", beerId),
            new MySqlParameter("@flavorId", flavorId));

        return rowsAffected > 0;
    }

    // Hämta smaker för specifikt öl
    public static async Task<List<Beer_Flavor_Data>> GetFlavorsForBeer(Config config, int beerId)
    {
        List<Beer_Flavor_Data> flavors = new();

        using (var reader = await MySqlHelper.ExecuteReaderAsync(
            config.ConnectionString,
            @"SELECT f.id, f.name 
              FROM flavors f
              INNER JOIN beer_flavors bf ON f.id = bf.flavor_id
              WHERE bf.beer_id = @beerId",
            new MySqlParameter("@beerId", beerId)))
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