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

    // Material value constants
    private const int pVal = 100; // Pawn   value
    private const int nVal = 300; // kNight value
    private const int bVal = 300; // Bishop value
    private const int rVal = 500; // Rook   value
    private const int qVal = 900; // Queen  value

    // Additional factors for evaluating the position
    private const int centerControlPriority = 4 ;
    private const int kingSafetyPriority    = 40 ;
    private const int pieceActivityPriority = 5  ;
    private const int pawnStructurePriority = 15 ;


    // 32byte optimised board and 64 classic board are stored here
    private static byte[] /*optimisedBoard = new byte[32],*/ mainBoard = new byte[64];

    private static readonly byte[] wkPos = new byte[2];  // White king position
    private static readonly byte[] bkPos = new byte[2];  // Black king position
    private static bool whiteTurn;               // Storing the player turn
    private static bool gPieceGotEaten = false;  // Storing if the last move was a capture

    static void Main(string[] args)
    {
        Title = "Zephyr engine Delta+";              // Set the app title

        int depth = GetDepth();                       // Get the depth for the alpha-beta search
        GetEncodedBoard(false);                       // Get the board position (classic storing)
        whiteTurn = GetTurn();                        // Ask whose turn it is (true = white)

        Clear();                                      // Clear the console
        PrintParsedBoard(mainBoard);                  // Print the parsed board

        int eval = Evaluate(mainBoard, true);         // Evaluate the start board position
        Write($"\n\t\t\t\tCurrent eval: {eval}\n\t"); // Write the start board position eval result

        string continueGame = ReadLine();             // Wait for when user is ready


        // Prepare for the Alpha-beta search
        int  alpha   = int.MinValue;                  // Min for the algorithm
        int  beta    = int.MaxValue;                  // Max for the algorithm

        while (continueGame == "")
        {
            AlphaBeta(mainBoard, depth, alpha, beta, whiteTurn, out Move makeBestMove); // Start the search

            Clear();                                           // Clear the console
            ApplyMove(mainBoard, makeBestMove);                // Apply the best found move

            PrintParsedBoard(mainBoard);                       // Print the parsed board
            eval = Evaluate(mainBoard, true);                  // Evaluate the new position
            Write($"\n\t\t\t\tCurrent eval: {eval}\n\n\n\t");  // Write the eval position value

            whiteTurn = !whiteTurn;

            Write("Continue?  (press ENTER): ");
            continueGame = ReadLine();                         // Wait for when user is ready
        }
        EncodeBoard(mainBoard);                                // Print the new board code
        ReadKey();                                             // Exit the program
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
                _userInput = ReadLine().Replace(" ", "");
                _encodedBoard += _userInput;
            }
            if (_encodedBoard.Length == 64)
            {
                if (_type)
                {
                    //if (TryParseOptimisedBoard(_encodedBoard, optimisedBoard)) _validEncoding = true;
                    //else Write("Error while parsing board: unknown character");
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

    private static bool TryParseOptimisedBoard(string _encoded, byte[] _board)
    {
        byte _tempBuffer;
        for (int i = 0; i < 32; i++)
        {
            _tempBuffer = ConvertBoardToBytes(_encoded[i], true);  // Parse the even    square
            if (_tempBuffer == decodingError) return false;        // Check for illegal characters
            _board[i] = _tempBuffer;                               // Save  the parsed  piece

            if (_tempBuffer == 6)          // Try get the king position
            {
                wkPos[0] = (byte)(i / 8);  // Save the king position if we found him (White king yPos)
                wkPos[1] = (byte)(i % 8);  // White king xPos
            }
            if (_tempBuffer == 14)         // Try get the king position
            {
                bkPos[0] = (byte)(i / 8);  // Save the king position if we found him (Black king yPos)
                bkPos[1] = (byte)(i % 8);  // Black king xPos
            }


            _tempBuffer = ConvertBoardToBytes(_encoded[i], false); // Parse the uneven  square
            if (_tempBuffer == decodingError) return false; // Check for illegal characters
            _board[i] += _tempBuffer;               // Save  the parsed  piece

            if (_tempBuffer == 6)
            {
                wkPos[0] = (byte)(i / 8);
                wkPos[1] = (byte)(i % 8);
            }
            if (_tempBuffer == 14)                          // Try get the king position
            {
                bkPos[0] = (byte)(i / 8);
                bkPos[1] = (byte)(i % 8);
            }
        }
        return true;
    }
    private static bool TryParseClassicBoard(string _encoded)
    {
        for (byte i = 0; i < 64; i++)
        {
            mainBoard[i] = ConvertBoardToBytes(_encoded[i], true); // Parse the board piece by piece
            if (mainBoard[i] == decodingError) return false;       // Check for illegal characters

            if (mainBoard[i] == 6)          // Try get the king position
            {
                wkPos[0] = (byte) (i / 8);  // Save the king position if we found him (White king yPos)
                wkPos[1] = (byte) (i % 8);  // White king xPos
            }
            if (mainBoard[i] == 14)         // Try get the king position
            {
                bkPos[0] = (byte) (i / 8);  // Save the king position if we found him (Black king yPos)
                bkPos[1] = (byte) (i % 8);  // Black king xPos
            }
        }
        return true;
    }

    private static byte ConvertBoardToBytes(char _encodedPiece, bool _parsingPos)
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
    private static char ConvertBoardToChars(byte _decodedPiece, bool _parsingPos)
    {
        if (_parsingPos) // Parsing pos means the color of the squares, 
        {                // it is used to save memory used to store the board up to 2 times 
            switch (_decodedPiece)                                       // (only 32 bytes)
            {
                case wp1: return 'P';
                case wn1: return 'N';
                case wb1: return 'B';
                case wr1: return 'R';
                case wq1: return 'Q';
                case wk1: return 'K';

                case bp1: return 'p';
                case bn1: return 'n';
                case bb1: return 'b';
                case br1: return 'r';
                case bq1: return 'q';
                case bk1: return 'k';

                case 0:
                default:  return '+';
            }
        }
        else
        {
            switch (_decodedPiece)
            {
                case wp2: return 'P';
                case wn2: return 'N';
                case wb2: return 'B';
                case wr2: return 'R';
                case wq2: return 'Q';
                case wk2: return 'K';

                case bp2: return 'p';
                case bn2: return 'n';
                case bb2: return 'b';
                case br2: return 'r';
                case bq2: return 'q';
                case bk2: return 'k';

                case 0:
                default: return '+';
            }
        }
    } // Transform the board encoding from chars to bytes

    private static void PrintParsedBoard(byte[] _board)
    {
        if (gPieceGotEaten) gPieceGotEaten = false;
        else Write("\n\n\n");
            Write("\n\n\n\n\n\n\t\t\t  Parsed board (in bytes): " + _board.Length + "\n\n\n\t\t\t"); 
        if (_board.Length == 64)
        {
            for (int i = 0; i < 64; i++)
            {
                if (_board[i] == 0) ForegroundColor = ConsoleColor.DarkGray;
                if (_board[i] > 9) Write(_board[i] + "  ");
                else Write(" " + _board[i] + "  ");
                if (i % 8 == 7) Write("\n\t\t\t");

                ForegroundColor = ConsoleColor.White;
            }
        }
        else
        {
            for (int i = 0; i < 32; i++)
            {
                // Print the optimised parsed board here
            }
        }
    }
    private static void EncodeBoard(byte[] _board)
    {
        string _encodedString = "";

        if (_board.Length == 64)
        {
            for (byte i = 0; i < 64; i++)
            {
                _encodedString += ConvertBoardToChars(_board[i], true);
            }   // Encode the position char by char
        }
        else
        {
            // Do the encoding for the optimised board
        }
        _encodedString = _encodedString.Replace("++++++++", "+++++++=");
        PrintEncodedBoard(_encodedString);
    }
    private static void PrintEncodedBoard(string _encodedBoard)
    {
        Write("\t\t\t       Encoded board code:\n");
        Write("\t" + _encodedBoard + "\n\n\n\n\n\n\n\n");
    }


    private static int  Evaluate(byte[] _board, bool _printInfo)
    {
        // 1. Material balance calculation
        int _material = CalculateMaterial(_board);

        // 2. Position balance calculation
        int _position = CalculateCentralControl(_board);

        // 3. King safety 
        int _kingSafety = CalculateKingSafety(_board);

        // 4. Pawn structure
        int _pawnStructure = CalculatePawnStructure(_board);

        // Toral score formula
        int totalScore = _material + _position + _kingSafety + _pawnStructure;

        if(_printInfo) Write("\n\tMaterial: " + _material + " + Position: " + _position + " + King safety: " + _kingSafety + " + Pawn structure: " + _pawnStructure);
        return totalScore; // Positive score = good for white, negative = good for black
    } // Board evaluation (its OK, but it should definitely be better in the future)

    private static int  CalculateMaterial(byte[] _board)
    {
        int _materialEval = 0;

        // Scanning the whole board (optimised board scan not supported yet)
        for (int i = 0; i < 64; i++)
        {
            byte _piece = _board[i];
            switch (_piece)
            {
                case 1: _materialEval += pVal; break;
                case 2: _materialEval += nVal; break;
                case 3: _materialEval += bVal; break;
                case 4: _materialEval += rVal; break;
                case 5: _materialEval += qVal; break;

                case 9: _materialEval -= pVal; break;
                case 10: _materialEval -= nVal; break;
                case 11: _materialEval -= bVal; break;
                case 12: _materialEval -= rVal; break;
                case 13: _materialEval -= qVal; break;
            }
        }

        return _materialEval; // Return the material score
    }
    private static int  CalculateCentralControl(byte[] _board)
    {
        int _controlScore = 0;
        byte _piece = _board[27];
        // Central squares are: (e4, d4, e5, d5) (27, 28, 35, 36)
        if (_board[35] == 1)
        {
            if (_board[42]      == 1) _controlScore += 15;
            if (_board[44]      == 3) _controlScore += 5;
            if (_board[45]      == 2) _controlScore += 15;
            else if (_board[52] == 2) _controlScore += 10;
            _controlScore += 10;
        }
        if (_board[36] == 1)
        {
            if (_board[45] == 1)      _controlScore += 5;
            if (_board[42] == 2)      _controlScore += 5;
            else if (_board[52] == 2) _controlScore += 3;
            _controlScore += 5;
        }

        if (_board[27] == 9)
        {
            if (_board[18]      == 9)  _controlScore -= 15;
            if (_board[20]      == 11) _controlScore -= 5;
            if (_board[21]      == 10) _controlScore -= 15;
            else if (_board[12] == 10) _controlScore -= 10;
            _controlScore -= 10;
        }
        if (_board[36] == 9)
        {
            if (_board[21]      == 9)  _controlScore -= 5;
            if (_board[18]      == 10) _controlScore -= 5;
            else if (_board[11] == 10) _controlScore -= 3;
            _controlScore -= 5;
        }
        // Center control calculation


        return _controlScore * centerControlPriority;
    }
    private static int  CalculateKingSafety(byte[] _board)
    {
        int whiteKingSafety = 0;
        int blackKingSafety = 0;

        // King safety calculation
        whiteKingSafety -= CountAttacks(_board, wkPos[1], wkPos[0], false);
        blackKingSafety -= CountAttacks(_board, bkPos[1], bkPos[0], true);

        // Return the value times the KingSafety coefficient
        return (whiteKingSafety - blackKingSafety) * kingSafetyPriority;
    }
    private static int  CalculatePawnStructure(byte[] _board)
    {
        int whitePawnStructure = 0;
        int blackPawnStructure = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int piece = _board[i * 8 + j];
                if      (piece == 1) // For the white pawn
                {
                    if (IsIsolatedPawn(_board, i, j, true)) whitePawnStructure -= pawnStructurePriority;
                }
                else if (piece == 9) // For the black pawn
                {
                    if (IsIsolatedPawn(_board, i, j, false)) blackPawnStructure -= pawnStructurePriority;
                }
            }
        }

        return whitePawnStructure - blackPawnStructure;
    }

    private static int  CountAttacks(byte[] _board, int _x, int _y, bool _isBlack)
    {
        int _attacks = 0;

        for (int i = 0; i < 64; i++)
        {
            int _piece = _board[i];
            if ((_isBlack && _piece < 8) || (!_isBlack && _piece > 8))
            {
                // Checking for attacks on the king
                if (CanAttack(_board, i % 8, i / 8, _x, _y)) _attacks++;
            }
        }

        return _attacks;
    }
    private static bool IsIsolatedPawn(byte[] _board, int _x, int _y, bool isWhite)
    {
        // Check for pawn chain
        if (_y > 0 && _y < 7)
        {
            bool hasLeftPawn;
            bool hasRightPawn;
            if (isWhite)
            {
                hasLeftPawn  = (_board[_y * 8 + 9 + _x] == 1);
                hasRightPawn = (_board[_y * 8 + 7 + _x] == 1);
            }
            else
            {
                hasLeftPawn  = (_board[_y * 8 - 9 + _x] == 9);
                hasRightPawn = (_board[_y * 8 - 7 + _x] == 9);
            }
            return !hasLeftPawn && !hasRightPawn;
        }
        return true;
    }
    private static bool CanAttack(byte[] _board, int fromX, int fromY, int toX, int toY)
    {
        // Piece attack logic, true if the square can be attacked, false if it cant
        int _piece = _board[fromY * 8 + fromX];
        int _distX = Math.Abs(toX - fromX);      // Important to note that I store the abs value
        int _distY = Math.Abs(toY - fromY);      // to reduce the checks needed for the piece attacks (since they are simetrical)

        switch (_piece)
        {
            case 1:  // White Pawn
            case 9:  // Black pawn
                return (_distX == 1 && _distY == 1); // Pawns attacks diagonaly and only 1 square

            case 2:  // White kNight
            case 10: // Black kNight
                return (_distX == 2 && _distY == 1) || (_distX == 1 && _distY == 2); // kNight attacks

            case 3:  // White Bishop
            case 11: // Black Bishop
                return (_distX == _distY); // Bishops attacks diagonaly

            case 4:  // White Rook
            case 12: // Black Rook
                return (_distX == 0 || _distY == 0); // Rooks attacks vertically or horizontally

            case 5:  // White Queen
            case 13: // Black Queen
                return (_distX == 0 || _distY == 0 || _distX == _distY); // Queens attack like bishops or rooks
        }
        return false;
    }



    public class Move
    {
        public int  From { get; set; }          // Start position
        public int  To { get; set; }            // Destination position
        public byte Piece { get; set; }         // Moving piece
        public byte CapturedPiece { get; set; } // Captured piece (for display)
    }

    public static List<Move> GenerateAllMoves(byte[] board, bool isWhiteTurn)
    {
        List<Move> moves = new List<Move>();
        for (int i = 0; i < 64; i++)
        {
            byte piece = board[i];
            if (piece == 0 || (isWhiteTurn && piece > 8) || (!isWhiteTurn && piece < 9))
                continue;

            switch (piece)
            {
                case 1:  // White pawn
                    GeneratePawnMoves(board, i, true, moves);  // Calculate the moves
                    break;

                case 9:  // Black pawn
                    GeneratePawnMoves(board, i, false, moves); // Calculate the moves
                    break;
                case 2:  // White knight
                case 10: // Black knight
                    GenerateKnightMoves(board, i, moves); // Calculate the moves
                    break;

                case 3:  // White bishop
                case 11: // Black bishop
                    GenerateBishopMoves(board, i, moves); // Calculate the moves
                    break;

                case 4:  // White rook
                case 12: // Black rook
                    GenerateRookMoves(board, i, moves);   // Calculate the moves
                    break;

                case 5:  // White queen
                case 13: // Black queen
                    GenerateQueenMoves(board, i, moves);  // Calculate the moves
                    break;

                case 6:  // White king
                case 14: // Black king
                    GenerateKingMoves(board, i, moves);   // Calculate the moves
                    break;
            }
        }
        return moves;
    }

    public static byte[] SimulateMove(byte[] board, Move move)
    {
        byte[] newBoard = (byte[])board.Clone();
        newBoard[move.To] = move.Piece;
        newBoard[move.From] = 0;
        return newBoard;
    }

    public static bool IsKingInCheck(byte[] board, bool isWhiteTurn)
    {
        int kingPosition = -1;
        byte kingPiece = (byte)(isWhiteTurn ? 6 : 14);

        // We are going to find the king
        for (int i = 0; i < 64; i++)
        {
            if (board[i] == kingPiece)
            {
                kingPosition = i;
                break;
            }
        }

        if (kingPosition == -1)
            return false; // the king was not found wtf, still return NoCheck info   D: 

        // Calculate if the king is in check
        List<Move> opponentMoves = GenerateAllMoves(board, !isWhiteTurn);
        foreach (Move move in opponentMoves)
        {
            if (move.To == kingPosition)
                return true; // return that the King is in check
        }

        return false;        // return that the King is not in check
    }

    public static bool IsCheckmate(byte[] board, bool isWhiteTurn)
    {
        // Not a checkmate if the king is not in check
        if (!IsKingInCheck(board, isWhiteTurn))
            return false;

        // Generate all possible moves for the player
        List<Move> moves = GenerateAllMoves(board, isWhiteTurn);

        // Check if we can block the check with another piece
        foreach (Move move in moves)
        {
            byte[] newBoard = SimulateMove(board, move);
            if (!IsKingInCheck(newBoard, isWhiteTurn))
                return false; // Remove checkmate mark if we can block the check
        }

        return true; // Else if we cant move or block - return checkmate
    }

    public static int  AlphaBeta(byte[] board, int depth, int alpha, int beta, bool maximizingPlayer, out Move bestMove)
    {
        bestMove = null;

        // If checkmate stop calculating
        if (IsCheckmate(board, !maximizingPlayer))
        {
            // Return checkmate state
            return maximizingPlayer ? int.MinValue + depth : int.MaxValue - depth;
        }

        if (depth == 0) // return eval result after the search ended
            return Evaluate(board, false);

        List<Move> moves = GenerateAllMoves(board, maximizingPlayer);
        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Move move in moves)
            {
                byte[] newBoard = SimulateMove(board, move);
                int eval = AlphaBeta(newBoard, depth - 1, alpha, beta, false, out Move currentBestMove);
                if (eval > maxEval)
                {
                    maxEval = eval;
                    bestMove = move;
                }
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                    break;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Move move in moves)
            {
                byte[] newBoard = SimulateMove(board, move);
                int eval = AlphaBeta(newBoard, depth - 1, alpha, beta, true, out Move currentBestMove);
                if (eval < minEval)
                {
                    minEval = eval;
                    bestMove = move;
                }
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                    break;
            }
            return minEval;
        }
    }

    public static Move FindBestMove(byte[] board, int depth, bool isWhiteTurn)
    {
        Move bestMove = null;
        int bestValue = isWhiteTurn ? int.MinValue : int.MaxValue;

        List<Move> moves = GenerateAllMoves(board, isWhiteTurn);
        foreach (Move move in moves)
        {
            byte[] newBoard = SimulateMove(board, move);
            int moveValue = AlphaBeta(newBoard, depth - 1, int.MinValue, int.MaxValue, !isWhiteTurn, out Move currentBestMove);

            // Stop the search if we found a forced mate
            if (isWhiteTurn && moveValue == int.MaxValue - (depth - 1))
            {
                Console.WriteLine("Forced mate was found!");
                return move;
            }
            else if (!isWhiteTurn && moveValue == int.MinValue + (depth - 1))
            {
                Console.WriteLine("Forced mate was found!");
                return move;
            }

            if (isWhiteTurn && moveValue > bestValue)
            {
                bestValue = moveValue;
                bestMove = move;
            }
            else if (!isWhiteTurn && moveValue < bestValue)
            {
                bestValue = moveValue;
                bestMove = move;
            }
        }

        return bestMove;
    }

    public static void PrintMateSequence(byte[] board, bool isWhiteTurn, int depth)
    {
        Move bestMove = FindBestMove(board, depth, isWhiteTurn);
        if (bestMove != null)
        {
            Write($"Forced mate, sequence: {bestMove.From} -> {bestMove.To}\n");
            byte[] newBoard = SimulateMove(board, bestMove);
            PrintMateSequence(newBoard, !isWhiteTurn, depth - 1);
        }
        Write("\n");
    }




    public static void GeneratePawnMoves(byte[] board, int position, bool isWhite, List<Move> moves)
    {
        int direction = isWhite ? -8 : 8;   // Movement direction is different for each players pawns
        int startRow = isWhite ? 6 : 1;     // First row for counting the double pawn moves
        int promotionRow = isWhite ? 0 : 7; // Pawn promotion row

        // Basic pawn move
        int newPosition = position + direction;
        if (newPosition >= 0 && newPosition < 64 && board[newPosition] == 0)
        {
            if (newPosition / 8 == promotionRow)
            {
                // Pawn promotion
                moves.Add(new Move { From = position, To = newPosition, Piece = board[position], CapturedPiece = 0 });
            }
            else
            {
                moves.Add(new Move { From = position, To = newPosition, Piece = board[position], CapturedPiece = 0 });
            }

            // Double move for pawns in the start position
            if (position / 8 == startRow && board[newPosition + direction] == 0)
            {
                moves.Add(new Move { From = position, To = newPosition + direction, Piece = board[position], CapturedPiece = 0 });
            }
        }

        // Diagonal attacks
        int[] attackOffsets = isWhite ? new int[] { -7, -9 } : new int[] { 7, 9 }; // Offset after attacks
        foreach (int offset in attackOffsets)
        {
            int attackPosition = position + offset;
            if (attackPosition >= 0 && attackPosition < 64 && Math.Abs((attackPosition % 8) - (position % 8)) == 1)
            {
                byte targetPiece = board[attackPosition];
                if (targetPiece != 0 && ((isWhite && targetPiece > 8) || (!isWhite && targetPiece < 9)))
                {
                    if (attackPosition / 8 == promotionRow)
                    {
                        // Check for promotion after attack
                        moves.Add(new Move { From = position, To = attackPosition, Piece = board[position], CapturedPiece = targetPiece });
                    }
                    else
                    {
                        moves.Add(new Move { From = position, To = attackPosition, Piece = board[position], CapturedPiece = targetPiece });
                    }
                }
            }
        }
    }

    public static void GenerateKnightMoves(byte[] board, int position, List<Move> moves)
    {
        int[] knightOffsets = { -17, -15, -10, -6, 6, 10, 15, 17 }; // All possible knight moves
        byte piece = board[position];
        bool isWhite = piece < 9;

        foreach (int offset in knightOffsets)
        {
            int newPosition = position + offset;
            if (newPosition >= 0 && newPosition < 64 && Math.Abs((newPosition % 8) - (position % 8)) <= 2)
            {
                byte targetPiece = board[newPosition];
                if (targetPiece == 0 || (isWhite && targetPiece > 8) || (!isWhite && targetPiece < 9))
                {
                    moves.Add(new Move { From = position, To = newPosition, Piece = piece, CapturedPiece = targetPiece });
                }
            }
        }
    }

    public static void GenerateBishopMoves(byte[] board, int position, List<Move> moves)
    {
        int[] bishopOffsets = { -9, -7, 7, 9 }; // Diagonal moves
        byte piece = board[position];
        bool isWhite = piece < 9;

        foreach (int offset in bishopOffsets)
        {
            int newPosition = position + offset;
            while (newPosition >= 0 && newPosition < 64 && Math.Abs((newPosition % 8) - (position % 8)) == Math.Abs((newPosition / 8) - (position / 8)))
            {
                byte targetPiece = board[newPosition];
                if (targetPiece == 0)
                {
                    moves.Add(new Move { From = position, To = newPosition, Piece = piece, CapturedPiece = 0 });
                }
                else
                {
                    if ((isWhite && targetPiece > 8) || (!isWhite && targetPiece < 9))
                    {
                        moves.Add(new Move { From = position, To = newPosition, Piece = piece, CapturedPiece = targetPiece });
                    }
                    break; // if a piece is found
                }
                newPosition += offset;
            }
        }
    }

    public static void GenerateRookMoves(byte[] board, int position, List<Move> moves)
    {
        int[] rookOffsets = { -8, -1, 1, 8 }; // Horizontal and vertical rook offsets
        byte piece = board[position];
        bool isWhite = piece < 9;

        foreach (int offset in rookOffsets)
        {
            int newPosition = position + offset;
            while (newPosition >= 0 && newPosition < 64 && (offset == 8 || offset == -8 || (newPosition / 8 == position / 8)))
            {
                byte targetPiece = board[newPosition];
                if (targetPiece == 0)
                {
                    moves.Add(new Move { From = position, To = newPosition, Piece = piece, CapturedPiece = 0 });
                }
                else
                {
                    if ((isWhite && targetPiece > 8) || (!isWhite && targetPiece < 9))
                    {
                        moves.Add(new Move { From = position, To = newPosition, Piece = piece, CapturedPiece = targetPiece });
                    }
                    break; // if a piece is found
                }
                newPosition += offset;
            }
        }
    }

    public static void GenerateQueenMoves(byte[] board, int position, List<Move> moves)
    {                                                // The queen is literally just a rook + bishop
        GenerateBishopMoves(board, position, moves); // Diagonal
        GenerateRookMoves(board, position, moves);   // Horizontal and vertical
    }

    public static void GenerateKingMoves(byte[] board, int position, List<Move> moves)
    {
        int[] kingOffsets = { -9, -8, -7, -1, 1, 7, 8, 9 }; // All posible king moves
        byte piece = board[position];
        bool isWhite = piece < 9;

        foreach (int offset in kingOffsets)
        {
            int newPosition = position + offset;
            if (newPosition >= 0 && newPosition < 64 && Math.Abs((newPosition % 8) - (position % 8)) <= 1)
            {
                byte targetPiece = board[newPosition];
                if (targetPiece == 0 || (isWhite && targetPiece > 8) || (!isWhite && targetPiece < 9))
                {
                    moves.Add(new Move { From = position, To = newPosition, Piece = piece, CapturedPiece = targetPiece });
                }
            }
        }
    }


    public static void ApplyMove(byte[] _board, Move move)
    {
        if (_board[move.To] != 0)      // If the moved to square is not empty
        {                             // Print  which piece was captured
            Write("\n\n\n\t\t\t   [!]  - ");
            switch (_board[move.To])
            {
                case wp1: 
                    Write("White pawn"); 
                    break;

                case wn1:
                    Write("White knight");
                    break;

                case wb1:
                    Write("White bishop");
                    break;

                case wr1:
                    Write("White rook");
                    break;

                case wq1:
                    Write("White queen");
                    break;



                case bp1:
                    Write("Black pawn");
                    break;

                case bn1:
                    Write("Black knight");
                    break;

                case bb1:
                    Write("Black bishop");
                    break;

                case br1:
                    Write("Black rook");
                    break;

                case bq1:
                    Write("Black queen");
                    break;
            }
            Write(" was taken");
            gPieceGotEaten = true;
        }

        _board[move.To] = move.Piece;  // Move  the piece to the new square
        _board[move.From] = 0;         // Clear the previous square
    }

    public static bool GetTurn()
    {
        string _userInput = "";

        Write("\n\tWhose turn? (W / Y / YES / 1  = player white turn): ");
        _userInput = ReadLine().Trim().ToLower();

        if (_userInput == "w" || _userInput == "y" || _userInput == "yes" || _userInput == "1") 
            return true;  // true  = white's turn
        return false;     // false = black's turn
    }

    public static int GetDepth()
    {
        string _userInput = "";
        int _depth = -1;
        Write("Enter the depth for the algorithm search (only from 1 to 6): ");
        while (_depth < 1 || _depth > 6)
        {
            _userInput = ReadLine();
            Clear();
            if (!int.TryParse(_userInput, out _depth)) Write("Invalid input, please try again: ");
            else if (_depth < 1 || _depth > 6) Write("Out of bounds, please enter a valid number from the interval: ");
        }
        return _depth;
    }
}
    