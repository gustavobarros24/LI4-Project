
namespace DataAccess
{
    public class PieceDAO
    {
        private static PieceDAO? _singleton = null;
        
        private PieceDAO() { }
        
        public static PieceDAO GetInstance()
        {
            if (_singleton == null)
            {
                _singleton = new PieceDAO();
            }
            return _singleton;
        }

        public int Size()
        {
            int size = 0;

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Piece", connection))
                {
                    size = (int)command.ExecuteScalar();
                }
            }

            return size;
        }

        public ICollection<Piece> GetAll()
        {
            List<Piece> pieces = new List<Piece>();
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                // Use explicit column names in the query
                string query = "SELECT id_piece, stock, nome FROM Piece";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pieces.Add(new Piece
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id_piece")),   // Maps to id_piece
                                AmountInStock = reader.GetInt32(reader.GetOrdinal("stock")), // Maps to stock
                                Name = reader.GetString(reader.GetOrdinal("nome"))    // Maps to nome
                            });
                        }
                    }
                }
            }
            return pieces;
        }

        public Piece? GetById(int id)
        { 
            Piece? piece = null;

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT id_piece, stock, nome FROM Piece WHERE id_piece = @id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            piece = new Piece
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id_piece")),
                                AmountInStock = reader.GetInt32(reader.GetOrdinal("stock")),
                                Name = reader.GetString(reader.GetOrdinal("nome"))
                            };
                        }
                    }
                }
            }

            return piece;
        }

        public ICollection<Piece> GetAllWithStock()
        {
            List<Piece> pieces = new List<Piece>();
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                // Use explicit column names in the query
                string query = "SELECT id_piece, stock, nome FROM Piece WHERE stock > 0";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pieces.Add(new Piece
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id_piece")),   // Maps to id_piece
                                AmountInStock = reader.GetInt32(reader.GetOrdinal("stock")), // Maps to stock
                                Name = reader.GetString(reader.GetOrdinal("nome"))    // Maps to nome
                            });
                        }
                    }
                }
            }
            return pieces;
        }

        public void EditStock(Dictionary<int, int> pieceIdAmountInStock)
        {
            if (pieceIdAmountInStock.Count == 0) return;
            // given a dictionary that maps a piece id to the new amount in stock, update the database

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                foreach (var entry in pieceIdAmountInStock)
                {
                    string query = "UPDATE Piece SET stock = @stock WHERE id_piece = @id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@stock", entry.Value);
                        command.Parameters.AddWithValue("@id", entry.Key);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public void AddToStock(Dictionary<int,int> pieceIdIncrementToStock)
        {
            if (pieceIdIncrementToStock.Count == 0) return;

            // given a dictionary that maps a piece id to the amount to increment the stock by, update the database
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                foreach (var entry in pieceIdIncrementToStock)
                {
                    string query = "UPDATE Piece SET stock = stock + @increment WHERE id_piece = @id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@increment", entry.Value);
                        command.Parameters.AddWithValue("@id", entry.Key);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

    }
}