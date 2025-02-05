using System.Timers;

namespace Models
{
    public partial class Piece
    {
        public static bool AllInStock(ICollection<Piece> pieces, Dictionary<int, int> pieceQuantities)
        {
            return pieces.All(piece => pieceQuantities[piece.Id] <= piece.AmountInStock);
        }
        
        public override string ToString()
        {
            return $"Piece: {Name} (ID: {Id}, In Stock: {AmountInStock})";
        }
    }

    public partial class Set
    {
        public static Dictionary<int, int> CombinedPieceQuantities(ICollection<Set> sets)
        {
            var combinedPieceQuantities = new Dictionary<int, int>();

            foreach (var set in sets)
            {
                foreach (var pieceQuantity in set.PieceQuantities)
                {
                    if (combinedPieceQuantities.ContainsKey(pieceQuantity.Key.Id))
                        combinedPieceQuantities[pieceQuantity.Key.Id] += pieceQuantity.Value;
                    else
                        combinedPieceQuantities[pieceQuantity.Key.Id] = pieceQuantity.Value;
                }
            }

            return combinedPieceQuantities;
        }

        public override string ToString()
        {
            // Format manual pages with deeper indentation
            string manualPages = ManualPages.Count > 0
                ? string.Join(Environment.NewLine, ManualPages.Select(page => $"|-- |-- {page}"))
                : "|-- |-- No Manual Pages";

            // Format piece quantities with deeper indentation
            string pieceQuantities = PieceQuantities.Count > 0
                ? string.Join(Environment.NewLine, PieceQuantities.Select(pq => $"|-- |-- {pq.Key.Name} (ID: {pq.Key.Id}, set quantity: {pq.Value})"))
                : "|-- |-- No Pieces";

            // Build the tree-like structure
            return $@"Set: {Name} (ID: {Id}) 
|-- Manual Pages 
{manualPages} 
|-- Pieces 
{pieceQuantities}";
        }
    }

    public partial class Order
    {
        public bool CheckFulfillability(ICollection<Piece> allPieces)
        {
            // Get combined piece quantities required by all sets in the order
            var requiredQuantities = Set.CombinedPieceQuantities(Sets);

            // Check if all required pieces are in stock
            return requiredQuantities.All(req =>
            {
                var piece = allPieces.FirstOrDefault(p => p.Id == req.Key);
                return piece != null && piece.AmountInStock >= req.Value;
            });
        }

        public override string ToString()
        {
            // Format sets with deeper indentation
            string sets = Sets.Count > 0
                ? string.Join(Environment.NewLine, Sets.Select(set => $"|-- {set.Name} (ID: {set.Id})"))
                : "|-- No Sets";

            return $@"Order: ID {Id} 
|-- Fulfillable: {IsFulfillable} 
|-- Finished: {IsFinished} 
|-- Fulfilling User ID: {(FulfillingUserId.HasValue ? FulfillingUserId.ToString() : "None")} 
|-- Sets 
{sets}";
        }
    }

    public partial class User
    {
        public override string ToString()
        {
            return $@"UserId: {Id} 
|-- Name: {Name}
|-- Password: {Password}";
        }
    }
}