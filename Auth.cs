namespace server;

using MySql.Data.MySqlClient;

/// 
/// Hjälpklass för autentisering och auktorisering

public static class Auth
{
    /// Hämtar rollen för den inloggade användaren
    /// <returns>Rollen ('admin' eller 'traveler') eller null om ej inloggad</returns>
    public static async Task<string?> GetUserRole(Config config, HttpContext ctx)
    {
        int? userId = ctx.Session.GetInt32("user_id");
        if (userId is null)
            return null;

        string query = "SELECT role FROM users WHERE id = @id";
        var parameters = new MySqlParameter[] { new("@id", userId) };

        using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
        {
            if (reader.Read())
            {
                return reader.GetString(0);
            }
        }

        return null;
    }

    /// Hämtar ID för den inloggade användaren
    /// <returns>User ID eller null om ej inloggad</returns>
    public static int? GetUserId(HttpContext ctx)
    {
        return ctx.Session.GetInt32("user_id");
    }

    /// Kontrollerar om användaren är inloggad

    public static bool IsLoggedIn(HttpContext ctx)
    {
        return ctx.Session.GetInt32("user_id") is not null;
    }

    /// Kontrollerar om användaren är admin

    public static async Task<bool> IsAdmin(Config config, HttpContext ctx)
    {
        string? role = await GetUserRole(config, ctx);
        return role == "admin";
    }

    /// Returnerar Unauthorized om användaren inte är inloggad

    public static IResult? RequireLogin(HttpContext ctx)
    {
        if (!IsLoggedIn(ctx))
            return Results.Unauthorized();
        return null;
    }

    /// Returnerar Unauthorized om användaren inte är admin

    public static async Task<IResult?> RequireAdmin(Config config, HttpContext ctx)
    {
        if (!await IsAdmin(config, ctx))
            return Results.Unauthorized();
        return null;
    }
}
