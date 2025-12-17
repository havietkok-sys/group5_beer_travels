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
        // 0. USERS (admin + testanvändare)
        // ======================================================
        //
        await MySqlHelper.ExecuteNonQueryAsync(conn, """
            INSERT INTO users (email, password, name, role) VALUES
            ('admin@beertravels.se', 'admin123', 'Admin User', 'admin'),
            ('user@beertravels.se', 'user123', 'Test User', 'traveler'),
            ('test@test.se', 'test123', 'Test Person', 'traveler');
        """);

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

        //
        // ======================================================
        // 2.5 ROOMS (rumstyper för varje hotell)
        // ======================================================
        //
        await MySqlHelper.ExecuteNonQueryAsync(conn, """
            INSERT INTO rooms (hotel_id, room_type, capacity, price_per_night, total_rooms) VALUES
            -- Stockholm (hotel_id = 1)
            (1, 'Single', 1, 890.00, 10),
            (1, 'Double', 2, 1290.00, 15),
            (1, 'Suite', 4, 2490.00, 5),
            
            -- Göteborg (hotel_id = 2)
            (2, 'Single', 1, 790.00, 8),
            (2, 'Double', 2, 1190.00, 12),
            (2, 'Suite', 4, 2290.00, 4),
            
            -- Malmö (hotel_id = 3)
            (3, 'Single', 1, 750.00, 10),
            (3, 'Double', 2, 1090.00, 15),
            (3, 'Suite', 4, 1990.00, 3),
            
            -- Uppsala (hotel_id = 4)
            (4, 'Single', 1, 690.00, 6),
            (4, 'Double', 2, 990.00, 10),
            (4, 'Family', 5, 1590.00, 4),
            
            -- Västerås (hotel_id = 5)
            (5, 'Single', 1, 650.00, 8),
            (5, 'Double', 2, 950.00, 12),
            
            -- Örebro (hotel_id = 6)
            (6, 'Single', 1, 650.00, 6),
            (6, 'Double', 2, 950.00, 10),
            (6, 'Suite', 4, 1790.00, 2),
            
            -- Linköping (hotel_id = 7)
            (7, 'Single', 1, 690.00, 8),
            (7, 'Double', 2, 1050.00, 10),
            
            -- Helsingborg (hotel_id = 8)
            (8, 'Single', 1, 720.00, 6),
            (8, 'Double', 2, 1090.00, 8),
            (8, 'Suite', 4, 1890.00, 3),
            
            -- Jönköping (hotel_id = 9)
            (9, 'Single', 1, 650.00, 8),
            (9, 'Double', 2, 990.00, 10),
            
            -- Norrköping (hotel_id = 10)
            (10, 'Single', 1, 620.00, 6),
            (10, 'Double', 2, 920.00, 8),
            (10, 'Family', 5, 1490.00, 3);
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
            (4, 'O''Connors', 'Stora Torget 1', 200, '11:00', '01:00'),
            (4, 'Taps Beer Bar', 'Sankt Olofsgatan 33', 350, '11:00', '01:00'),
            (4, 'Stationen', 'Olof Palmes Plats 6', 450, '11:00', '01:00'),

            -- Västerås
            (5, 'The Bishops Arms Västerås', 'Kungsgatan 5', 200, '11:00', '01:00'),
            (5, 'Pitcher''s Västerås', 'Stora Gatan 46', 250, '11:00', '01:00'),
            (5, 'P2 Pub', 'Stora gatan 32', 280, '11:00', '01:00'),

            -- Örebro
            (6, 'Sörby Pub', 'Sörbyvägen 12', 600, '11:00', '01:00'),
            (6, 'Rosengrens Skafferi Pub', 'Kungsgatan 2', 300, '11:00', '01:00'),
            (6, 'O''Learys Örebro', 'Stortorget 8', 400, '11:00', '01:00'),

            -- Linköping
            (7, 'Pitcher''s Linköping', 'Ågatan 39', 250, '11:00', '01:00'),
            (7, 'John Scott''s Linköping', 'Stora torget 3', 350, '11:00', '01:00'),
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
