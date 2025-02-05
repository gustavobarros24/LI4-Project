
namespace DataAccess
{
    internal class DatabaseAccess
    {
        public const string MACHINE = "localhost\\SQLEXPRESS";
        public const string DATABASE = "AutoBrick";

        public static string GetConnectionString()
        {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();
            csb.DataSource = MACHINE;
            csb.InitialCatalog = DATABASE;
            csb.IntegratedSecurity = true;
            csb.TrustServerCertificate = true; // Bypass SSL certificate validation
            return csb.ConnectionString;
        }

        public static void TestConnection()
        {
            string connectionString = GetConnectionString();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Connection successful!");

                    // Optional: Test a simple query
                    using (SqlCommand command = new SqlCommand("SELECT 1", connection))
                    {
                        var result = command.ExecuteScalar();
                        Console.WriteLine($"Test query result: {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection failed.");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
