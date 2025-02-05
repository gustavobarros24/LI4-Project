namespace Models
{
    public partial class Piece
    {
        public required int Id { get; set; }

        public required string Name { get; set; }

        public int AmountInStock { get; set; } = 0;
    }

    public partial class Set
    {
        public required int Id { get; set; }

        public required string Name { get; set; } = "";

        public ICollection<string> ManualPages { get; set; } = new List<string>();

        public Dictionary<Piece, int> PieceQuantities { get; set; } = new Dictionary<Piece, int>();
    }

    public partial class Order
    {
        public int Id { get; set; }

        public bool IsFulfillable { get; set; } = false;

        public bool IsFinished { get; set; } = false;

        public int? FulfillingUserId { get; set; } = null;

        public ICollection<Set> Sets { get; set; } = new List<Set>();
    }

    public partial class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }
    }
}
