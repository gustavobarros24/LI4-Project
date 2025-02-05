@code {
    private List<Piece> pieces = new();
    private Dictionary<int, int> pieceIdIncrementToStock = new();

    private void Submit()
    {
        PieceDAO.GetInstance().AddToStock(pieceIdIncrementToStock);
        NavigationManager.NavigateTo("/pieces");
    }

    protected override async Task OnInitializedAsync()
    {
        pieces = PieceDAO.GetInstance().GetAll()
            .OrderBy(piece => piece.Id)
            .ToList();

        // Initialize dictionary
        foreach (var piece in pieces)
        {
            pieceIdIncrementToStock[piece.Id] = 0;
        }
    }
}