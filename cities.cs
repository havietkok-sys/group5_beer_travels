namespace server;

using MySql.Data.MySqlClient;

public static class Cities
{
    public record CityCreate(string Name);

    // POST /cities/create - Skapa stad (admin only)
    public static async Task<IResult> CreateCity(Config config, CityCreate data, HttpContext ctx)
    {
        // Kolla att användaren är admin
        var authCheck = await Auth.RequireAdmin(config, ctx);
        if (authCheck is not null)
            return authCheck;

        string query = "INSERT INTO cities (name) VALUES (@name)";

        var parameters = new MySqlParameter[]
        {
            new("@name", data.Name)
        };

        try
        {
            await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
            return Results.Ok("City created");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Results.BadRequest("Staden finns redan");
        }
    }

    // DELETE /cities/{id} - Ta bort stad (admin only)
    public static async Task<IResult> DeleteCity(Config config, int id, HttpContext ctx)
    {
        // Kolla att användaren är admin
        var authCheck = await Auth.RequireAdmin(config, ctx);
        if (authCheck is not null)
            return authCheck;

        string query = "DELETE FROM cities WHERE id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok("City deleted");
    }
}
