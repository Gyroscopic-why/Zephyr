using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Console;
class Program
{

    private const byte emptySquare = 0;
    // This thing will be asigned to multiple symbols which resemble an empty square

    private const byte decodingError = 8;  // Error for entering an invalid character


    private const byte wp1 = 1;   // White Pawn   at uneven (black) squares, code: P
    private const byte wn1 = 2;   // White kNight at uneven (black) squares, code: N
    private const byte wb1 = 3;   // White Bishop at uneven (black) squares, code: B
    private const byte wr1 = 4;   // White Rook   at uneven (black) squares, code: R
    private const byte wq1 = 5;   // White Queen  at uneven (black) squares, code: Q
    private const byte wk1 = 6;   // White King   at uneven (black) squares, code: K

    private const byte wp2 = 16;  // White Pawn   at even   (white) squares, code: P
    private const byte wn2 = 32;  // White kNight at even   (white) squares, code: N
    private const byte wb2 = 48;  // White Bishop at even   (white) squares, code: B
    private const byte wr2 = 64;  // White Rook   at even   (white) squares, code: R
    private const byte wq2 = 80;  // White Queen  at even   (white) squares, code: Q
    private const byte wk2 = 96;  // White King   at even   (white) squares, code: K


    private const byte bp1 = 9;   // Black Pawn   at uneven (black) squares, code: p
    private const byte bn1 = 10;  // Black kNight at uneven (black) squares, code: n
    private const byte bb1 = 11;  // Black Bishop at uneven (black) squares, code: b
    private const byte br1 = 12;  // Black Rook   at uneven (black) squares, code: r
    private const byte bq1 = 13;  // Black Queen  at uneven (black) squares, code: q
    private const byte bk1 = 14;  // Black King   at uneven (black) squares, code: k

    private const byte bp2 = 144; // Black Pawn   at even   (white) squares, code: p
    private const byte bn2 = 160; // Black kNight at even   (white) squares, code: n
    private const byte bb2 = 176; // Black Bishop at even   (white) squares, code: b
    private const byte br2 = 192; // Black Rook   at even   (white) squares, code: r
    private const byte bq2 = 208; // Black Queen  at even   (white) squares, code: q
    private const byte bk2 = 224; // Black King   at even   (white) squares, code: k

    // Constants for the board parsing, where figures are stored in bytes (temporary ?)
    // Ok now it is not so bad as it was before
    // But I still should improve this


    // 32byte main board is stored here
    private static byte[] optimisedBoard = new byte[32], mainBoard = new byte[64];

    static void Main(string[] args)
    {
        GetEncodedBoard(false); // Get the board position (classic storing)

        PrintParsedBoard(mainBoard);      // Print the parsed board
        ReadLine();                            // Wait for when user is ready

        int bestScore = EvaluateBoard();       // Evaluate the start board position
        WriteLine($"Best eval: {bestScore}");  // Write the start board position eval result
        ReadLine();                            // Wait for when user is ready


        // Prepare for the Alpha-beta search
        int  depth   = 2;              // Search depth
        int  alpha   = int.MinValue;   // Min for the algorithm
        int  beta    = int.MaxValue;   // Max for the algorithm
        bool isWhite = true;           // Begin with white

        //bestScore = AlphaBeta(depth, alpha, beta, isWhite); // Start the search

        WriteLine($"Best eval: {bestScore}");           // Write the search result
    }


    private static bool GetEncodedBoard(bool _type) // Check the type of board
    {                                               // true = optimised board, false = classic board
        bool _validEncoding = false;
        string _userInput, _encodedBoard;
        while (!_validEncoding)
        {
            _encodedBoard = "";
            Write("\nEnter the board (64 characters): (SP: rnbqkbnrpppppppp+++++++=+++++++=+++++++=+++++++=PPPPPPPPRNBQKBNR)\n");

            while (_encodedBoard.Length < 64)
            {
                _userInput = ReadLine();
                _encodedBoard += _userInput;
            }
            if (_encodedBoard.Length == 64)
            {
                if (_type)
                {
                    if (TryParseOptimisedBoard(_encodedBoard)) _validEncoding = true;
                    else Write("Error while parsing board: unknown character");
                }

                else
                {
                    if (TryParseClassicBoard(_encodedBoard)) _validEncoding = true;
                    else Write("Error while parsing board: unknown character");
                }
            }
            else Write("Error while parsing board: to many characters");
        }


        return _validEncoding;
    }
    private static bool TryParseOptimisedBoard(string _encoded)
    {
        byte _tempBuffer;
        for (int i = 0; i < 32; i++)
        {
            _tempBuffer = ConvertBoard(_encoded[i], true);  // Parse the even    square
            if (_tempBuffer == decodingError) return false; // Check for illegal characters
            mainBoard[i] = _tempBuffer;                     // Save  the parsed  piece

            _tempBuffer = ConvertBoard(_encoded[i], false); // Parse the uneven  square
            if (_tempBuffer == decodingError) return false; // Check for illegal characters
            mainBoard[i] += _tempBuffer;                    // Save  the parsed  piece
        }
        return true;
    }

    private static bool TryParseClassicBoard(string _encoded)
    {
        for (int i = 0; i < 64; i++)
        {
            mainBoard[i] = ConvertBoard(_encoded[i], true);  // Parse the board piece by piece
            if (mainBoard[i] == decodingError) return false; // Check for illegal characters
        }
        return true;
    }

    
    private static byte ConvertBoard(char _encodedPiece, bool _parsingPos)
    {
        if (_parsingPos) // Parsing pos means the color of the squares, 
        {                // it is used to save memory used to store the board up to 2 times 
            switch (_encodedPiece)                                       // (only 32 bytes)
            {
                case 'P': return wp1;
                case 'N': return wn1;
                case 'B': return wb1;
                case 'R': return wr1;
                case 'Q': return wq1;
                case 'K': return wk1;

                case 'p': return bp1;
                case 'n': return bn1;
                case 'b': return bb1;
                case 'r': return br1;
                case 'q': return bq1;
                case 'k': return bk1;

                case '+':
                case '/':
                case '0':
                case '-':
                case '_':
                case '=': return emptySquare;
                default: return decodingError;
            }
        }
        else
        {
            switch (_encodedPiece)
            {
                case 'P': return wp2;
                case 'N': return wn2;
                case 'B': return wb2;
                case 'R': return wr2;
                case 'Q': return wq2;
                case 'K': return wk2;

                case 'p': return bp2;
                case 'n': return bn2;
                case 'b': return bb2;
                case 'r': return br2;
                case 'q': return bq2;
                case 'k': return bk2;

                case '+':
                case '/':
                case '0':
                case '-':
                case '_':
                case '=': return emptySquare;
                default: return decodingError;
            }
        }
    } // Transform the board encoding from chars to bytes

    private static void PrintParsedBoard(byte[] _board)
    {
        Write("\n\n\t     Обрабатанная доска (в байтах):" + _board.Length + "\n\n\n\t"); 
        if (_board.Length == 64)
        {
            for (int i = 0; i < 64; i++)
            {
                if(_board[i] > 9) Write(_board[i] + "  ");
                else Write(" " + _board[i] + "  ");
                if (i % 8 == 7) Write("\n\t");
            }
        }
        else
        {
            for (int i = 0; i < 32; i++)
            {

            }
        }
    }
                                                            
    
    // Generate all moves
/*private static List<(int, int, int, int)> GenerateMoves(bool isWhite)
{
    List<(int, int, int, int)> moves = new List<(int, int, int, int)>();
    for (int x = 0; x < 8; x++)
    {
        for (int y = 0; y < 8; y++)
        {
            byte piece = mainBoard[x, y];
            if ((isWhite && piece >= 1 && piece <= 6) || (!isWhite && piece >= 250 && piece <= 255))
            {
                // Simplified generation
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < 8 && ny >= 0 && ny < 8)
                        {
                            byte target = mainBoard[nx, ny];
                            if (target == emptySquare && ((isWhite && target >= 250) || (!isWhite && target <= 6)))
                            {
                                moves.Add((x, y, nx, ny));
                            }
                        }
                    }
                }
            }
        }
    }
    return moves;
}*/

// Board evaluation (TEMPORARY PLACE HOLDER)
private static int EvaluateBoard()
    {
        int score = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)  // Actually I should add real coeficients to this shit,
            {                            // Bcs currently a Queen weights 5, and a king weights 6
                byte piece = mainBoard[y];
                if (piece >= 1 && piece <= 6)
                {
                    score += piece;         // Score moves up for white pieces on the board
                }
                else if (piece >= 250 && piece <= 255)
                {
                    score -= (256 - piece); // Score moves down for black pieces on the board
                }
            }
        }
        return score; // Wow this is also such bad code D:
    }
}
public class AlphaBetaSearch
{
    private readonly object lockObject = new object();
    private int bestMove;
    private int bestValue;

    /*public int FindBestMove(Board board, int depth)
    {
        bestValue = int.MinValue;
        bestMove = -1;

        var moves = board.GenerateMoves();
        var tasks = new List<Task>();

        foreach (var move in moves)
        {
            var task = Task.Run(() =>
            {
                board.MakeMove(move);
                int value = -AlphaBeta(board, depth - 1, int.MinValue, int.MaxValue, false);
                board.UndoMove(move);

                lock (lockObject)
                {
                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestMove = move;
                    }
                }
            });
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());
        return bestMove;
    }*/

    /*private int AlphaBeta(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth == 0 || board.IsGameOver())
        {
            return board.Evaluate();
        }

        var moves = board.GenerateMoves();

        if (maximizingPlayer)
        {
            int value = int.MinValue;
            foreach (var move in moves)
            {
                board.MakeMove(move);
                value = Math.Max(value, -AlphaBeta(board, depth - 1, -beta, -alpha, false));
                board.UndoMove(move);

                alpha = Math.Max(alpha, value);
                if (alpha >= beta)
                {
                    break;
                }
            }
            return value;
        }
        else
        {
            int value = int.MaxValue;
            foreach (var move in moves)
            {
                board.MakeMove(move);
                value = Math.Min(value, -AlphaBeta(board, depth - 1, -beta, -alpha, true));
                board.UndoMove(move);

                beta = Math.Min(beta, value);
                if (alpha >= beta)
                {
                    break;
                }
            }
            return value;
        }
    }*/
}
    