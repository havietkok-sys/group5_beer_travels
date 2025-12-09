namespace server;

using MySql.Data.MySqlClient;

public static class DatabaseSeedEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/db/seed", SeedDatabase);
    }


    public static Task<IResult> SeedDatabase(Config config)
    {
        string conn = config.ConnectionString;

        //
        //              Cities
        //
        MySqlHelper.ExecuteNonQuery(conn,
            "INSERT INTO cities (name) VALUES ('Halmstad'), ('Göteborg'), ('Stockholm');"
        );

        //
        //                  Hotels
        //
        MySqlHelper.ExecuteNonQuery(conn, """
            INSERT INTO hotels (city_id, name, address)
            VALUES
                (1, 'BeerHotel Halmstad', 'Storgatan 1'),
                (2, 'Göteborg BrewStay', 'Avenyn 21'),
                (3, 'Stockholm Craft Inn', 'Södergatan 9');
        """);

        //
        //                  Beers
        //
        MySqlHelper.ExecuteNonQuery(conn, """
            INSERT INTO beers (name, type)
            VALUES
                ('Carlsberg', 'Lager'),
                ('Guinness', 'Stout'),
                ('Brooklyn IPA', 'IPA'),
                ('Heineken', 'Lager'),
                ('Punk IPA', 'IPA'),
                ('Norrlands Guld', 'Lager');
        """);

        //
        //                       Pubs
        //
        MySqlHelper.ExecuteNonQuery(conn, """
            INSERT INTO pubs (city_id, name, address, distance_to_hotel_m, open_time, close_time)
            VALUES
                (1, 'Fox & Anchor', 'Hamngatan 4', 350, '14:00', '01:00'),
                (1, 'Captain Brew', 'Kustvägen 12', 500, '16:00', '02:00'),
                (2, 'Goth Alehouse', 'Avenyn 10', 150, '15:00', '01:00'),
                (3, 'Old Town Tavern', 'Gamla Stan 2', 220, '13:00', '02:00');
        """);

        //
        //                  Pub Beers (pub_id, beer_id, price)
        //
        MySqlHelper.ExecuteNonQuery(conn, """
            INSERT INTO pub_beers (pub_id, beer_id, price_per_liter)
            VALUES
                -- Fox & Anchor (pub 1)
                (1, 1, 89),
                (1, 3, 119),
                (1, 6, 79),

                -- Captain Brew (pub 2)
                (2, 2, 140),
                (2, 3, 125),
                (2, 5, 135),

                -- Goth Alehouse (pub 3)
                (3, 1, 95),
                (3, 4, 105),
                (3, 6, 85),

                -- Old Town Tavern (pub 4)
                (4, 2, 145),
                (4, 3, 130),
                (4, 5, 120);
        """);

        return Task.FromResult(Results.Ok("Database seeded with test data.") as IResult);
    }
}
