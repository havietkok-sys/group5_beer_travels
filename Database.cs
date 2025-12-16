namespace server;

public static class Database
{
    public static async Task<IResult> db_reset_to_default(Config config)
    {
        string conn = config.ConnectionString;


        await MySqlHelper.ExecuteNonQueryAsync(conn, "SET FOREIGN_KEY_CHECKS = 0;");


        // Droppa dom i rätt ordning så det inte blockas av fq
        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS booking_rooms");
        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS bookings");
        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS rooms");
        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS pub_beers");
        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS beers");
        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS pubs");
        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS hotels");
        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS cities");
        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS users");


        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS beer_flavors");
        await MySqlHelper.ExecuteNonQueryAsync(conn, "DROP TABLE IF EXISTS flavors");


        await MySqlHelper.ExecuteNonQueryAsync(conn, "SET FOREIGN_KEY_CHECKS = 1;");


        //  USERS (med role-kolumn för admin/traveler)
        string users_table = """
            CREATE TABLE users (
                id INT PRIMARY KEY AUTO_INCREMENT,
                email VARCHAR(256) NOT NULL UNIQUE,
                password TEXT NOT NULL,
                name VARCHAR(100),
                role VARCHAR(20) NOT NULL DEFAULT 'traveler'
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, users_table);


        //  CITIES 
        string cities_table = """
            CREATE TABLE cities (
                id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(100) NOT NULL UNIQUE
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, cities_table);


        //  HOTELS (1 per stad via UNIQUE city_id) 
        string hotels_table = """
            CREATE TABLE hotels (
                id INT PRIMARY KEY AUTO_INCREMENT,
                city_id INT NOT NULL UNIQUE,
                name VARCHAR(150) NOT NULL,
                address VARCHAR(200),
                distance_to_city_center_m INT NOT NULL DEFAULT 0,
                FOREIGN KEY (city_id) REFERENCES cities(id)
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, hotels_table);


        //  ROOMS (rumstyper för hotell)
        string rooms_table = """
            CREATE TABLE rooms (
                id INT PRIMARY KEY AUTO_INCREMENT,
                hotel_id INT NOT NULL,
                room_type VARCHAR(50) NOT NULL,
                capacity INT NOT NULL,
                price_per_night DECIMAL(10,2) NOT NULL,
                total_rooms INT NOT NULL,
                FOREIGN KEY (hotel_id) REFERENCES hotels(id)
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, rooms_table);


        //  BOOKINGS (bokningar)
        string bookings_table = """
            CREATE TABLE bookings (
                id INT PRIMARY KEY AUTO_INCREMENT,
                user_id INT NOT NULL,
                hotel_id INT NOT NULL,
                check_in DATE NOT NULL,
                check_out DATE NOT NULL,
                guests INT NOT NULL,
                total_price DECIMAL(10,2) NOT NULL,
                status VARCHAR(20) NOT NULL DEFAULT 'confirmed',
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id),
                FOREIGN KEY (hotel_id) REFERENCES hotels(id)
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, bookings_table);


        //  BOOKING_ROOMS (vilka rum som ingår i en bokning)
        string booking_rooms_table = """
            CREATE TABLE booking_rooms (
                id INT PRIMARY KEY AUTO_INCREMENT,
                booking_id INT NOT NULL,
                room_id INT NOT NULL,
                quantity INT NOT NULL,
                FOREIGN KEY (booking_id) REFERENCES bookings(id),
                FOREIGN KEY (room_id) REFERENCES rooms(id)
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, booking_rooms_table);


        //  PUBS 
        string pubs_table = """
            CREATE TABLE pubs (
                id INT PRIMARY KEY AUTO_INCREMENT,
                city_id INT NOT NULL,
                name VARCHAR(150) NOT NULL,
                address VARCHAR(200),
                distance_to_hotel_m INT NOT NULL,
                open_time TIME NOT NULL,
                close_time TIME NOT NULL,
                FOREIGN KEY (city_id) REFERENCES cities(id)
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, pubs_table);


        //  BEERS 
        string beers_table = """
            CREATE TABLE beers (
                id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(150) NOT NULL,
                type VARCHAR(50) NOT NULL
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, beers_table);

        // Smak table för öl
        string flavors_table = """
            CREATE TABLE flavors (
                id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(50) NOT NULL UNIQUE
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, flavors_table);



        // Kopplingstabell mellan öl och smaker (Many-to-Many)zzz
        string beer_flavors = """ 
            CREATE TABLE beer_flavors (
                beer_id INT NOT NULL,
                flavor_id INT NOT NULL,
                PRIMARY KEY (beer_id, flavor_id),
                FOREIGN KEY (beer_id) REFERENCES beers(id),
                FOREIGN KEY (flavor_id) REFERENCES flavors(id)
            );
        """;


        await MySqlHelper.ExecuteNonQueryAsync(conn, beer_flavors);



        //  PUB_BEERS (unik öl per pub, med pris) 
        string pub_beers_table = """
            CREATE TABLE pub_beers (
                pub_id INT NOT NULL,
                beer_id INT NOT NULL,
                price_per_liter SMALLINT NOT NULL,
                PRIMARY KEY (pub_id, beer_id),
                FOREIGN KEY (pub_id) REFERENCES pubs(id),
                FOREIGN KEY (beer_id) REFERENCES beers(id)
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, pub_beers_table);
        return Results.Ok("Database reset completed");
    }
}
