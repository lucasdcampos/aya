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
        
        var allLegalMoves = _generator.GenerateLegalMoves().ToList();
        if (!allLegalMoves.Any()) return 0;

        // Ensure we have a default best move in case all evals are equal
        BestMove = allLegalMoves[0];

        // Order moves at the root
        var orderedMoves = OrderMoves(allLegalMoves);

        int alpha = int.MinValue + 1;
        int beta = int.MaxValue - 1;
        bool maximizing = _board.ActiveColor == PieceColor.White;

        if (maximizing)
        {
            int maxEval = int.MinValue + 1;
            foreach (var move in orderedMoves)
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
            foreach (var move in orderedMoves)
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

        // Apply Move Ordering
        var orderedMoves = OrderMoves(legalMoves);

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue + 1;
            foreach (var move in orderedMoves)
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
            foreach (var move in orderedMoves)
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

    private IEnumerable<Move> OrderMoves(IEnumerable<Move> moves)
    {
        return moves.OrderByDescending(move => ScoreMove(move));
    }

    private int ScoreMove(Move move)
    {
        int score = 0;
        Piece movingPiece = _board.GetPiece(move.FromFile, move.FromRank);
        Piece targetPiece = _board.GetPiece(move.ToFile, move.ToRank);

        // 1. MVV-LVA (Most Valuable Victim - Least Valuable Attacker)
        if (targetPiece.Type != PieceType.None)
        {
            score = 10 * GetPieceValue(targetPiece.Type) - GetPieceValue(movingPiece.Type);
        }

        // 2. Promotions are good
        if (move.Flag == MoveFlag.Promotion)
        {
            score += GetPieceValue(move.PromotionType);
        }

        return score;
    }

    private int GetPieceValue(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => 100,
            PieceType.Knight => 320,
            PieceType.Bishop => 330,
            PieceType.Rook => 500,
            PieceType.Queen => 900,
            PieceType.King => 20000,
            _ => 0
        };
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
