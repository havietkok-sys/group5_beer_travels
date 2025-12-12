namespace server;

using MySql.Data.MySqlClient;

class Users
{
    // GET /users - H�mta alla anv�ndare (admin only)
    public record GetAll_Data(int Id, string? Name, string Email, string Role);
    public static async Task<IResult> GetAll(Config config, HttpContext ctx)
    {
        // Kolla att anv�ndaren �r admin
        string? role = await Auth.GetUserRole(config, ctx);
        if (role != "admin")
            return Results.Unauthorized();

        List<GetAll_Data> result = new();
        string query = "SELECT id, name, email, role FROM users";

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
        {
            while (reader.Read())
            {
                string? name = reader.IsDBNull(1) ? null : reader.GetString(1);
                result.Add(new(reader.GetInt32(0), name, reader.GetString(2), reader.GetString(3)));
            }
        }
        return Results.Ok(result);
    }

    // GET /users/{id} - H�mta en specifik anv�ndare
    public record Get_Data(string? Name, string Email, string Role);
    public static async Task<IResult> Get(int id, Config config, HttpContext ctx)
    {
        // Kolla att anv�ndaren �r inloggad
        int? userId = ctx.Session.GetInt32("user_id");
        if (userId is null)
            return Results.Unauthorized();

        // Anv�ndare kan bara se sig sj�lva, admin kan se alla
        string? role = await Auth.GetUserRole(config, ctx);
        if (role != "admin" && userId != id)
            return Results.Unauthorized();

        Get_Data? result = null;
        string query = "SELECT name, email, role FROM users WHERE id = @id";
        var parameters = new MySqlParameter[] { new("@id", id) };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                string? name = reader.IsDBNull(0) ? null : reader.GetString(0);
                result = new(name, reader.GetString(1), reader.GetString(2));
            }
        }

        if (result is null)
            return Results.NotFound();

        return Results.Ok(result);
    }

    // POST /users - Registrera ny anv�ndare
    public record Post_Args(string? Name, string Email, string Password);
    public static async Task<IResult> Post(Post_Args user, Config config)
    {
        // Validera input
        if (string.IsNullOrWhiteSpace(user.Email))
            return Results.BadRequest("Email kr�vs");

        if (string.IsNullOrWhiteSpace(user.Password))
            return Results.BadRequest("L�senord kr�vs");

        if (user.Password.Length < 6)
            return Results.BadRequest("L�senordet m�ste vara minst 6 tecken");

        string query = """
            INSERT INTO users(name, email, password, role)
            VALUES (@name, @email, @password, 'traveler')
        """;

        var parameters = new MySqlParameter[]
        {
            new("@name", user.Name ?? (object)DBNull.Value),
            new("@email", user.Email),
            new("@password", user.Password),
        };

        try
        {
            await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
            return Results.Ok("Anv�ndare skapad");
        }
        catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
        {
            return Results.BadRequest("E-postadressen �r redan registrerad");
        }
    }

    // DELETE /users/{id} - Ta bort anv�ndare (admin only)
    public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
    {
        // Kolla att anv�ndaren �r admin
        string? role = await Auth.GetUserRole(config, ctx);
        if (role != "admin")
            return Results.Unauthorized();

        string query = "DELETE FROM users WHERE id = @id";
        var parameters = new MySqlParameter[] { new("@id", id) };

        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
        return Results.Ok("Anv�ndare borttagen");
    }
}
