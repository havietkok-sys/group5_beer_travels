namespace server;

using MySql.Data.MySqlClient;

public static class DatabaseSeedEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/db/seed", SeedDatabase);
    }

    public static async Task<IResult> SeedDatabase(Config config)
    {
        string conn = config.ConnectionString;

        //
        // ======================================================
        // 1. CITIES
        // ======================================================
        //
        await MySqlHelper.ExecuteNonQueryAsync(conn, """
            INSERT INTO cities (name) VALUES
            ('Stockholm'),
            ('Göteborg'),
            ('Malmö'),
            ('Uppsala'),
            ('Västerås'),
            ('Örebro'),
            ('Linköping'),
            ('Helsingborg'),
            ('Jönköping'),
            ('Norrköping');
        """);

        //
        // ======================================================
        // 2. HOTELS  (OBS: hotell-tabellen har INGA öppettider)
        // ======================================================
        //
        await MySqlHelper.ExecuteNonQueryAsync(conn, """
            INSERT INTO hotels (city_id, name, address, distance_to_city_center_m) VALUES
            (1, 'BeerTravelHotel Stockholm', 'Vasagatan 1', 200),
            (2, 'BeerTravelHotel Göteborg', 'Avenyn 12', 300),
            (3, 'BeerTravelHotel Malmö', 'Gustav Adolfs torg 4', 250),
            (4, 'BeerTravelHotel Uppsala', 'Stora Torget 3', 150),
            (5, 'BeerTravelHotel Västerås', 'Hamnplan 7', 300),
            (6, 'BeerTravelHotel Örebro', 'Kungsgatan 20', 400),
            (7, 'BeerTravelHotel Linköping', 'Stora Torget 5', 200),
            (8, 'BeerTravelHotel Helsingborg', 'Kullagatan 1', 100),
            (9, 'BeerTravelHotel Jönköping', 'Östra Storgatan 28', 300),
            (10,'BeerTravelHotel Norrköping', 'Drottninggatan 9', 250);
        """);

        //                   Flavors
        await MySqlHelper.ExecuteNonQueryAsync(conn, """  
    INSERT INTO flavors (name) VALUES  
        ('Humle'),  
        ('Söt'),  
        ('Sur'),  
        ('Bitter'),  
        ('Maltig');  
""");




        //
        // ======================================================
        // 3. PUBS  (FK → city_id)
        // ======================================================
        //
        await MySqlHelper.ExecuteNonQueryAsync(conn, """
            INSERT INTO pubs (city_id, name, address, distance_to_hotel_m, open_time, close_time) VALUES
            -- Stockholm
            (1, 'Akkurat', 'Hornsgatan 18', 400, '11:00', '01:00'),
            (1, 'Oliver Twist', 'Repslagargatan 6', 450, '11:00', '01:00'),
            (1, 'The Flying Horse', 'Odengatan 44', 600, '11:00', '01:00'),

            -- Göteborg
            (2, 'The Rover', 'Andra Långgatan 12', 500, '11:00', '01:00'),
            (2, 'BrewDog Bar Göteborg', 'Kungsgatan 10', 450, '11:00', '01:00'),
            (2, 'Old Beefeater Inn', 'Vasagatan 43', 600, '11:00', '01:00'),

            -- Malmö
            (3, 'Mikkeller Bar Malmö', 'Norra Vallgatan 60', 450, '11:00', '01:00'),
            (3, 'Pickwick Pub', 'Baltzarsgatan 34', 500, '11:00', '01:00'),
            (3, 'Green Lion Inn', 'Norra Vallgatan 38', 350, '11:00', '01:00'),

            -- Uppsala
            (4, 'O’Connor’s', 'Stora Torget 1', 200, '11:00', '01:00'),
            (4, 'Taps Beer Bar', 'Sankt Olofsgatan 33', 350, '11:00', '01:00'),
            (4, 'Stationen', 'Olof Palmes Plats 6', 450, '11:00', '01:00'),

            -- Västerås
            (5, 'The Bishops Arms Västerås', 'Kungsgatan 5', 200, '11:00', '01:00'),
            (5, 'Pitcher’s Västerås', 'Stora Gatan 46', 250, '11:00', '01:00'),
            (5, 'P2 Pub', 'Stora gatan 32', 280, '11:00', '01:00'),

            -- Örebro
            (6, 'Sörby Pub', 'Sörbyvägen 12', 600, '11:00', '01:00'),
            (6, 'Rosengrens Skafferi Pub', 'Kungsgatan 2', 300, '11:00', '01:00'),
            (6, 'O’Learys Örebro', 'Stortorget 8', 400, '11:00', '01:00'),

            -- Linköping
            (7, 'Pitcher’s Linköping', 'Ågatan 39', 250, '11:00', '01:00'),
            (7, 'John Scott’s Linköping', 'Stora torget 3', 350, '11:00', '01:00'),
            (7, 'De Klomp', 'Sankt Larsgatan 13', 400, '11:00', '01:00'),

            -- Helsingborg
            (8, 'The Bishop Arms Helsingborg', 'Södra Storgatan 14', 230, '11:00', '01:00'),
            (8, 'Charles Dickens Pub', 'Bruksgatan 31', 300, '11:00', '01:00'),
            (8, 'The Pub', 'Nedre Långvinkelsgatan 2', 350, '11:00', '01:00'),

            -- Jönköping
            (9, 'Pipes of Scotland', 'Tändsticksgränd 12', 300, '11:00', '01:00'),
            (9, 'The Bishops Arms Jönköping', 'Västra Storgatan 12', 200, '11:00', '01:00'),
            (9, 'The Fox and Anchor', 'Barnarpsgatan 29', 350, '11:00', '01:00'),

            -- Norrköping
            (10,'Pappa Grappa Pub', 'Drottninggatan 26', 280, '11:00', '01:00'),
            (10,'The Black Lion Inn', 'Tyska Torget 3', 350, '11:00', '01:00'),
            (10,'Durkslaget Pub', 'Nygatan 23', 320, '11:00', '01:00');
        """);

        //
        // ======================================================
        // 4. BEERS
        // ======================================================
        //
        await MySqlHelper.ExecuteNonQueryAsync(conn, """
            INSERT INTO beers (name, type) VALUES
            ('Norrlands Guld', 'Lager'),
            ('Mariestads Export', 'Lager'),
            ('Pripps Blå', 'Lager'),
            ('Brooklyn Lager', 'Lager'),
            ('Guinness Draught', 'Stout'),
            ('Kilkenny', 'Red Ale'),
            ('Sierra Nevada Pale Ale', 'Pale Ale'),
            ('Lagunitas IPA', 'IPA'),
            ('BrewDog Punk IPA', 'IPA'),
            ('Pilsner Urquell', 'Pilsner'),
            ('Hoegaarden', 'Wheat'),
            ('Leffe Blonde', 'Belgian Ale'),
            ('Kronenbourg 1664 Blanc', 'Wheat'),
            ('Somersby Pear', 'Cider'),
            ('Strongbow', 'Cider'),
            ('Oppigårds Amarillo', 'IPA'),
            ('Stigbergets West Coast IPA', 'IPA'),
            ('S:t Eriks APA', 'APA'),
            ('Staropramen', 'Lager'),
            ('Carlsberg Hof', 'Lager');
        """);
        // SEED: Koppla öl till smaker
        await MySqlHelper.ExecuteNonQueryAsync(conn, """  
        INSERT INTO beer_flavors (beer_id, flavor_id) VALUES  
        (1, 1), (1, 5),   -- Brooklyn Lager: Humle, Maltig  
        (2, 1), (2, 4),   -- Staropramen: Humle, Bitter  
        (3, 5), (3, 2),   -- Guinness Draught: Maltig, Söt  
        (4, 3), (4, 2),   -- Hoegaarden: Sur, Söt  
        (5, 1), (5, 4), (5, 2);  -- Strongbow: Humle, Bitter, Söt  
    """);


        //
        // ======================================================
        // 5. PUB_BEERS  
        // (6 öl per pub, FK → pub_id,beer_id)
        // ======================================================
        //
        await MySqlHelper.ExecuteNonQueryAsync(conn, """
            INSERT INTO pub_beers (pub_id, beer_id, price_per_liter) VALUES
            (1,1,120),(1,4,140),(1,7,160),(1,8,170),(1,11,150),(1,17,185),
            (2,2,130),(2,3,135),(2,7,155),(2,9,165),(2,10,145),(2,18,176),
            (3,1,118),(3,4,138),(3,5,180),(3,6,150),(3,9,168),(3,12,160),

            (4,1,118),(4,7,150),(4,8,162),(4,11,148),(4,15,140),(4,17,182),
            (5,2,130),(5,3,135),(5,7,152),(5,8,162),(5,11,149),(5,17,177),
            (6,1,118),(6,4,138),(6,9,166),(6,10,145),(6,13,155),(6,19,178),

            (7,1,110),(7,4,130),(7,9,150),(7,13,155),(7,17,172),(7,20,165),
            (8,2,118),(8,3,125),(8,7,142),(8,8,155),(8,12,150),(8,18,170),
            (9,1,112),(9,4,132),(9,8,158),(9,10,148),(9,14,150),(9,17,168),

            (10,2,120),(10,3,128),(10,7,148),(10,9,160),(10,15,145),(10,19,175),
            (11,1,110),(11,4,130),(11,12,155),(11,13,140),(11,17,168),(11,20,170),
            (12,1,118),(12,4,138),(12,9,160),(12,10,150),(12,11,145),(12,16,172);
        """);

        return Results.Ok("Database seeded successfully!");
    }
}
