using System.Text.Json;

namespace RedAlertService.Email
{
    internal class DBConnection
    {
        private static bool ConnectionRead = false;
        private static RedConnection? Connection = null;
        public static string GetConnectionString()
        {
            if (!ConnectionRead)
            {

                string jsonData = File.ReadAllText("db_connection.json");
                var connection = JsonSerializer.Deserialize<RedConnection>(jsonData);
                if (connection != null)
                {
                    Connection = connection;
                    ConnectionRead= true;
                }
            }

            return Connection is null ? string.Empty : Connection.ConnectionString;
        }
    }
    internal class RedConnection
    {
        public string ConnectionString { get; set; } = string.Empty;

    }
}