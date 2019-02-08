using System.Configuration;
using System.Data.SqlClient;

namespace GhigoWeb.Helpers
{
    public static class DB
    {
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

        public static SqlConnection GetOpenedConnection(bool mars = false)
        {
            var cs = ConnectionString;
            if (mars)
            {
                SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder(cs);
                scsb.MultipleActiveResultSets = true;
                cs = scsb.ConnectionString;
            }
            var connection = new SqlConnection(cs);
            connection.Open();
            return connection;
        }

        public static SqlConnection GetClosedConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}