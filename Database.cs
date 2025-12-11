namespace server;

public static class Database
{
    public static async Task db_reset_to_default(Config config)
    {
        string conn = config.ConnectionString;

        // Droppa dom i rätt ordning så det inte blockas av fq
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
    }
}
