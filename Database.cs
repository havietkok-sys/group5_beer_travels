namespace server;


public static class Database
{
    public static async Task db_reset_to_default(Config config)
    {

        // Drop all tables from database
        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, "DROP TABLE IF EXISTS users");

        // Create all tables
        string users_table = """
            CREATE TABLE users
            (
            id INT PRIMARY KEY AUTO_INCREMENT,
            email varchar(256) NOT NULL UNIQUE,
            password TEXT
            )
            """;
        await MySqlHelper.ExecuteNonQueryAsync(config.ConnectionString, users_table);
    }
}