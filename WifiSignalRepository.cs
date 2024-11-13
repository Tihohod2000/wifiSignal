using Npgsql;

public class WifiSignalRepository
{
    private readonly DbConfig _config;

    public WifiSignalRepository(DbConfig config)
    {
        _config = config;
    }

    public void EnsureTableExists()
    {
        using (var connection = new NpgsqlConnection(_config.ConnectionString))
        {
            connection.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS ""wifiSignal_table"" (
                    ""ID"" SERIAL PRIMARY KEY,
                    ""SSID"" TEXT NOT NULL UNIQUE,
                    ""Signal"" INTEGER NOT NULL
                )";

            using (var command = new NpgsqlCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    public void InsertWifiSignal(string ssid, double signal)
    {
        using (var connection = new NpgsqlConnection(_config.ConnectionString))
        {
            connection.Open();

            string query = "INSERT INTO \"wifiSignal_table\" (\"SSID\", \"Signal\") VALUES (@ssid, @signal) on conflict (\"SSID\") do update set \"Signal\" = excluded.\"Signal\"";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("ssid", ssid);
                command.Parameters.AddWithValue("signal", signal);

                command.ExecuteNonQuery();
            }
        }
    }
}
