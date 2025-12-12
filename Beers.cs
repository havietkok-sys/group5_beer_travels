namespace server;

public class Beers
{
  public record BeerData(int Id, string Name, string Type);
  public record BeerCreate(string Name, string Type);

  public static async Task<List<BeerData>> GetAll(Config config)
  {
    List<BeerData> result = new();
    string query = "SELECT id, name, type FROM beers";

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query))
    {
      while (reader.Read())
      {
        result.Add(new(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2)
        ));
      }
    }

    return result;
  }

  public static async Task<BeerData?> Get(int id, Config config)
  {
    BeerData? result = null;
    string query = "SELECT id, name, type FROM beers WHERE id = @id";
    var parameters = new MySqlParameter[] { new("@id", id) };

    using (var reader = await MySqlHelper.ExecuteReaderAsync(config.ConnectionString, query, parameters))
    {
      if (reader.Read())
      {
        result = new(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2)
        );
      }
    }

    return result;
  }

  // POST /beers/create - Skapa �l (admin only)
  public static async Task<IResult> Post(BeerCreate beer, Config config, HttpContext ctx)
  {
    // Kolla att anv�ndaren �r admin
    var authCheck = await Auth.RequireAdmin(config, ctx);
    if (authCheck is not null)
      return authCheck;

    if (string.IsNullOrWhiteSpace(beer.Name))
      return Results.BadRequest("Namn kr�vs");

    if (string.IsNullOrWhiteSpace(beer.Type))
      return Results.BadRequest("Typ kr�vs");

    string query = "INSERT INTO beers (name, type) VALUES (@name, @type)";
    var parameters = new MySqlParameter[]
    {
            new("@name", beer.Name),
            new("@type", beer.Type)
    };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
    return Results.Ok("�l skapad");
  }

  // DELETE /beers/{id} - Ta bort �l (admin only)
  public static async Task<IResult> Delete(int id, Config config, HttpContext ctx)
  {
    // Kolla att anv�ndaren �r admin
    var authCheck = await Auth.RequireAdmin(config, ctx);
    if (authCheck is not null)
      return authCheck;

    string query = "DELETE FROM beers WHERE id = @id";
    var parameters = new MySqlParameter[] { new("@id", id) };

    await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, query, parameters);
    return Results.Ok("�l borttagen");
  }
}