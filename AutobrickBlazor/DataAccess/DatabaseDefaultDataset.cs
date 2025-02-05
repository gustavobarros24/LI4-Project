using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public static class DatabaseDefaultDataset
    {   
        public static void WipeTables()
        {
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                var deletionQuery = @"
                    DELETE FROM encomenda_has_set;
                    DELETE FROM set_has_piece;
                    DELETE FROM ManualPage;
                    DELETE FROM encomenda;
                    DELETE FROM [user];
                    DELETE FROM [set];
                    DELETE FROM piece;
                    DELETE FROM admin;
                    ";
                connection.Open();
                using (SqlCommand command = new SqlCommand(deletionQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void SeedTables()
        {
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnectionString()))
            {
                var seedingQuery = """
                        USE AutoBrick

                        -- Povoar a tabela "piece"
                        INSERT INTO piece (id_piece, stock, nome) VALUES
                        (1, 20, 'QuadradoAzul'),
                        (2, 20, 'QuadradoVerde'),
                        (3, 20, 'RetanguloVermelho'),
                        (4, 20, 'RetanguloVerde'),
                        (5, 20, 'QuadradoAmarelo'),
                        (6, 20, 'TroncoArvore'),
                        (7, 20, 'RetanguloRoxo'),
                        (8, 20, 'QuadradoPreto');
                        -- Povoar a tabela "set"
                        INSERT INTO [set] (id_set, nome) VALUES
                        (21178, 'TheFoxLodge'),
                        (30394, 'TheSkeletonDefense'),
                        (30432, 'TheTurtleBeach'),
                        (40265, 'Tic-Tac-Toe');

                        -- Povoar a tabela "piece_has_set"
                        INSERT INTO set_has_piece (set_id_set, piece_id_piece, quantity) VALUES
                        (21178, 1, '4'), -- 21178 usa 4 peças do tipo 1 (QuadradoAzul)
                        (21178, 2, '6'), -- 21178 usa 6 peças do tipo 2 (QuadradoVerde)
                        (21178, 6, '2'), -- 21178 usa 2 peças do tipo 6 (TroncoArvore)
                        (30394, 3, '8'), -- 30394 usa 8 peças do tipo 3 (RetanguloVermelho)
                        (30394, 4, '10'), --30394 usa 10 peças do tipo 4 (RetanguloVerde)
                        (30432, 5, '12'), --30432 usa 12 peças do tipo 5 (QuadradoAmarelo)
                        (30432, 7, '7'), -- 30432 usa 7 peças do tipo 7 (RetanguloRoxo)
                        (40265, 8, '5'), -- 40265 usa 5 peças do tipo 8 (QuadradoPreto)
                        (40265, 1, '3'); -- 40265 usa 3 peças do tipo 1 (QuadradoAzul)


                        -- Povoar a tabela "user"
                        INSERT INTO [user] (id_user, nome, senha) VALUES
                        (1, 'João', 'password'),
                        (2, 'Maria', 'password'),
                        (3, 'Pedro', 'password');


                        INSERT INTO encomenda (id_encomenda, isfulfillable, isfinished, user_id_user) VALUES
                        (1, 0, 0, NULL),
                        (2, 0, 0, NULL),
                        (3, 0, 0, NULL),
                        (4, 0, 0, NULL),
                        (5, 0, 0, NULL),
                        (6, 0, 0, NULL),
                        (7, 0, 0, NULL),
                        (8, 0, 0, NULL);

                        INSERT INTO encomenda_has_set (encomenda_id_encomenda, set_id_set) VALUES
                        (1, 21178),
                        (1, 30394),
                        (2, 21178),
                        (3, 30432),
                        (4, 40265),
                        (4, 21178),
                        (5, 21178),
                        (6, 30432),
                        (7, 30432),
                        (8, 21178);

                        INSERT INTO ManualPage (id_manualpagepath, set_id_set) VALUES
                        ('21178__The_Fox_Lodge_page-0001.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0002.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0003.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0004.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0005.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0006.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0007.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0008.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0009.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0010.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0011.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0012.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0013.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0014.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0015.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0016.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0017.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0018.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0019.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0020.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0021.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0022.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0023.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0024.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0025.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0026.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0027.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0028.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0029.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0030.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0031.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0032.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0033.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0034.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0035.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0036.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0037.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0038.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0039.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0040.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0041.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0042.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0043.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0044.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0045.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0046.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0047.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0048.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0049.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0050.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0051.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0052.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0053.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0054.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0055.jpg', 21178),
                        ('21178__The_Fox_Lodge_page-0056.jpg', 21178),
                        ('30394__The_Skeleton_Defense_page-0001.jpg', 30394),
                        ('30394__The_Skeleton_Defense_page-0002.jpg', 30394),
                        ('30432__The_Turtle_Beach_page-0001.jpg', 30432),
                        ('30432__The_Turtle_Beach_page-0002.jpg', 30432),
                        ('40265__Tic-Tac-Toe_page-0001.jpg', 40265),
                        ('40265__Tic-Tac-Toe_page-0002.jpg', 40265);


                        INSERT INTO admin (code) VALUES
                        (12345678);
                        """;

                connection.Open();
                using (SqlCommand command = new SqlCommand(seedingQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
