

using System.ComponentModel.DataAnnotations;

namespace DataAccess
{
    public class OrderDAO : IGenericDAO<Order>
    {
        private static OrderDAO? _singleton = null;

        public OrderDAO() { }

        public static OrderDAO GetInstance()
        {
            if (_singleton == null)
            {
                _singleton = new OrderDAO();
            }
            return _singleton;
        }

        public int Size()
        {
            int size = 0;
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Order", connection))
                {
                    size = (int)command.ExecuteScalar();
                }
            }
            return size;
        }

        public ICollection<Order> GetAll()
        {
            List<Order> orders = new List<Order>();

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Retrieve all orders
                        string getOrdersQuery = @"
                            SELECT id_encomenda, isfulfillable, isfinished, user_id_user
                            FROM encomenda";
                        using (SqlCommand command = new SqlCommand(getOrdersQuery, connection, transaction))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    orders.Add(new Order
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("id_encomenda")),
                                        IsFulfillable = (reader.GetByte(reader.GetOrdinal("isfulfillable")) == 1),
                                        IsFinished = (reader.GetByte(reader.GetOrdinal("isfinished")) == 1),
                                        FulfillingUserId = reader.IsDBNull(reader.GetOrdinal("user_id_user")) ? null : reader.GetInt32(reader.GetOrdinal("user_id_user")),
                                        Sets = new List<Set>()
                                    });
                                }
                            }
                        }

                        // Step 2: Retrieve all sets associated with orders
                        string getOrderSetsQuery = @"
                            SELECT eh.encomenda_id_encomenda, s.id_set, s.nome
                            FROM encomenda_has_set eh
                            JOIN [set] s ON eh.set_id_set = s.id_set";
                        using (SqlCommand command = new SqlCommand(getOrderSetsQuery, connection, transaction))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int orderId = reader.GetInt32(reader.GetOrdinal("encomenda_id_encomenda"));
                                    int setId = reader.GetInt32(reader.GetOrdinal("id_set"));
                                    string setName = reader.GetString(reader.GetOrdinal("nome"));

                                    var order = orders.FirstOrDefault(o => o.Id == orderId);
                                    if (order != null)
                                    {
                                        order.Sets.Add(new Set
                                        {
                                            Id = setId,
                                            Name = setName,
                                            ManualPages = new List<string>(),
                                            PieceQuantities = new Dictionary<Piece, int>()
                                        });
                                    }
                                }
                            }
                        }

                        // Step 3: Retrieve manual pages for all sets
                        string getManualPagesQuery = @"
                            SELECT set_id_set, id_manualpagepath
                            FROM ManualPage";
                        
                        using (SqlCommand command = new SqlCommand(getManualPagesQuery, connection, transaction))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int setId = reader.GetInt32(0);
                                    string manualPath = reader.GetString(1);

                                    foreach (var order in orders)
                                    {
                                        var set = order.Sets.FirstOrDefault(s => s.Id == setId);
                                        if (set != null)
                                        {
                                            set.ManualPages.Add(manualPath);
                                        }
                                    }
                                }
                            }
                        }

                        // Step 4: Retrieve pieces and their quantities for each set
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
                                    int stock = reader.GetInt32(reader.GetOrdinal("stock"));
                                    int quantity = reader.GetInt32(reader.GetOrdinal("quantity"));

                                    foreach (var order in orders)
                                    {
                                        var set = order.Sets.FirstOrDefault(s => s.Id == setId);
                                        if (set != null)
                                        {
                                            var piece = new Piece
                                            {
                                                Id = pieceId,
                                                Name = pieceName,
                                                AmountInStock = stock
                                            };

                                            set.PieceQuantities[piece] = quantity;
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction in case of failure
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            return orders;
        }

        public ICollection<Order> GetAllQueuedByUser(int id)
        {
            List<Order> orders = new List<Order>();

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Retrieve all orders
                        string getOrdersQuery = @"
                            SELECT id_encomenda, isfulfillable, isfinished, user_id_user
                            FROM encomenda
                            WHERE user_id_user = @id AND isfinished = @false";
                        using (SqlCommand command = new SqlCommand(getOrdersQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            command.Parameters.AddWithValue("@false", Convert.ToByte(false));
                            //command.Parameters.AddWithValue("@false", (byte)0);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    orders.Add(new Order
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("id_encomenda")),
                                        IsFulfillable = (reader.GetByte(reader.GetOrdinal("isfulfillable")) == 1),
                                        IsFinished = (reader.GetByte(reader.GetOrdinal("isfinished")) == 1),
                                        FulfillingUserId = reader.IsDBNull(reader.GetOrdinal("user_id_user")) ? null : reader.GetInt32(reader.GetOrdinal("user_id_user")),
                                        Sets = new List<Set>()
                                    });
                                }
                            }
                        }

                        // Step 2: Retrieve all sets associated with orders
                        string getOrderSetsQuery = @"
                            SELECT eh.encomenda_id_encomenda, s.id_set, s.nome
                            FROM encomenda_has_set eh
                            JOIN [set] s ON eh.set_id_set = s.id_set";
                        using (SqlCommand command = new SqlCommand(getOrderSetsQuery, connection, transaction))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int orderId = reader.GetInt32(reader.GetOrdinal("encomenda_id_encomenda"));
                                    int setId = reader.GetInt32(reader.GetOrdinal("id_set"));
                                    string setName = reader.GetString(reader.GetOrdinal("nome"));

                                    var order = orders.FirstOrDefault(o => o.Id == orderId);
                                    if (order != null)
                                    {
                                        order.Sets.Add(new Set
                                        {
                                            Id = setId,
                                            Name = setName,
                                            ManualPages = new List<string>(),
                                            PieceQuantities = new Dictionary<Piece, int>()
                                        });
                                    }
                                }
                            }
                        }
                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction in case of failure
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            return orders;                            
        }

        public ICollection<Order> GetAllPending()
        {
            List<Order> orders = new List<Order>();

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Retrieve all orders
                        string getOrdersQuery = @"
                            SELECT id_encomenda, isfulfillable, isfinished, user_id_user
                            FROM encomenda
                            WHERE user_id_user IS NULL";
                        using (SqlCommand command = new SqlCommand(getOrdersQuery, connection, transaction))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    orders.Add(new Order
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("id_encomenda")),
                                        IsFulfillable = (reader.GetByte(reader.GetOrdinal("isfulfillable")) == 1),
                                        IsFinished = (reader.GetByte(reader.GetOrdinal("isfinished")) == 1),
                                        FulfillingUserId = reader.IsDBNull(reader.GetOrdinal("user_id_user")) ? null : reader.GetInt32(reader.GetOrdinal("user_id_user")),
                                        Sets = new List<Set>()
                                    });
                                }
                            }
                        }

                        // Step 2: Retrieve all sets associated with orders
                        string getOrderSetsQuery = @"
                            SELECT eh.encomenda_id_encomenda, s.id_set, s.nome
                            FROM encomenda_has_set eh
                            JOIN [set] s ON eh.set_id_set = s.id_set";
                        using (SqlCommand command = new SqlCommand(getOrderSetsQuery, connection, transaction))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int orderId = reader.GetInt32(reader.GetOrdinal("encomenda_id_encomenda"));
                                    int setId = reader.GetInt32(reader.GetOrdinal("id_set"));
                                    string setName = reader.GetString(reader.GetOrdinal("nome"));

                                    var order = orders.FirstOrDefault(o => o.Id == orderId);
                                    if (order != null)
                                    {
                                        order.Sets.Add(new Set
                                        {
                                            Id = setId,
                                            Name = setName,
                                            ManualPages = new List<string>(),
                                            PieceQuantities = new Dictionary<Piece, int>()
                                        });
                                    }
                                }
                            }
                        }

                        // Step 3: Retrieve manual pages for all sets
                        string getManualPagesQuery = @"
                            SELECT set_id_set, id_manualpagepath
                            FROM ManualPage";
                        using (SqlCommand command = new SqlCommand(getManualPagesQuery, connection, transaction))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int setId = reader.GetInt32(0);
                                    string manualPath = reader.GetString(1);

                                    foreach (var order in orders)
                                    {
                                        var set = order.Sets.FirstOrDefault(s => s.Id == setId);
                                        if (set != null)
                                        {
                                            set.ManualPages.Add(manualPath);
                                        }
                                    }
                                }
                            }
                        }

                        // Step 4: Retrieve pieces and their quantities for each set
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
                                    int stock = reader.GetInt32(reader.GetOrdinal("stock"));
                                    int quantity = reader.GetInt32(reader.GetOrdinal("quantity"));

                                    foreach (var order in orders)
                                    {
                                        var set = order.Sets.FirstOrDefault(s => s.Id == setId);
                                        if (set != null)
                                        {
                                            var piece = new Piece
                                            {
                                                Id = pieceId,
                                                Name = pieceName,
                                                AmountInStock = stock
                                            };

                                            set.PieceQuantities[piece] = quantity;
                                        }
                                    }
                                }
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction in case of failure
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            return orders;            
        }
         
        public Order? GetById(int id)
{
            Order? order = null;

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Retrieve the specific order
                        string getOrderQuery = @"
                            SELECT id_encomenda, isfulfillable, isfinished, user_id_user
                            FROM encomenda
                            WHERE id_encomenda = @Id";
                        using (SqlCommand command = new SqlCommand(getOrderQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    order = new Order
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("id_encomenda")),
                                        IsFulfillable = (reader.GetByte(reader.GetOrdinal("isfulfillable")) == 1),
                                        IsFinished = (reader.GetByte(reader.GetOrdinal("isfinished")) == 1),
                                        FulfillingUserId = reader.IsDBNull(reader.GetOrdinal("user_id_user")) ? null : reader.GetInt32(reader.GetOrdinal("user_id_user")),
                                        Sets = new List<Set>()
                                    };
                                }
                            }
                        }

                        if (order == null)
                        {
                            // If no order is found, return null
                            return null;
                        }

                        // Step 2: Retrieve sets associated with the specific order
                        string getOrderSetsQuery = @"
                            SELECT eh.set_id_set, s.id_set, s.nome
                            FROM encomenda_has_set eh
                            JOIN [set] s ON eh.set_id_set = s.id_set
                            WHERE eh.encomenda_id_encomenda = @Id";
                        using (SqlCommand command = new SqlCommand(getOrderSetsQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int setId = reader.GetInt32(reader.GetOrdinal("id_set"));
                                    string setName = reader.GetString(reader.GetOrdinal("nome"));

                                    order.Sets.Add(new Set
                                    {
                                        Id = setId,
                                        Name = setName,
                                        ManualPages = new List<string>(),
                                        PieceQuantities = new Dictionary<Piece, int>()
                                    });
                                }
                            }
                        }

                        // Step 3: Retrieve manual pages for sets in the order
                        string getManualPagesQuery = @"
                            SELECT set_id_set, id_manualpagepath
                            FROM ManualPage
                            WHERE set_id_set IN (
                                SELECT set_id_set
                                FROM encomenda_has_set
                                WHERE encomenda_id_encomenda = @Id
                            )";
                        using (SqlCommand command = new SqlCommand(getManualPagesQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int setId = reader.GetInt32(reader.GetOrdinal("set_id_set"));
                                    string manualPath = reader.GetString(reader.GetOrdinal("id_manualpagepath"));

                                    var set = order.Sets.FirstOrDefault(s => s.Id == setId);
                                    if (set != null)
                                    {
                                        set.ManualPages.Add(manualPath);
                                    }
                                }
                            }
                        }

                        // Step 4: Retrieve pieces and their quantities for sets in the order
                        string getPiecesQuery = @"
                            SELECT sh.set_id_set, p.id_piece, p.nome, p.stock, sh.quantity
                            FROM set_has_piece sh
                            JOIN piece p ON sh.piece_id_piece = p.id_piece
                            WHERE sh.set_id_set IN (
                                SELECT set_id_set
                                FROM encomenda_has_set
                                WHERE encomenda_id_encomenda = @Id
                            )";
                        using (SqlCommand command = new SqlCommand(getPiecesQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int setId = reader.GetInt32(reader.GetOrdinal("set_id_set"));
                                    int pieceId = reader.GetInt32(reader.GetOrdinal("id_piece"));
                                    string pieceName = reader.GetString(reader.GetOrdinal("nome"));
                                    int stock = reader.GetInt32(reader.GetOrdinal("stock"));
                                    int quantity = reader.GetInt32(reader.GetOrdinal("quantity"));

                                    var set = order.Sets.FirstOrDefault(s => s.Id == setId);
                                    if (set != null)
                                    {
                                        var piece = new Piece
                                        {
                                            Id = pieceId,
                                            Name = pieceName,
                                            AmountInStock = stock
                                        };

                                        set.PieceQuantities[piece] = quantity;
                                    }
                                }
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction in case of failure
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return order;
        }

        public bool CheckIfQueuedByUser(int orderId, int userId)
        {
            // confirms if:
            // - the orderId exists
            // - isFinished is set to 0
            // - the userId matches the one in the user_id_user column
            
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string checkQuery = @"
                            SELECT id_encomenda
                            FROM encomenda
                            WHERE id_encomenda = @OrderId AND isfinished = 0 AND user_id_user = @UserId";
                        using (SqlCommand command = new SqlCommand(checkQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@OrderId", orderId);
                            command.Parameters.AddWithValue("@UserId", userId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Rollback the transaction in case of failure
                        transaction.Rollback();;
                    }
                }
            }
            return false;
        }

        public bool CheckIfFulfillable(Order order)
        {
            // For every set referenced in the order, check if its piece quantities are below the available stock

            if (order == null || order.Sets == null || !order.Sets.Any())
                throw new ArgumentException("Invalid order. Order must not be null and must contain sets.");

            var id = order.Id;

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Retrieve the specific order
                        string getOrderQuery = @"
                            SELECT id_encomenda, isfulfillable, isfinished, user_id_user
                            FROM encomenda
                            WHERE id_encomenda = @Id";
                        using (SqlCommand command = new SqlCommand(getOrderQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    order = new Order
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("id_encomenda")),
                                        IsFulfillable = (reader.GetByte(reader.GetOrdinal("isfulfillable")) == 1),
                                        IsFinished = (reader.GetByte(reader.GetOrdinal("isfinished")) == 1),
                                        FulfillingUserId = reader.IsDBNull(reader.GetOrdinal("user_id_user")) ? null : reader.GetInt32(reader.GetOrdinal("user_id_user")),
                                        Sets = new List<Set>()
                                    };
                                }
                            }
                        }

                        if (order == null)
                        {
                            // If no order is found, return null
                            return false;
                        }

                        // Step 2: Retrieve sets associated with the specific order
                        string getOrderSetsQuery = @"
                            SELECT eh.set_id_set, s.id_set, s.nome
                            FROM encomenda_has_set eh
                            JOIN [set] s ON eh.set_id_set = s.id_set
                            WHERE eh.encomenda_id_encomenda = @Id";
                        using (SqlCommand command = new SqlCommand(getOrderSetsQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int setId = reader.GetInt32(reader.GetOrdinal("id_set"));
                                    string setName = reader.GetString(reader.GetOrdinal("nome"));

                                    order.Sets.Add(new Set
                                    {
                                        Id = setId,
                                        Name = setName,
                                        ManualPages = new List<string>(),
                                        PieceQuantities = new Dictionary<Piece, int>()
                                    });
                                }
                            }
                        }

                        // Step 4: Retrieve pieces and their quantities for sets in the order, and check if they are fulfillable by the stock amount
                        bool allFulfilled = true;
                        string getPiecesQuery = @"
                            SELECT sh.set_id_set, p.id_piece, p.nome, p.stock, sh.quantity
                            FROM set_has_piece sh
                            JOIN piece p ON sh.piece_id_piece = p.id_piece
                            WHERE sh.set_id_set IN (
                                SELECT set_id_set
                                FROM encomenda_has_set
                                WHERE encomenda_id_encomenda = @Id
                            )";
                        using (SqlCommand command = new SqlCommand(getPiecesQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Id", id);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int setId = reader.GetInt32(reader.GetOrdinal("set_id_set"));
                                    int pieceId = reader.GetInt32(reader.GetOrdinal("id_piece"));
                                    string pieceName = reader.GetString(reader.GetOrdinal("nome"));
                                    int stock = reader.GetInt32(reader.GetOrdinal("stock"));
                                    int quantity = reader.GetInt32(reader.GetOrdinal("quantity"));
                                    var set = order.Sets.FirstOrDefault(s => s.Id == setId);
                                    if (set != null)
                                    {
                                        var piece = new Piece
                                        {
                                            Id = pieceId,
                                            Name = pieceName,
                                            AmountInStock = stock
                                        };
                                        set.PieceQuantities[piece] = quantity;
                                        if (stock < quantity)
                                        {
                                            allFulfilled = false;
                                            return allFulfilled;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Rollback the transaction in case of failure
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return true;
        }

        public bool PutInQueue(Order order, int userId)
        {
            if (order == null || order.Sets == null || !order.Sets.Any())
                throw new ArgumentException("Invalid order. Order must not be null and must contain sets.");

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Combine all required piece quantities for the order
                        var requiredQuantities = Set.CombinedPieceQuantities(order.Sets);

                        // Check stock availability for each piece
                        foreach (var requirement in requiredQuantities)
                        {
                            string checkStockQuery = @"
                        SELECT stock
                        FROM Piece
                        WHERE id_piece = @PieceId";

                            using (SqlCommand command = new SqlCommand(checkStockQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@PieceId", requirement.Key);
                                int stock = (int?)command.ExecuteScalar() ?? 0;

                                if (stock < requirement.Value)
                                {
                                    // Rollback transaction if stock is insufficient
                                    throw new InvalidOperationException($"Insufficient stock for Piece ID: {requirement.Key}. Required: {requirement.Value}, Available: {stock}");
                                }
                            }
                        }

                        // Consume the stock for each piece
                        foreach (var requirement in requiredQuantities)
                        {
                            string consumeStockQuery = @"
                        UPDATE Piece
                        SET stock = stock - @Quantity
                        WHERE id_piece = @PieceId";

                            using (SqlCommand command = new SqlCommand(consumeStockQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@PieceId", requirement.Key);
                                command.Parameters.AddWithValue("@Quantity", requirement.Value);
                                command.ExecuteNonQuery();
                            }
                        }

                        // Update the fulfilling user for the order
                        string updateFulfillingUserQuery = @"
                    UPDATE encomenda
                    SET user_id_user = @UserId
                    WHERE id_encomenda = @OrderId";

                        using (SqlCommand command = new SqlCommand(updateFulfillingUserQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UserId", userId);
                            command.Parameters.AddWithValue("@OrderId", order.Id);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                throw new InvalidOperationException($"Failed to set the fulfilling user for Order ID {order.Id}.");
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        // Rollback the transaction in case of failure
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Finish(int orderId)
        {
            // just set the isfinished flag to 1
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string finalizeOrderQuery = @"
                            UPDATE encomenda
                            SET isfinished = 1
                            WHERE id_encomenda = @OrderId";
                        using (SqlCommand command = new SqlCommand(finalizeOrderQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@OrderId", orderId);
                            command.ExecuteNonQuery();
                        }
                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction in case of failure
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public Dictionary<int, int> RestockMissingPieceQuantities(int orderId)
        {
            var restockedQuantities = new Dictionary<int, int>();

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Fetch all sets for the given order
                        string fetchOrderSetsQuery = @"
                    SELECT eh.set_id_set
                    FROM encomenda_has_set eh
                    WHERE eh.encomenda_id_encomenda = @OrderId";
                        List<int> setIds = new List<int>();
                        using (SqlCommand command = new SqlCommand(fetchOrderSetsQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@OrderId", orderId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    setIds.Add(reader.GetInt32(0));
                                }
                            }
                        }

                        if (!setIds.Any())
                        {
                            throw new InvalidOperationException($"No sets found for Order ID {orderId}.");
                        }

                        // Step 2: Fetch all required piece quantities for the sets
                        Dictionary<int, int> requiredQuantities = new Dictionary<int, int>();
                        string fetchPieceQuantitiesQuery = @"
                    SELECT sh.piece_id_piece, SUM(sh.quantity) AS total_quantity
                    FROM set_has_piece sh
                    WHERE sh.set_id_set IN (@SetIds)
                    GROUP BY sh.piece_id_piece";
                        using (SqlCommand command = new SqlCommand(fetchPieceQuantitiesQuery, connection, transaction))
                        {
                            // Replace @SetIds with the actual set IDs for the query
                            command.CommandText = fetchPieceQuantitiesQuery.Replace("@SetIds", string.Join(",", setIds));
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int pieceId = reader.GetInt32(0);
                                    int totalQuantity = reader.GetInt32(1);
                                    requiredQuantities[pieceId] = totalQuantity;
                                }
                            }
                        }

                        if (!requiredQuantities.Any())
                        {
                            throw new InvalidOperationException($"No piece quantities found for Order ID {orderId}.");
                        }

                        // Step 3: Fetch current stock levels
                        string fetchStockQuery = @"
                    SELECT id_piece, stock
                    FROM piece
                    WHERE id_piece IN (@PieceIds)";
                        Dictionary<int, int> stockLevels = new Dictionary<int, int>();
                        using (SqlCommand command = new SqlCommand(fetchStockQuery, connection, transaction))
                        {
                            // Replace @PieceIds with the actual piece IDs for the query
                            command.CommandText = fetchStockQuery.Replace("@PieceIds", string.Join(",", requiredQuantities.Keys));
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int pieceId = reader.GetInt32(0);
                                    int stock = reader.GetInt32(1);
                                    stockLevels[pieceId] = stock;
                                }
                            }
                        }

                        // Step 4: Calculate and restock missing quantities
                        foreach (var requirement in requiredQuantities)
                        {
                            int pieceId = requirement.Key;
                            int requiredQuantity = requirement.Value;
                            int currentStock = stockLevels.ContainsKey(pieceId) ? stockLevels[pieceId] : 0;
                            int restockQuantity = Math.Max(0, requiredQuantity - currentStock);

                            if (restockQuantity > 0) // Only add entries with a restockQuantity > 0
                            {
                                string restockQuery = @"
                            UPDATE piece
                            SET stock = stock + @RestockQuantity
                            WHERE id_piece = @PieceId";
                                using (SqlCommand command = new SqlCommand(restockQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@RestockQuantity", restockQuantity);
                                    command.Parameters.AddWithValue("@PieceId", pieceId);
                                    command.ExecuteNonQuery();
                                }

                                // Add the restocked quantity to the dictionary
                                restockedQuantities[pieceId] = restockQuantity;
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback the transaction in case of failure
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return restockedQuantities;
        }

        public void Remove(int orderId)
        {
            Console.WriteLine($"From the OrderDAO Remove method, removing {orderId}!");

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Delete entries in the encomenda_has_set table
                        string deleteOrderSetsQuery = @"
                    DELETE FROM encomenda_has_set
                    WHERE encomenda_id_encomenda = @OrderId";
                        using (SqlCommand command = new SqlCommand(deleteOrderSetsQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@OrderId", orderId);
                            command.ExecuteNonQuery();
                        }

                        // Step 2: Delete the order from the encomenda table
                        string deleteOrderQuery = @"
                    DELETE FROM encomenda
                    WHERE id_encomenda = @OrderId";
                        using (SqlCommand command = new SqlCommand(deleteOrderQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@OrderId", orderId);
                            command.ExecuteNonQuery();
                        }

                        // Commit the transaction
                        transaction.Commit();
                        Console.WriteLine($"Order {orderId} successfully removed.");
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of failure
                        transaction.Rollback();
                        Console.WriteLine($"Failed to remove order {orderId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

    }
}
