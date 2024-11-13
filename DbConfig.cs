public class DbConfig
{
    public string Host { get; set; } = "localhost";
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = "12345";
    public string Database { get; set; } = "wifiSignal";

    public string ConnectionString => $"Host={Host};Username={Username};Password={Password};Database={Database}";
}
