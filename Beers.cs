namespace server;


public class Beers
{
  public record BeerCreate(string Name, string Type);
  public record BeerData(int Id, string Name, string Type);

  public static Task<IResult> CreateBeer(Config config, BeerCreate data)
  {
    string query = "INSERT INTO beers (name, type) VALUES (@name, @type)";

    MySqlHelper.ExecuteNonQuery(
        config.ConnectionString,
        query,
        new("@name", data.Name),
        new("@type", data.Type)
    );

    return Task.FromResult(Results.Ok("Beer created") as IResult);
  }

  public static Task<IResult> GetAllBeers(Config config)
  {
    List<BeerData> beers = new();

    using var reader = MySqlHelper.ExecuteReader(
        config.ConnectionString,
        "SELECT id, name, type FROM beers"
    );

    while (reader.Read())
    {
      beers.Add(new BeerData(
          reader.GetInt32(0),
          reader.GetString(1),
          reader.GetString(2)
      ));
    }
    return Task.FromResult(Results.Ok(beers) as IResult);
  }
  public static Task<IResult> GetBeer(Config config, int id)
  {
    using var reader = MySqlHelper.ExecuteReader(
        config.ConnectionString,
        "SELECT id, name, type FROM beers WHERE id = @id",
        new MySqlParameter("@id", id)
    );
    if (reader.Read())
    {
      var beer = new BeerData(
          reader.GetInt32(0),
          reader.GetString(1),
          reader.GetString(2)
      );
      return Task.FromResult(Results.Ok(beer) as IResult);
    }
    return Task.FromResult(Results.NotFound("Beer not found") as IResult);
  }
  public static Task<IResult> DeleteBeer(Config config, int id)
  {
    MySqlHelper.ExecuteNonQuery(
        config.ConnectionString,
        "DELETE FROM beers WHERE id = @id",
        new MySqlParameter("@id", id)
    );
    return Task.FromResult(Results.Ok("Beer deleted") as IResult);
  }
}