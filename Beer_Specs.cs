namespace server;
using MySql.Data.MySqlClient;

 //  BEERS 
        string beers_table = """
            CREATE TABLE beers (
                id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(150) NOT NULL,
                name VARCHAR(50) NOT NULL
            );
            """;