namespace server;

using MySql.Data.MySqlClient;

public static class Cities
{
    public record CityCreate(string Name);

    public static async Task<IResult>
    CreateCity(Config config, CityCreate data)
    {
        string query = "INSERT INTO cities (name) VALUES (@name)";

        var parameters = new MySqlParameter[]
        {
            new("@name", data.Name)
        };

        await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            query,
            parameters
        );

        return Results.Ok("City created");
    }


    public static async Task<IResult>
    DeleteCity(Config config, int id)
    {
        string query = "DELETE FROM cities WHERE id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        await MySqlHelper.ExecuteNonQueryAsync(
            config.ConnectionString,
            query,
            parameters
        );

        return Results.Ok("City deleted");
    }
}

