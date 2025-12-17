namespace server;

using MySql.Data.MySqlClient;

static class Login
{
    // GET /login - Returnerar info om inloggad anv�ndare (inkl. roll)
    public record Get_Data(string? Name, string Email, string Role);
    public static async Task<Get_Data?> Get(Config config, HttpContext ctx)
    {
        Get_Data? result = null;

        if (ctx.Session.IsAvailable)
        {
            if (ctx.Session.Keys.Contains("user_id"))
            {
                string query = "SELECT name, email, role FROM users WHERE id = @id";
                var parameters = new MySqlParameter[]
                {
                    new("@id", ctx.Session.GetInt32("user_id"))
                };

                using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
                {
                    if (reader.Read())
                    {
                        string? name = reader.IsDBNull(0) ? null : reader.GetString(0);
                        result = new(name, reader.GetString(1), reader.GetString(2));
                    }
                }
            }
        }
        return result;
    }

    // POST /login - Logga in
    public record Post_Args(string Email, string Password);
    public static async Task<bool> Post(Post_Args credentials, Config config, HttpContext ctx)
    {
        bool result = false;
        string query = "SELECT id FROM users WHERE email = @email AND password = @password";
        var parameters = new MySqlParameter[]
        {
            new("@email", credentials.Email),
            new("@password", credentials.Password),
        };

        object query_result = await MySqlHelper.ExecuteScalarAsync(config.ConnectionString, query, parameters);

        if (query_result is int id)
        {
            ctx.Session.SetInt32("user_id", id);
            result = true;
        }

        return result;
    }

    // DELETE /login - Logga ut
    public static void Delete(HttpContext ctx)
    {
        ctx.Session.Clear();
    }
}
