namespace server;

public static class Database
{
    public static async Task db_reset_to_default(Config config)
    {
        string conn = config.ConnectionString;

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


        //  USERS - med name och role
        string users_table = """
            CREATE TABLE users (
                id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(255),
                email VARCHAR(254) NOT NULL UNIQUE,
                password VARCHAR(128) NOT NULL,
                role VARCHAR(20) NOT NULL DEFAULT 'traveler'
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, users_table);

        // Skapa en default admin-användare
        string create_admin = """
            INSERT INTO users (name, email, password, role)
            VALUES ('Admin', 'admin@beertravels.se', 'admin123', 'admin');
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, create_admin);


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
                distance_to_city_center_m INT NOT NULL,
                FOREIGN KEY (city_id) REFERENCES cities(id)
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, hotels_table);


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

        //  ROOMS (rumstyper per hotell)
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
                status VARCHAR(20) NOT NULL DEFAULT 'pending',
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id),
                FOREIGN KEY (hotel_id) REFERENCES hotels(id)
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, bookings_table);


        //  BOOKING_ROOMS (koppling bokning <-> rum)
        string booking_rooms_table = """
            CREATE TABLE booking_rooms (
                id INT PRIMARY KEY AUTO_INCREMENT,
                booking_id INT NOT NULL,
                room_id INT NOT NULL,
                quantity INT NOT NULL,
                FOREIGN KEY (booking_id) REFERENCES bookings(id) ON DELETE CASCADE,
                FOREIGN KEY (room_id) REFERENCES rooms(id)
            );
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, booking_rooms_table);
    }
}
