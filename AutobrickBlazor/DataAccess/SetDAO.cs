using System.Data;

namespace DataAccess
{
    public class SetDAO: IGenericDAO<Set>
    {
        
        private static SetDAO? _singleton = null;
        public SetDAO() { }

        public static SetDAO GetInstance()
        {
            if (_singleton == null)
            {
                _singleton = new SetDAO();
            }
            return _singleton;
        }

        public int Size()
        {
            int size = 0;
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM [Set]", connection))
                {
                    size = (int)command.ExecuteScalar();
                }
            }
            return size;
        }

        public ICollection<Set> GetAll()
        {
            Dictionary<int, Set> sets = new Dictionary<int, Set>();

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Get all sets without reference to manual pages or piece quantities
                        string getSetsQuery = "SELECT id_set, nome FROM [set]";
                        using (SqlCommand command = new SqlCommand(getSetsQuery, connection, transaction))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int setId = reader.GetInt32(reader.GetOrdinal("id_set"));
                                    string setName = reader.GetString(reader.GetOrdinal("nome"));

                                    sets.Add(setId, new Set
                                    {
                                        Id = setId,
                                        Name = setName,
                                        ManualPages = new List<string>(),
                                        PieceQuantities = new Dictionary<Piece, int>()
                                    });
                                }
                            }
                        }

                        // Step 2: Get manual pages for each set
                        string getManualPagesQuery = "SELECT set_id_set, id_manualpagepath FROM ManualPage";
                        using (SqlCommand command = new SqlCommand(getManualPagesQuery, connection, transaction))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int setId = reader.GetInt32(reader.GetOrdinal("set_id_set"));
                                    string manualPath = reader.GetString(reader.GetOrdinal("id_manualpagepath"));

                                    var set = sets[setId];
                                    if (set != null)
                                    {
                                        sets[setId].ManualPages.Add(manualPath);
                                    }
                                }
                            }
                        }

                        // Step 3: Get pieces and their quantities for each set
                        string getPiecesQuery = @"
                            SELECT sh.set_id_set, p.id_piece, p.nome, p.stock, sh.quantity
                            FROM set_has_piece sh
                            JOIN piece p ON sh.piece_id_piece = p.id_piece";
                        using (SqlCommand command = new SqlCommand(getPiecesQuery, connection, transaction))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int setId = reader.GetInt32(reader.GetOrdinal("set_id_set"));
                                    int pieceId = reader.GetInt32(reader.GetOrdinal("id_piece"));
                                    string pieceName = reader.GetString(reader.GetOrdinal("nome"));
                                    int pieceAmountInStock = reader.GetInt32(reader.GetOrdinal("stock"));
                                    int setPieceQuantity = reader.GetInt32(reader.GetOrdinal("quantity"));

                                    var set = sets[setId];
                                    if (set != null)
                                    {
                                        var piece = new Piece
                                        {
                                            Id = pieceId,
                                            Name = pieceName,
                                            AmountInStock = pieceAmountInStock
                                        };

                                        set.PieceQuantities[piece] = setPieceQuantity;
                                    }
                                }
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction if any query fails
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return sets.Values.ToList().OrderBy(set => set.Id).ToList();
        }

        public Set GetById(int setId)
        {
            Set resultSet = null;

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Get the specific set without references to manual pages or piece quantities
                        string getSetQuery = "SELECT id_set, nome FROM [set] WHERE id_set = @SetId";
                        using (SqlCommand command = new SqlCommand(getSetQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@SetId", setId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    resultSet = new Set
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("id_set")),
                                        Name = reader.GetString(reader.GetOrdinal("nome")),
                                        ManualPages = new List<string>(),
                                        PieceQuantities = new Dictionary<Piece, int>()
                                    };
                                }
                            }
                        }

                        if (resultSet == null)
                        {
                            // If no set was found, return null or throw an exception if desired
                            return null;
                        }

                        // Step 2: Get manual pages for the specific set
                        string getManualPagesQuery = "SELECT id_manualpagepath FROM ManualPage WHERE set_id_set = @SetId";
                        using (SqlCommand command = new SqlCommand(getManualPagesQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@SetId", setId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string manualPath = reader.GetString(reader.GetOrdinal("id_manualpagepath"));
                                    resultSet.ManualPages.Add(manualPath);
                                }
                            }
                        }

                        // Step 3: Get pieces and their quantities for the specific set
                        string getPiecesQuery = @"
                    SELECT p.id_piece, p.nome, p.stock, sh.quantity
                    FROM set_has_piece sh
                    JOIN piece p ON sh.piece_id_piece = p.id_piece
                    WHERE sh.set_id_set = @SetId";
                        using (SqlCommand command = new SqlCommand(getPiecesQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@SetId", setId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int pieceId = reader.GetInt32(reader.GetOrdinal("id_piece"));
                                    string pieceName = reader.GetString(reader.GetOrdinal("nome"));
                                    int pieceAmountInStock = reader.GetInt32(reader.GetOrdinal("stock"));
                                    int setPieceQuantity = reader.GetInt32(reader.GetOrdinal("quantity"));

                                    var piece = new Piece
                                    {
                                        Id = pieceId,
                                        Name = pieceName,
                                        AmountInStock = pieceAmountInStock
                                    };

                                    resultSet.PieceQuantities[piece] = setPieceQuantity;
                                }
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction if any query fails
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return resultSet;
        }

        public ICollection<Set> GetAllFromOrder(int orderId)
        {
            // get all sets that are referenced by some orderId, via table encomenda_has_set

            Dictionary<int, Set> sets = new Dictionary<int, Set>();


            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    // Step 1: Get all sets without reference to manual pages or piece quantities
                    string getSetsQuery = @"
                        SELECT s.id_set, s.nome
                        FROM [set] s
                        JOIN encomenda_has_set ehs ON s.id_set = ehs.set_id_set
                        WHERE ehs.encomenda_id_encomenda = @OrderId";
                    using (SqlCommand command = new SqlCommand(getSetsQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int setId = reader.GetInt32(reader.GetOrdinal("id_set"));
                                string setName = reader.GetString(reader.GetOrdinal("nome"));
                                sets.Add(setId, new Set
                                {
                                    Id = setId,
                                    Name = setName,
                                    ManualPages = new List<string>(),
                                    PieceQuantities = new Dictionary<Piece, int>()
                                });
                            }
                        }
                    }

                    // Step 2: Get manual pages for each set
                    string getManualPagesQuery = @"
                        SELECT set_id_set, id_manualpagepath
                        FROM ManualPage
                        WHERE set_id_set IN (SELECT id_set FROM [set] WHERE id_set IN (SELECT set_id_set FROM encomenda_has_set WHERE encomenda_id_encomenda = @OrderId))";
                    using (SqlCommand command = new SqlCommand(getManualPagesQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int setId = reader.GetInt32(reader.GetOrdinal("set_id_set"));
                                string manualPath = reader.GetString(reader.GetOrdinal("id_manualpagepath"));
                                var set = sets[setId];
                                if (set != null)
                                {
                                    sets[setId].ManualPages.Add(manualPath);
                                }
                            }
                        }
                    }
                }
            }

            return sets.Values.ToList()
                .OrderBy(set => set.Id)
                .ToList();
        }


        public List<String> GetSetManual(int id){
            List<String> result = new List<string>();
            using(SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString())){
                connection.Open();
                using(SqlTransaction transaction = connection.BeginTransaction()){
                    try{
                        string sql = "SELECT id_manualpagepath FROM ManualPage WHERE set_id_set = @SetId";
                        using (SqlCommand command = new SqlCommand(sql, connection, transaction)){
                            command.Parameters.AddWithValue("@SetId", id);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string manualPath = reader.GetString(reader.GetOrdinal("id_manualpagepath"));
                                    result.Add(manualPath);
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    catch{
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            return result;
        }

    }
}
