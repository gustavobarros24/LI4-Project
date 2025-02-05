namespace DataAccess
{
    public class UserDAO : IGenericDAO<User>
    {

        private static UserDAO? _singleton = null;
        public UserDAO() { }

        public static UserDAO GetInstance()
        {
            if (_singleton == null)
                _singleton = new UserDAO();

            return _singleton;
        }

        public int Size()
        {
            int size = 0;
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM [User]", connection))
                {
                    size = (int)command.ExecuteScalar();
                }
            }
            return size;
        }

        public ICollection<User> GetAll()
        {
            List<User> users = new List<User>();
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT id_user, nome, senha FROM [User]";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id_user")),
                                Name = reader.GetString(reader.GetOrdinal("nome")),
                                Password = reader.GetString(reader.GetOrdinal("senha"))
                            });
                        }
                    }
                }
            }
            return users;
        }

        public User? GetById(int id)
        {
            User user = null;

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT id_user, nome, senha FROM [User] WHERE id_user = @id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id_user")),
                                Name = reader.GetString(reader.GetOrdinal("nome")),
                                Password = reader.GetString(reader.GetOrdinal("senha"))
                            };
                        }
                    }
                }
            }
                return user;
        }
    
        
        public bool VerifyAdminPin(int pinInput)
        {
            // check if argument matches any row's code from the admin table
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT * FROM [admin] WHERE code = @pin";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pin", pinInput);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        return reader.Read();
                    }
                }
            }
        }

    }
}
