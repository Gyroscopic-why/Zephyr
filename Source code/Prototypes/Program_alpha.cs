using System;
using System.Collections.Generic;
using static System.Console;
class Program
{
    // Constants for the board parsing, where figures are stored in bytes (temporary)
    private const byte Empty       = 128;  // This thing will be asigned to multiple symbols which resemble an empty square
    private const byte WhitePawn   = 1;
    private const byte WhiteRook   = 2;
    private const byte WhiteKnight = 3;
    private const byte WhiteBishop = 4;
    private const byte WhiteQueen  = 5;
    private const byte WhiteKing   = 6;
    private const byte BlackPawn   = 255;
    private const byte BlackRook   = 254;
    private const byte BlackBishop = 253;   // OMG
    private const byte BlackKnight = 252;   // This is actually one of the worst code I have written
    private const byte BlackQueen  = 251;   // But I really need it to work ASAP
    private const byte BlackKing   = 250;   // So it is a problem for future me

    // 8x8 Board is stored here, this is also temporary code
    private static byte[,] board = new byte[8, 8];

    static void Main(string[] args)
    {
        string input = "";
        while (input.Length != 64)
        {
            WriteLine("Введите доску (64 символа): (BP: rnbqkbnrpppppppp+++++++=+++++++=+++++++=+++++++=PPPPPPPPRNBQKBNR, BLACK, white)");
            input = ReadLine();        // Got the encoded board

            if (input.Length != 64)    // Check for wrong input 
            {
                WriteLine("Ошибка: строка должна содержать ровно 64 символа.");
                return;                // Return the error
            }
        }

        // Try to parse the board
        ParseBoard(input);

        Write("\n\n\t     Обрабатанная доска (в байтах):\n\n\n\t"); //
        for(int i = 0; i < 64; i++)                                 //
        {                                                           //
            int x = i % 8;                                          //
            int y = i / 8;                                          //
            if(board[x, y] > 99) Write(board[x, y] + "   ");        //
            else Write(board[x, y] + "     ");                      //
            if (x == 7) Write("\n\n\n\t");                          //
        }                                                           // Print the parsed board
        ReadLine(); // Wait for when user is ready

        int bestScore = EvaluateBoard();          // Evaluate the start board position
        WriteLine($"Лучшая оценка: {bestScore}"); // Write the start board position eval result
        ReadLine();                               // Wait for when user is ready


        // Prepare for the Alpha-beta search
        int depth = 2;            // Search depth
        int alpha = int.MinValue; // Min for the algorithm
        int beta  = int.MaxValue; // Max for the algorithm
        bool isWhite = true;      // Begin with wjite

        bestScore = AlphaBeta(depth, alpha, beta, isWhite); // Start the search

        WriteLine($"Лучшая оценка: {bestScore}");           // Write the search result
    }

    
    private static void ParseBoard(string input)
    {
        for (int i = 0; i < 64; i++)
        { 
            int x = i % 8;                      // Get the x pos of the symbol that we are parsing
            int y = i / 8;                      // Get the y pos of the symbol that we are parsing
            board[x, y] = CharToByte(input[i]); // Parse the board symbol by symbol
        }
    }

    // Transform the board encoding from chars to bytes
    private static byte CharToByte(char c)
    {
        switch (c)
        {
            case 'p': return WhitePawn;
            case 'r': return WhiteRook;
            case 'b': return WhiteBishop;
            case 'n': return WhiteKnight;
            case 'q': return WhiteQueen;
            case 'k': return WhiteKing;

            case 'P': return BlackPawn;
            case 'R': return BlackRook;
            case 'B': return BlackBishop;
            case 'N': return BlackKnight;
            case 'Q': return BlackQueen;
            case 'K': return BlackKing;

            case '+':
            case '/':
            case '0':
            case '-':
            case '_':
            case '=': return Empty;
            default: throw new ArgumentException($"Неизвестный символ: {c}"); // Throw error if symbol is invalid
        };
    }

    // Search algorith (IT DOES NOT WORK)
    private static int AlphaBeta(int depth, int alpha, int beta, bool isWhite)
    {
        if (depth == 0)
        {
            return EvaluateBoard(); // If we are done searching return the Eval result
        }

        List<(int, int, int, int)> moves = GenerateMoves(isWhite); // Generate all moves

        if (isWhite)
        {
            int maxEval = int.MinValue;
            foreach (var move in moves)
            {
                byte original = board[move.Item3, move.Item4];
                board[move.Item3, move.Item4] = board[move.Item1, move.Item2];
                board[move.Item1, move.Item2] = Empty;

                int eval = AlphaBeta(depth - 1, alpha, beta, false);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);

                // Return to the original position
                board[move.Item1, move.Item2] = board[move.Item3, move.Item4];
                board[move.Item3, move.Item4] = original;

                if (beta <= alpha)
                    break; // Kill the useless moves
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in moves)
            {
                byte original = board[move.Item3, move.Item4];
                board[move.Item3, move.Item4] = board[move.Item1, move.Item2];
                board[move.Item1, move.Item2] = Empty;

                int eval = AlphaBeta(depth - 1, alpha, beta, true);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);

                // Return to the original position
                board[move.Item1, move.Item2] = board[move.Item3, move.Item4];
                board[move.Item3, move.Item4] = original;

                if (beta <= alpha)
                    break; // Kill the useless moves
            }
            return minEval;
        }
    }

    // Generate all moves
    private static List<(int, int, int, int)> GenerateMoves(bool isWhite)
    {
        List<(int, int, int, int)> moves = new List<(int, int, int, int)>();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                byte piece = board[x, y];
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
                                byte target = board[nx, ny];
                                if (target == Empty && ((isWhite && target >= 250) || (!isWhite && target <= 6)))
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
    }

    // Board evaluation (TEMPORARY PLACE HOLDER)
    private static int EvaluateBoard()
    {
        int score = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)  // Actually I should add real coeficients to this shit,
            {                            // Bcs currently a Queen weights 5, and a king weights 6
                byte piece = board[x, y];
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
