namespace Aya;

public static class OpeningBook
{
    private static readonly Dictionary<string, string> Book = new()
    {
        // Initial position -> 1. e4
        { "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -", "e2e4" },
        
        // After 1. e4 -> 1... e5 (Open Game)
        { "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3", "e7e5" },
        
        // After 1. e4 e5 -> 2. Nf3
        { "rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6", "g1f3" },
        
        // After 1. e4 e5 2. Nf3 -> 2... Nc6
        { "rnbqkbnr/pppp1ppp/8/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq -", "b8c6" },
        
        // After 1. e4 e5 2. Nf3 Nc6 -> 3. Bc4 (Italian Game)
        { "r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq -", "c4b5" }, // Ruy Lopez
        
        // Sicilian Defense 1... c5
        { "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", "e7e5" } // Fallback
    };

    public static string? GetMove(string fen)
    {
        // We match only the first 4 parts of FEN (placement, turn, castling, en passant)
        string[] parts = fen.Split(' ');
        if (parts.Length < 4) return null;
        
        string key = $"{parts[0]} {parts[1]} {parts[2]} {parts[3]}";
        return Book.TryGetValue(key, out string? move) ? move : null;
    }
}
