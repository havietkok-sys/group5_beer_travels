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


        //  USERS 
        string users_table = """
            CREATE TABLE users (
                id INT PRIMARY KEY AUTO_INCREMENT,
                email VARCHAR(256) NOT NULL UNIQUE,
                password TEXT NOT NULL
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
        
        // Smak table för öl
        string flavors_table = """
            CREATE TABLE flavors (
                id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(50) NOT NULL UNIQUE
            );z
        """;
        await MySqlHelper.ExecuteNonQueryAsync(conn, flavors_table);  
    }
        

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
        string beers_table = """
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