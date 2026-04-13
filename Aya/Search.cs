namespace Aya;

public class Search
{
    private readonly Board _board;
    private readonly MoveGenerator _generator;
    private readonly Evaluator _evaluator;
    
    public Move BestMove { get; private set; }
    public int NodesEvaluated { get; private set; }
    public bool UsedBook { get; private set; }

    public Search(Board board)
    {
        _board = board;
        _generator = new MoveGenerator(board);
        _evaluator = new Evaluator();
    }

    public int StartSearch(int depth)
    {
        BestMove = default;
        NodesEvaluated = 0;
        UsedBook = false;

        // 1. Check Opening Book
        string? bookMoveStr = OpeningBook.GetMove(_board.GetCurrentFen());
        if (bookMoveStr != null)
        {
            var legalMoves = _generator.GenerateLegalMoves().ToList();
            var bookMove = legalMoves.FirstOrDefault(m => m.ToString() == bookMoveStr);
            
            if (!bookMove.Equals(default(Move)))
            {
                BestMove = bookMove;
                UsedBook = true;
                return _evaluator.Evaluate(_board);
            }
        }
        
        // 2. Regular Search if no book move
        var allLegalMoves = _generator.GenerateLegalMoves().ToList();
        if (!allLegalMoves.Any()) return 0;

        int alpha = int.MinValue + 1;
        int beta = int.MaxValue - 1;
        bool maximizing = _board.ActiveColor == PieceColor.White;

        if (maximizing)
        {
            int maxEval = int.MinValue + 1;
            foreach (var move in allLegalMoves)
            {
                _board.MakeMove(move);
                int eval = AlphaBeta(depth - 1, alpha, beta, false);
                _board.UndoMove(move);
                if (eval > maxEval)
                {
                    maxEval = eval;
                    BestMove = move;
                }
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue - 1;
            foreach (var move in allLegalMoves)
            {
                _board.MakeMove(move);
                int eval = AlphaBeta(depth - 1, alpha, beta, true);
                _board.UndoMove(move);
                if (eval < minEval)
                {
                    minEval = eval;
                    BestMove = move;
                }
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break;
            }
            return minEval;
        }
    }

    private int AlphaBeta(int depth, int alpha, int beta, bool maximizingPlayer)
    {
        NodesEvaluated++;
        
        var legalMoves = _generator.GenerateLegalMoves().ToList();

        if (depth == 0 || !legalMoves.Any())
        {
            if (!legalMoves.Any())
            {
                var (kFile, kRank) = FindKing(_board.ActiveColor);
                bool inCheck = _generator.IsSquareAttacked(kFile, kRank, 
                    _board.ActiveColor == PieceColor.White ? PieceColor.Black : PieceColor.White);

                if (inCheck) return maximizingPlayer ? -30000 - depth : 30000 + depth;
                return 0;
            }
            return _evaluator.Evaluate(_board);
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue + 1;
            foreach (var move in legalMoves)
            {
                _board.MakeMove(move);
                int eval = AlphaBeta(depth - 1, alpha, beta, false);
                _board.UndoMove(move);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue - 1;
            foreach (var move in legalMoves)
            {
                _board.MakeMove(move);
                int eval = AlphaBeta(depth - 1, alpha, beta, true);
                _board.UndoMove(move);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break;
            }
            return minEval;
        }
    }

    private (int file, int rank) FindKing(PieceColor color)
    {
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                Piece p = _board.GetPiece(file, rank);
                if (p.Type == PieceType.King && p.Color == color) return (file, rank);
            }
        }
        return (-1, -1);
    }
}
