using Microsoft.Data.SqlClient;

namespace FireStationApp
{
    public static class Database
    {
        private static readonly string ConnectionString =
            @"Server=.\SQLEXPRESS;Database=FireStationDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}