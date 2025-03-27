using System;
using System.Collections.Generic;
//using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Lifetime;
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
    private const int pVal = 101;    // Pawn   value
    private const int nVal = 300;    // kNight value
    private const int bVal = 321;    // Bishop value
    private const int rVal = 500;    // Rook   value
    private const int qVal = 915;    // Queen  value
    private const int kVal = 10000;  // King val

    // Additional factors for evaluating the position  //
    private const int rookOpenFileBonus         = 30    ;
    private const int queenOpenFileBonus        = 25    ;
    private const int openFileNotUsedPunishment = -5    ;
    ////                                                /
    private const int centerControlPriority     = 1     ;
    private const int enemyInCheckPriority      = 150   ;
    private const int piecePositionPriority     = 1     ;
    private const int kingSafetyPriority        = 40    ;
    private const int pieceActivityPriority     = 5     ;
    private const int pawnStructurePriority     = 15    ;
    //-------------------------------------------------//


    // 32byte optimised board and 64 classic board are stored here
    readonly private static byte[] /*optimisedBoard = new byte[32],*/ mainBoard = new byte[64];

    private static byte wkPos;                   // White king position
    private static byte bkPos;                   // Black king position
    private static bool whiteTurn;               // Storing the player turn
    private static bool gPieceGotEaten = false;  // Storing if the last move was a capture
    private static byte gCheckMate;              // Storing the game state
    private static int  gSkippedPositions;       // Storing the amount of skipped positions

    static void Main()
    {
        Stopwatch timeCounter;
        OutputEncoding = System.Text.Encoding.Unicode;
        Title = "Zephyr engine Eta7";                       // Set the app title
        string continueGame = "";
        Move makeBestMove = null;

        while (continueGame != "exit")
        {
            gCheckMate = 0;                                 // Reset game
            int depth = GetDepth();                         // Get the depth for the alpha-beta search
            bool boardDisplayType = GetBoardDisplayType();  // Set the board display type
            GetEncodedBoard(false);                         // Get the board position (classic storing)
            whiteTurn = GetTurn();                          // Ask whose turn it is (true = white)

            Clear();                                        // Clear the console
            DisplayBoard(mainBoard, boardDisplayType);      // Print the board position

            int eval = AdvEvaluate(mainBoard, true, true);  // Evaluate the start board position
            Write($"\n\t\t\t\tCurrent eval: {eval}\n\n\n\t");   // Write the start board position eval result
            Write("White king in check: " + IsKingInCheck(mainBoard, wkPos, true) + ", wkPos: " + wkPos);       // Print info
            Write("\n\tBlack king in check: " + IsKingInCheck(mainBoard, bkPos, false) + ", bkPos: " + bkPos);  //
            Write("\n\n\tContinue?  (press ENTER): ");

            continueGame = ReadLine();             // Wait for when user is ready


            while (continueGame == "")
            {
                gSkippedPositions = 0;                                // Reset the skipped positions
                timeCounter = Stopwatch.StartNew();
                if (gCheckMate < 3)  // If not checkmate make a move
                {
                    makeBestMove = AlphaBetaSearch(mainBoard, depth, whiteTurn); // Start the search

                    //Write("\tDepth: " + depth + ",\tResult: " + PositionsAmountTest(mainBoard, depth, whiteTurn) + " positions,\t\ttime elapsed: " + timeCounter.ElapsedMilliseconds + " ms");
                    //depth++;

                    ApplyMove(mainBoard, makeBestMove);                // Apply the best found move
                    whiteTurn = !whiteTurn;                            // Change the player turn
                    Clear();                                           // Clear the console
                }
                else
                { 
                    Clear();                                       // Clear the console
                    Write("\t\tCHECKMATE  :D");
                }

                timeCounter.Stop();
                if (makeBestMove != null) DisplayBoard(mainBoard, boardDisplayType, makeBestMove.From, makeBestMove.To); // Print the new board
                else DisplayBoard(mainBoard, boardDisplayType);

                eval = AdvEvaluate(mainBoard, whiteTurn, true);    // Evaluate the new position
                Write($"\n\t\t\t\tCurrent eval: {eval}\n\n\n\t");  // Write the eval position value


                Write("\n\tAlpha-beta skipped: " + gSkippedPositions + ", time elapsed: " + timeCounter.ElapsedMilliseconds + " ms\n\t");
                Write("White king in check: " + IsKingInCheck(mainBoard, wkPos, true) + ", wkPos: " + wkPos);       // Print info
                Write("\n\tBlack king in check: " + IsKingInCheck(mainBoard, bkPos, false) + ", bkPos: " + bkPos);  //
                Write("\n\n\tContinue?  (press ENTER): ");

                continueGame = ReadLine().Trim().ToLower();        // Wait for when user is ready

                if (TryParseUserMove(mainBoard, continueGame))     // Check for a move input from the user
                {
                    whiteTurn = !whiteTurn;                        // Change the player turn
                    continueGame = "";                             // Reset the continue game string
                }
            }
            EncodeBoard(mainBoard);                                // Print the new board code
            if (continueGame != "exit")
            {
                ForegroundColor = ConsoleColor.Green;              // Inform about the new game
                Write("\tNew game settings: ");                    //
                ForegroundColor = ConsoleColor.White;              //
            }
        }
        ReadKey();                                                 // Exit the program
    }


    private static bool GetEncodedBoard(bool _type) // Check the type of board
    {                                               // true = optimised board, false = classic board
        bool _validEncoding = false, _reset = false;
        string _userInput, _encodedBoard;
        while (!_validEncoding)
        {
            _encodedBoard = "";
            Write("\n\tEnter the board (64 characters): (SP: 0 / rnbqkbnrpppppppp32PPPPPPPPRNBQKBNR)\n\t");

            while (_encodedBoard.Length < 64 && !_validEncoding && !_reset)
            {
                _userInput = ReadLine().Replace(" ", "");
                _encodedBoard += _userInput;

                if (_userInput == "0") _encodedBoard = "rnbqkbnrpppppppp32PPPPPPPPRNBQKBNR";
                if (_encodedBoard.Length <= 64 && _encodedBoard.Length > 0)
                {
                    if (TryParseClassicBoard(ref _encodedBoard)) _validEncoding = true;
                    else if (_encodedBoard[_encodedBoard.Length - 1] == '#')
                    {
                        Clear();
                        Write("\t[!]  - Error while parsing board: to many characters\n");
                        _reset = true;
                    }
                    else if (_encodedBoard[_encodedBoard.Length - 1] == '!')
                    {
                        Clear();
                        Write("\t[!]  - Error while parsing board: unknown character\n");
                        _reset = true;
                    }
                }
                
                Write("\t");
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
                    if (TryParseClassicBoard(ref _encodedBoard)) _validEncoding = true;
                    else
                    {
                        Clear();
                        Write("\t[!]  - Error while parsing board: unknown character\n");
                    }
                }
            }
        }


        return _validEncoding;
    }

    private static bool TryParseOptimisedBoard(string _encoded, byte[] _board)
    {
        byte _tempBuffer;
        for (byte i = 0; i < 32; i++)
        {
            _tempBuffer = ConvertBoardToBytes(_encoded[i], true);  // Parse the even    square
            if (_tempBuffer == decodingError) return false;        // Check for illegal characters
            _board[i] = _tempBuffer;                               // Save  the parsed  piece

            if (_board[i] == 6)   // If the parsed piece is the white king
            {
                wkPos = i;        // Save the white king position
            }
            if (_board[i] == 14)  // If the parsed piece is the black king
            {
                bkPos = i;        // Save the black king position
            }


            _tempBuffer = ConvertBoardToBytes(_encoded[i], false); // Parse the uneven  square
            if (_tempBuffer == decodingError) return false; // Check for illegal characters
            _board[i] += _tempBuffer;               // Save  the parsed  piece

            if (_board[i] == 6)   // If the parsed piece is the white king
            {
                wkPos = i;        // Save the white king position
            }
            if (_board[i] == 14)  // If the parsed piece is the black king
            {
                bkPos = i;        // Save the black king position
            }
        }
        return true;
    } // Needs an update
    private static bool TryParseClassicBoard(ref string _encoded)
    {
        byte _fenOffset = 0;
        for (byte i = 0; i < _encoded.Length; i++)
        {
            if (i + _fenOffset < 64)
            {
                mainBoard[i + _fenOffset] = ConvertBoardToBytes(_encoded[i], true); // Parse the board piece by piece

                if (mainBoard[i + _fenOffset] == decodingError) // If encountored an unknown character
                {
                    string _temp = "";
                    if (i < _encoded.Length - 1)      // Prevent index out of range 
                    {
                        _temp += _encoded[i];     // Temporary string to stop the strange parsing errors:
                        _temp += _encoded[i + 1]; // (if you only add .ToString() to the byte.TryParse later)
                    }

                    if (byte.TryParse(_temp, out byte _freeSpaceForFen))
                    {   // Check for custom short fen - fen number confirmed

                        for (byte j = i; j < _freeSpaceForFen + i; j++)
                        {
                            if (i + _fenOffset < 64)
                            {
                                mainBoard[i + _fenOffset] = emptySquare;  // Fill empty squares
                                _fenOffset++;                             // Increase offset for board parsing
                            }
                            else
                            {
                                j += _freeSpaceForFen;
                                _encoded += "#";
                            }
                        }
                        _fenOffset -= 2;  // Account for the offset of the two chars that was converted to bytes
                        i++;
                    }
                    else if (byte.TryParse(_encoded[i].ToString(), out _freeSpaceForFen))
                    {   // Check for Fen char - Fen char confirmed

                        for (int j = i; j < _freeSpaceForFen + i; j++)
                        {
                            if (i + _fenOffset < 64)
                            {
                                mainBoard[i + _fenOffset] = emptySquare;  // Fill empty squares
                                _fenOffset++;                             // Increase offset for board parsing
                            }
                            else
                            {
                                j += _freeSpaceForFen;
                                _encoded += "#";
                            }
                        }
                        _fenOffset--;
                    }
                    else
                    {
                        _encoded += "!";
                        return false;       // Return parsing error if the charcter is invalid
                    }
                }

                if (mainBoard[i + _fenOffset] == 6)   // If the parsed piece is the white king
                {
                    wkPos = (byte)(i + _fenOffset);  // Save the white king position
                }
                if (mainBoard[i + _fenOffset] == 14)  // If the parsed piece is the black king
                {
                    bkPos = (byte)(i + _fenOffset);  // Save the black king position
                }
            }
            else
            {
                i += 128;
                _encoded += "#";
            }
        }
        if (_encoded.Length + _fenOffset != 64) return false; // If the full board isnt filled return error
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


    private static void DisplayBoard(byte[] _board, bool _displayType, byte _moveFrom = 64, byte _moveTo = 64)
    {
        if (gPieceGotEaten) gPieceGotEaten = false;
        else Write("\n\n\n");

        Write("\n\n\n");
        if (!_displayType) Write("\n\n\n");
        Write("\t\t\t\tParsed board (in bytes): " + _board.Length + "\n\n\n\t\t\t  "); 
        if (_board.Length == 64)
        {
            if (!_displayType)  // If we are displaying the board in bytes
            {
                for (int i = 0; i < 64; i++)
                {
                    if (_board[i] == 0) ForegroundColor = ConsoleColor.DarkGray;                             // Highlight empty squares
                    else if (_board[i] == 1 || _board[i] == 9) ForegroundColor = ConsoleColor.White;        // Highlight pawns
                    else if (_board[i] == 6 || _board[i] == 14) ForegroundColor = ConsoleColor.Red;          // Highlight kings
                    else if (_board[i] == 5 || _board[i] == 13) ForegroundColor = ConsoleColor.DarkMagenta;  // Highlight queens
                    else if (_board[i] < 8) ForegroundColor = ConsoleColor.DarkGreen;                        // Highlight every other white's pieces
                    else ForegroundColor = ConsoleColor.DarkBlue;                                            // Highlight every other black's pieces

                    // Highlight the square were there previously was a piece
                    if (i == _moveFrom) ForegroundColor = ConsoleColor.DarkRed;
                    else if (i == _moveTo)
                        ForegroundColor = ConsoleColor.Cyan;  // Highlight the square we the last piece moved

                    if (_board[i] > 9) Write(" " + _board[i] + "  ");  // Print alligned grid
                    else Write(" 0" + _board[i] + "  ");               //
                    if (i % 8 == 7) Write("\n\n\t\t\t  ");             // Move to new line
                }
            }
            else
            {
                Write("\t  ");
                for (int i = 0; i < 64; i++)
                {
                    if (_board[i] == 0) ForegroundColor = ConsoleColor.DarkGray;                             // Highlight empty squares
                    else if (_board[i] == 1 || _board[i] == 9) ForegroundColor = ConsoleColor.White;        // Highlight pawns
                    else if (_board[i] == 6 || _board[i] == 14) ForegroundColor = ConsoleColor.Red;          // Highlight kings
                    else if (_board[i] == 5 || _board[i] == 13) ForegroundColor = ConsoleColor.DarkMagenta;  // Highlight queens
                    else if (_board[i] < 8) ForegroundColor = ConsoleColor.DarkGreen;                        // Highlight every other white's pieces
                    else ForegroundColor = ConsoleColor.DarkBlue;                                            // Highlight every other black's pieces


                    if((i + i / 8) % 2 == 0) BackgroundColor = ConsoleColor.Gray;
                    else BackgroundColor = ConsoleColor.Black;


                    // Highlight the square were there previously was a piece
                    if (i == _moveFrom) BackgroundColor = ConsoleColor.DarkRed;
                    else if (i == _moveTo)
                        BackgroundColor = ConsoleColor.Cyan;  // Highlight the square we the last piece moved

                    switch (_board[i])
                    {
                        case 0:  Write(" . "); break;
                        case 1:  Write(" ♟ "); break;
                        case 2:  Write(" ♞ "); break;
                        case 3:  Write(" ♝ "); break;
                        case 4:  Write(" ♜ "); break;
                        case 5:  Write(" ♛ "); break;
                        case 6:  Write(" ♚ "); break;

                        case 9:  Write(" ♙ "); break;
                        case 10: Write(" ♘ "); break;
                        case 11: Write(" ♗ "); break;
                        case 12: Write(" ♖ "); break;
                        case 13: Write(" ♕ "); break;
                        case 14: Write(" ♔ "); break;
                    }
                    if (i % 8 == 7 && i < 63)
                    {
                        BackgroundColor = ConsoleColor.Black;  // Set backgroung color to black (invisible)
                        Write("\n\t\t\t\t  ");                 // Move to the new line
                    }
                }
            }
            ForegroundColor = ConsoleColor.White;
            BackgroundColor = ConsoleColor.Black;
            Write("\n\n\t\t\t");
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
        string _fullEncoded = "", _fen = "";
        byte _emptySquares = 0;

        if (_board.Length == 64)
        {
            for (byte i = 0; i < 64; i++)
            {
                _fullEncoded += ConvertBoardToChars(_board[i], true);
                if (_fullEncoded[i] != '+')
                {
                    if (_emptySquares != 0)
                    {
                        _fen += _emptySquares;
                        _emptySquares = 0;
                    }
                    _fen += _fullEncoded[i];
                }
                else _emptySquares++; // Count empty squares for the fen

            }   // Encode the position char by char
            if (_emptySquares != 0)
            {
                _fen += _emptySquares;
                _emptySquares = 0;
            }
        }
        else
        {
            // Do the encoding for the optimised board
        }
        _fullEncoded = _fullEncoded.Replace("++++++++", "+++++++/");
        PrintEncodedBoard(_fullEncoded, _fen);
    }
    private static void PrintEncodedBoard(string _encodedBoard, string _boardFen)
    {
        Write("\n\tEncoded board code: " + _encodedBoard);
        Write("\n\tCustom fen board code: " + _boardFen + "\n\n\n");
    }


    private static int  AdvEvaluate(byte[] _board, bool _isWhite, bool _writeInfo = false)
    {
        // START Positional values for each piece //
        int[] STARTpawnTable = {
         0,   0,   0,   0,   0,   0,   0,   0,
        80,  80,  80,  70,  70,  80,  80,  80,
        20,  20,  30,  30,  30,  30,  20,  20,
        10,  10,  20,  30,  30,  20,  10,  10,
         0,   0,   5,  30,  30,   5,   0,   0,
         0,   5,   0,   5,   5,   0,   5,   0,
        10,  10,   5, -20, -20,   5,  10,  10,
         0,   0,   0,   0,   0,   0,   0,   0
    };                //
        int[] STARTknightTable = {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -41, -20,   0,   5,   5,   0, -20, -41,
        -30,   0,  20,  25,  25,  20,   0, -30,
        -30,   5,  25,  35,  35,  25,   5, -30,
        -30,   0,  25,  35,  35,  25,   0, -30,
        -30,   5,  20,  25,  25,  20,   5, -30,
        -51, -20,   0,   5,   5,   0, -20, -51,
        -50, -50, -30, -30, -30, -30, -50, -50
    };              //
        int[] STARTbishopTable = {
        -25, -10, -10, -10, -10, -10, -10, -25,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -10,   0,  10,  15,  15,  10,   0, -10,
        -10,   5,  15,  20,  20,  15,   5, -10,
        -10,   0,  15,  20,  20,  15,   0, -10,
        -10,   5,  10,  15,  15,  10,   5, -10,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -25, -10, -10, -10, -10, -10, -10, -25
    };              //
        int[] STARTrookTable = {
          0,    0,    0,   0,   0,    0,    0,   0,
         10,   20,   20,  20,  20,   20,   20,  10,
        -10,    0,    0,   0,   0,    0,    0, -10,
        -10,    0,    0,   0,   0,    0,    0, -10,
        -10,    0,    0,   0,   0,    0,    0, -10,
        -10,    0,    0,   0,   0,    0,    0, -10,
        -10,    0,    0,   0,   0,    0,    0, -10,
          0,  -10,  -10,  10,  10,  -10,  -10,   0
    };                //
        int[] STARTqueenTable = {
        -20, -10, -10,  -5,  -5, -10, -10, -20,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -10,   0,  10,  10,  10,  10,   0, -10,
         -5,   0,  10,  15,  15,  10,   0,  -5,
          0,   0,  10,  15,  15,  10,   0,  -5,
        -10,   5,  10,  10,  10,  10,   0, -10,
        -10,   0,   5,   0,   0,   0,   0, -10,
        -20, -10, -10,  -5,  -5, -10, -10, -20
    };               //
        int[] STARTkingTable = {
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -20, -25, -20, -20, -20, -20, -25, -20,
         20,  20,  -5, -10, -10,  -5,  20,  20,
         20,  30,  10,  -1,  -1,  10,  30,  20
    };                //
        //----------------------------------------//

        // END Positional values for each piece //
        int[] ENDpawnTable = {
         0,    0,   0,   0,   0,   0,   0,   0,
        70,   60,  55,  50,  50,  55,  60,  70,
        20,   20,  30,  30,  30,  30,  20,  20,
        10,   10,  20,  20,  20,  20,  10,  10,
        -5,    5,   0,   0,   0,   0,   5,  -5,
        -20, -15, -15, -20, -20, -15, -15, -20,
        -30, -20, -20, -40, -40, -20, -20, -30,
         0,    0,   0,   0,   0,   0,   0,   0
    };                //
        int[] ENDknightTable = {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -41, -20,   0,   5,   5,   0, -20, -41,
        -30,   0,  20,  25,  25,  20,   0, -30,
        -30,   5,  25,  35,  35,  25,   5, -30,
        -30,   0,  25,  35,  35,  25,   0, -30,
        -30,   5,  20,  25,  25,  20,   5, -30,
        -51, -20,   0,   5,   5,   0, -20, -51,
        -50, -50, -30, -30, -30, -30, -50, -50
    };              //
        int[] ENDbishopTable = {
        -40, -10, -10, -10, -10, -10, -10, -40,
        -20,   0,   0,   0,   0,   0,   0, -20,
        -15,   0,  10,  15,  15,  10,   0, -15,
        -10,   5,  15,  20,  20,  15,   5, -10,
        -10,   0,  15,  20,  20,  15,   0, -10,
        -15,   5,  10,  15,  15,  10,   5, -15,
        -20,   0,   0,   0,   0,   0,   0, -20,
        -40, -10, -10, -10, -10, -10, -10, -40
    };              //
        int[] ENDrookTable = {
          0,   0,   0,   0,   0,   0,   0,   0,
         10,  20,  20,  20,  20,  20,  20,  10,
        -10,   0,   0,   0,   0,   0,   0,  -5,
        -10,   0,   0,   0,   0,   0,   0,  -5,
        -10,   0,   0,   0,   0,   0,   0,  -5,
        -10,   0,   0,   0,   0,   0,   0,  -5,
         -5,   0,   0,   0,   0,   0,   0,  -5,
          0,   0,   0,  10,  10,   0,   0,   0
    };                //
        int[] ENDqueenTable = {
        -20, -10, -10,  -5,  -5, -10, -10, -20,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -10,   0,  10,  10,  10,  10,   0, -10,
         -5,   0,  10,  15,  15,  10,   0,  -5,
          0,   0,  10,  15,  15,  10,   0,  -5,
        -10,   5,  10,  10,  10,  10,   0, -10,
        -10,   0,   5,   0,   0,   0,   0, -10,
        -20, -10, -10,  -5,  -5, -10, -10, -20
    };               //
        int[] ENDkingTable = {
        -50, -30, -10,  -5,  -5, -10, -30, -50,
        -30, -10,  -3,  -2,  -2,  -3, -10, -30,
        -10,  -3,  -2,  -1,  -1,  -2,  -3, -10,
         -5,  -2,  -1,   0,   0,  -1,  -2,  -5,
         -5,  -2,  -1,   0,   0,  -1,  -2,  -5,
        -10,  -3,  -2,  -1,  -1,  -2,  -3, -10,
        -30, -10,  -3,  -2,  -2,  -3, -10, -30,
        -50, -30, -10,  -5,  -5, -10, -30, -50
    };                //
        //--------------------------------------//

        int _blackMajorPieces = 0;
        int _whiteMajorPieces = 0;
        int _materialVal = CalculateMaterial(_board, ref _whiteMajorPieces, ref _blackMajorPieces);

        int _positionalVal = 0;  // Positional value will be stored here
        int _enemyInCheckVal = 0;

        if (Math.Max(_whiteMajorPieces, _blackMajorPieces) > 1700)  // Calculate position values for start and middle game
        {
            for (byte i = 0; i < 64; i++)
            {
                byte _piece = _board[i];
                if (_piece == 0) continue;
                switch (_piece)
                {
                    case 1: // White Pawn
                        _positionalVal += STARTpawnTable[i];
                        break;

                    case 2: // White kNight
                        _positionalVal += STARTknightTable[i];
                        break;

                    case 3: // White Bishop
                        _positionalVal += STARTbishopTable[i];
                        break;

                    case 4: // White Rook
                        _positionalVal += STARTrookTable[i];
                        break;

                    case 5: // White Queen
                        _positionalVal += STARTqueenTable[i];
                        break;

                    case 6: // White King
                        _positionalVal += STARTkingTable[i];
                        wkPos = i;
                        break;



                    case 9: // Black Pawn
                        _positionalVal -= STARTpawnTable[63 - i];
                        break;

                    case 10: // Black kNight
                        _positionalVal -= STARTknightTable[63 - i];
                        break;

                    case 11: // BlackBishop
                        _positionalVal -= STARTbishopTable[63 - i];
                        break;

                    case 12: // Black Rook
                        _positionalVal -= STARTrookTable[63 - i];
                        break;

                    case 13: // Black Queen
                        _positionalVal -= STARTqueenTable[63 - i];
                        break;

                    case 14: // Black King
                        _positionalVal -= STARTkingTable[63 - i];
                        bkPos = i;
                        break;
                }
                _positionalVal *= piecePositionPriority;
            }    // Count the positional value for each piece on the board
        }
        else  // Calculate position value for the end game (with different piece table scores)
        {
            for (byte i = 0; i < 64; i++)
            {
                byte _piece = _board[i];
                if (_piece == 0) continue;
                switch (_piece)
                {
                    case 1: // White Pawn
                        _positionalVal += ENDpawnTable[i];
                        break;

                    case 2: // White kNight
                        _positionalVal += ENDknightTable[i];
                        break;

                    case 3: // White Bishop
                        _positionalVal += ENDbishopTable[i];
                        break;

                    case 4: // White Rook
                        _positionalVal += ENDrookTable[i];
                        break;

                    case 5: // White Queen
                        _positionalVal += ENDqueenTable[i];
                        break;

                    case 6: // White King
                        _positionalVal += ENDkingTable[i];
                        wkPos = i;
                        break;



                    case 9: // Black Pawn
                        _positionalVal -= ENDpawnTable[63 - i];
                        break;

                    case 10: // Black kNight
                        _positionalVal -= ENDknightTable[63 - i];
                        break;

                    case 11: // BlackBishop
                        _positionalVal -= ENDbishopTable[63 - i];
                        break;

                    case 12: // Black Rook
                        _positionalVal -= ENDrookTable[63 - i];
                        break;

                    case 13: // Black Queen
                        _positionalVal -= ENDqueenTable[63 - i];
                        break;

                    case 14: // Black King
                        _positionalVal -= ENDkingTable[63 - i];
                        bkPos = i;
                        break;
                }
                _positionalVal *= piecePositionPriority;
            }    // Count the positional value for each piece on the board
        }

        if (_isWhite) if (IsKingInCheck(_board, bkPos, false)) _enemyInCheckVal += enemyInCheckPriority;  
        // Enemy is in check bonus

        else if (IsKingInCheck(_board, wkPos, true)) _enemyInCheckVal -= enemyInCheckPriority;
        // Enemy is in check bonus


        // Add capture bonus (plans for later)
        //evaluation += CalculateCaptureBonus(_board, _isWhite);

        // King safety bonus (simplified)
        int _kingSafetyVal = /*CalculateKingSafety(_board);*/ 0;

        // Bonus for capitalising on open files
        int _openFileVal = CalculateOpenFiles(_board, _isWhite);

        // Bonus for center control (simplified)
        int _centerControlVal = CalculateCenterControl(_board);

        // Bonus for the pawn structure (simplified)
        int _pawnStructureVal = CalculatePawnStructure(_board);


        // CALCULATE FINAL EVALUATION
        int _evaluation = 
            _materialVal + 
            _positionalVal + 
            _openFileVal + 
            _centerControlVal +
            _pawnStructureVal +
            _enemyInCheckVal;           // Add sum value to the eval result

        if (_writeInfo)  // Write extra info
        {
            Write("Material: " + _materialVal + " + positional: " + _positionalVal + " + open file: " + _openFileVal);
            Write("\n\t\t\t+ king safety: " + _kingSafetyVal + " + enemy in check: " + _enemyInCheckVal + " +\n\t\t\t");
            Write("center control: " + _centerControlVal + " + pawn structure: " + _pawnStructureVal);
        }

        return _evaluation;
    }
    private static int  CalculateOpenFiles(byte[] _board, bool _isWhite)
    {
        int _totalBonus = 0;
        for (int x = 0; x < 8; x++)
        {
            bool _hasPawn = false;   // Later will be used to assigned punishment

            // Check for pawns in the file, to determine if its open or not
            for (int y = 0; y < 8; y++)
            {
                byte _piece = _board[y * 8 + x];

                if ((_isWhite && _piece == 1) || (!_isWhite && _piece == 9))  // Found a pawn of our color, so the file isnt open
                {
                    _hasPawn = true;
                    break;
                }
            }

            // If the file doent contain pawns, its an open file
            if (!_hasPawn)
            {
                // Find the rooks and queens in the open file
                for (int y = 0; y < 8; y++)
                {
                    byte _piece = _board[y * 8 + x];

                    switch(_piece)
                    {
                        case 4:
                            _totalBonus += rookOpenFileBonus;
                            _hasPawn = true; // Reusing an old variable to stop the punishment
                            break;
                        case 5:
                            _totalBonus += queenOpenFileBonus;
                            _hasPawn = true; // Reusing an old variable to stop the punishment
                            break;

                        case 12:
                            _totalBonus -= rookOpenFileBonus;
                            _hasPawn = true; // Reusing an old variable to stop the punishment
                            break;
                        case 13:
                            _totalBonus -= queenOpenFileBonus;
                            _hasPawn = true; // Reusing an old variable to stop the punishment
                            break;
                    }
                }
                if (!_hasPawn) _totalBonus += _isWhite ? openFileNotUsedPunishment : -openFileNotUsedPunishment;
                // Assign a punishment if an open file is not used
            }
        }

        return _totalBonus;
    }
    private static int  CalculateMaterial(byte[] _board, ref int _whiteLargePiecesVal, ref int _blackLargePiecesVal)
    {
        int _materialEval = 0;

        // Scanning the whole board (optimised board scan not supported yet)
        for (int i = 0; i < 64; i++)
        {
            byte _piece = _board[i];
            switch (_piece)
            {
                case 1: 
                    _materialEval += pVal; 
                    break;
                case 2: 
                    _materialEval += nVal;
                    _whiteLargePiecesVal += nVal;
                    break;
                case 3: 
                    _materialEval += bVal;
                    _whiteLargePiecesVal += bVal;
                    break;
                case 4: 
                    _materialEval += rVal;
                    _whiteLargePiecesVal += rVal;
                    break;
                case 5: 
                    _materialEval += qVal;
                    _whiteLargePiecesVal += qVal;
                    break;
                case 6: 
                    _materialEval += kVal; 
                    break;

                case 9:  
                    _materialEval -= pVal; 
                    break;
                case 10: 
                    _materialEval -= nVal;
                    _blackLargePiecesVal -= nVal;
                    break;
                case 11: 
                    _materialEval -= bVal;
                    _blackLargePiecesVal -= bVal;
                    break;
                case 12: 
                    _materialEval -= rVal;
                    _blackLargePiecesVal -= rVal;
                    break;
                case 13: 
                    _materialEval -= qVal;
                    _blackLargePiecesVal -= qVal;
                    break;
                case 14: 
                    _materialEval -= kVal; 
                    break;
            }
        }

        return _materialEval; // Return the material score
    }
    private static int  CalculateCenterControl(byte[] _board)
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
    /*private static int  CalculateKingSafety(byte[] _board)
    {
        int _kingSafety = 0;

        // King safety calculation
        _kingSafety -= CountAttacks(_board, wkPos, true);   // True  = our piece is white
        _kingSafety += CountAttacks(_board, bkPos, false);  // False = our piece is black

        // Return the value times the KingSafety coefficient
        return _kingSafety * kingSafetyPriority;
    }*/
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

    /*private static byte CountAttacks(byte[] _board, int _ourPos, bool _weWhite)
    {
        // _ourPos is the position of the piece we want to calculate attacks on
        // _weWhite is the color of our piece

        byte _attacks = 0;   // The amount of possible attacks on our piece

        for (byte i = 0; i < 64; i++)  // Scan the board for possible attacks
        {
            byte _enemy = _board[i];  // Get the enemy piece, although we arent sure its an enemy yet

            if ((_weWhite && _enemy > 8) || (!_weWhite && _enemy < 8)) 
            {   // Dont count attacks from pieces of the same color
                
                // Check if the enemy piece can attack our square

                // i - is the enemy position
                if (CanAttack(_board, _enemy, (byte)(i % 8), (byte)(i / 8), 
                    (byte)(_ourPos % 8), (byte)(_ourPos / 8))) _attacks++;
            }
        }

        return _attacks;
    }*/
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
    /*private static bool CanAttack(byte[] _board, byte _enemy, byte _fromX, byte _fromY, byte _toX, byte _toY)
    {
        int _distX = Math.Abs(_toX - _fromX);      // Important to note that I store the abs value
        int _distY = Math.Abs(_toY - _fromY);      // to reduce the checks needed for the piece attacks (since they are simetrical)

        switch (_enemy)
        {
            case 1:  // White Pawn
            case 9:  // Black pawn
                return (_distX == 1 && _distY == 1); // Pawns attacks diagonaly and only 1 square

            case 2:  // White kNight
            case 10: // Black kNight
                return (_distX == 2 && _distY == 1) || (_distX == 1 && _distY == 2); // kNight attacks

            case 3:  // White Bishop
            case 11: // Black Bishop
                if (_distX == _distY) // if the bishop is in diagonal line with our square
                {
                    return (!CheckForDiagonalBlocking(_board, _fromX, _fromY, _toX, _toY));
                    // Bishops attacks diagonaly
                }
                return false; // return false if we cant reach the piece

            case 4:  // White Rook
            case 12: // Black Rook
                if (_distX == 0 || _distY == 0)  // if the bishop is in diagonal line with our square
                {
                    return !(CheckForVerticalBlocking(_board, (byte)(8 * _fromY + _fromX), (byte)(8 * _toY + _toX)) ||
                     CheckForHorizontalBlocking(_board, (byte)(8 * _fromY + _fromX), (byte)(8 * _toY + _toX)));
                    // Rooks attacks vertically or horizontally
                }
                return false; // return false if we cant reach the piece

            case 5:  // White Queen
            case 13: // Black Queen
                if (_distX == 0 || _distY == 0 || _distX == _distY)  // If the queen is in line with our square
                {
                    return !(CheckForVerticalBlocking(_board, (byte)(8 * _fromY + _fromX), (byte)(8 * _toY + _toX)) ||
                     CheckForDiagonalBlocking(_board, _fromX, _fromY, _toX, _toY) ||
                     CheckForHorizontalBlocking(_board, (byte)(8 * _fromY + _fromX), (byte)(8 * _toY + _toX)));
                    // Queens attack like bishops or rooks
                    // So we check for vertical or diagonal or horizontal blocking
                }
                return false; // return false if we cant reach the piece
                
        }
        return false; // Piece attack logic, true if our square can be attacked, false if it cant
    }*/

    private static bool CheckForDiagonalBlocking(byte[] _board, byte _fromX, byte _fromY, byte _toX, byte _toY)
    {
        for (byte y = Math.Min(_fromY, _toY); y < Math.Max(_fromY, _toY); y++)
        {
            for (byte x = Math.Min(_fromX, _toX); x < Math.Max(_fromX, _toX); x++)
            {
                if (_board[y * 8 + x] != 0) return true; // Check if the path is obstructed
            }
        }
        return false; // return false = no obstruction so the attack is possible
    }

    private static bool CheckForVerticalBlocking(byte[] _board, byte _from, byte _to)
    {
        if (Math.Abs(_from - _to) % 8 != 0) return false; // return false = obstruction not possible if we are in different lanes
        for (byte position = (byte)(Math.Min(_from, _to) + 8); position < Math.Max(_from, _to); position += 8)
        {
            //Write("b[" + position + "]:" + _board[position] + "\n");
            if (_board[position] != 0) return true; // Check if the path is obstructed
        }
        return false; // return false = no obstruction so the attack is possible
    }

    private static bool CheckForHorizontalBlocking(byte[] _board, byte _from, byte _to)
    {
        if (Math.Abs(_from - _to) > 7) return false; // return false = obstruction not possible if we are in different lanes
        for (byte position = (byte)(Math.Min(_from, _to) + 1); position < Math.Max(_from, _to); position++)
        {
            if (_board[position] != 0) return true; // Check if the path is obstructed
        }
        return false; // return false = no obstruction so the attack is possible
    }

    private class Move
    {
        public byte From { get; set; }          // Start position
        public byte To { get; set; }            // Destination position
        public byte Piece { get; set; }         // Moving piece
        public byte CapturedPiece { get; set; } // Captured piece (for display)
    }

    private static List<Move> GenerateAllMoves(byte[] _thisboard, bool _isWhiteTurn)
    {
        List<Move> _moves = new List<Move>();
        for (byte i = 0; i < 64; i++)
        {
            byte _piece = _thisboard[i];
            if (_piece != 0 && (_isWhiteTurn && _piece < 8) || (!_isWhiteTurn && _piece > 8))
            {
                switch (_piece)
                {
                    case 1:  // White pawn
                    case 9:  // Black pawn
                        _moves.AddRange(                                                // Thirdly we save all the legal moves
                            IsMoveLegalNoCheckCriteria(_thisboard,                      // Secondly we kill of the pawn moves where the king is in check
                            GeneratePawnMoves(_thisboard, i, _isWhiteTurn),             // First we generate all pawn moves
                            _isWhiteTurn));                                             // King color for the legal checking
                        break;
                    case 2:  // White knight
                    case 10: // Black knight
                        _moves.AddRange(                                                // Thirdly we save all the legal moves
                            IsMoveLegalNoCheckCriteria(_thisboard,                      // Secondly we kill of the moves where the king is in check
                            GenerateKnightMoves(_thisboard, i, _piece, _isWhiteTurn),   // First we generate all knight moves
                            _isWhiteTurn));                                             // King color for the legal checking
                        break;

                    case 3:  // White bishop
                    case 11: // Black bishop
                        _moves.AddRange(                                                // Thirdly we save all the legal moves
                            IsMoveLegalNoCheckCriteria(_thisboard,                      // Secondly we kill of the moves where the king is in check
                            GenerateBishopMoves(_thisboard, i, _piece, _isWhiteTurn),   // First we generate all bishop moves
                            _isWhiteTurn));                                             // King color for the legal checking
                        break;

                    case 4:  // White rook
                    case 12: // Black rook
                        _moves.AddRange(                                                // Thirdly we save all the legal moves
                            IsMoveLegalNoCheckCriteria(_thisboard,                      // Secondly we kill of the moves where the king is in check
                            GenerateRookMoves(_thisboard, i, _piece, _isWhiteTurn),     // First we generate all rook moves
                            _isWhiteTurn));                                             // King color for the legal checking
                        break;

                    case 5:  // White queen
                    case 13: // Black queen
                        _moves.AddRange(                                                // Thirdly we save all the legal moves
                             IsMoveLegalNoCheckCriteria(_thisboard,                     // Secondly we kill of the moves where the king is in check
                             GenerateQueenMoves(_thisboard, i, _piece, _isWhiteTurn),   // First we generate all queen moves
                             _isWhiteTurn));                                            // King color for the legal checking
                        break;

                    case 6:  // White king
                    case 14: // Black king
                        _moves.AddRange(                                                // Thirdly we save all the legal moves
                             IsMoveLegalNoCheckCriteria(_thisboard,                     // Secondly we kill of the moves where the king is in check
                             GenerateKingMoves(_thisboard, i, _piece, _isWhiteTurn),    // First we generate all king moves
                             _isWhiteTurn));                                            // King color for the legal checking
                        break;
                }
            }
        }
        return _moves;
    }
    private static List<Move> IsMoveLegalNoCheckCriteria(byte[] _board, List<Move> _moves, bool _isKingWhite)
    {
        for (int i = 0; i < _moves.Count; i++)
        {
            if (_moves[i].Piece == 6 || _moves[i].Piece == 14)  // The king has moved
            {
                if (IsKingInCheck(SimulateMove(_board, _moves[i]), _moves[i].To, !_isKingWhite))
                {
                    _moves.RemoveAt(i);  // Clear the moves where the king is still in check
                    i--;                 // Decrease by one so by the end of the loop (i++) we dont hop over a move
                }
            }
            else
            {
                if (IsKingInCheck(SimulateMove(_board, _moves[i]), 64, !_isKingWhite)) // The king hasnt moved
                {
                    _moves.RemoveAt(i);  // Clear the moves where the king is still in check
                    i--;                 // Decrease by one so by the end of the loop (i++) we dont hop over a move
                }
            }
        }

        return _moves;
    }


    private static byte[] SimulateMove(byte[] _oldBoard, Move _move)
    {
        byte[] _newBoard = (byte[])_oldBoard.Clone();
        // Someone explain to me WHY you need to clone this
        // Because if you dont, the original (_oldBoard) gets changed to _newBoard

        if (_move.To == 6)       wkPos = _move.To;  // If the white king moves, update the king position
        else if (_move.To == 14) bkPos = _move.To;  // If the black king moves, update the king position

        _newBoard[_move.To] = _move.Piece;   //
        _newBoard[_move.From] = 0;           // Simulating the move

        return _newBoard;        // Returning the new board with the simulated move
    }

    private static int PositionsAmountTest(byte[] _board, int _depth, bool _isWhiteTurn)
    {
        if (_depth == 0)
            return 1;

        List<Move> _moves = GenerateAllMoves(_board, _isWhiteTurn);

        int _amountOfPositions = 0;

        foreach (Move _move in _moves)
        {
            //PrintParsedBoard(SimulateMove(_board, _move), _move.From, _move.To);
            //Write("\nMove from: " + _move.From + " to " + _move.To + ", Piece: " + _move.Piece);
            _amountOfPositions += PositionsAmountTest(SimulateMove(_board, _move), _depth - 1, !_isWhiteTurn);
        }

        return _amountOfPositions;
    }


    private static bool IsKingInCheck(byte[] _board, byte _kingPos, bool _kingColor)
    {
        if(_kingPos == 64)
        {
            for (int i = 0; i < 63; i++)
            {
                if (_kingColor && _board[i] == 6 || !_kingColor && _board[i] == 14)
                {
                    _kingPos = (byte)i;
                    i += 64;
                }
            }
        }
        int[] _rookOffsets = { -1, -8, 8, 1 };          // Horizontal (-1, 1) and vertical (-8, 8) moves (1st iteration)
        byte _xPos = (byte)(_kingPos % 8);              // Enemy x position

        for (int i = 0; i < 4; i++)
        {
            if ((i < 1 && _xPos > 0) || i == 1 || i == 2 || (i > 2 && _xPos < 7))
            {   // Prevent the check calculation from looping around the board (optimised checking)

                int _newPosition = _kingPos + _rookOffsets[i];   // Choose a new direction for checking
                while (_newPosition >= 0 && _newPosition < 64)
                {
                    byte _target = _board[_newPosition];  // target piece
                    if (_target != 0)
                    {
                        if ((_kingColor && (_target == 12 || _target == 13)) || (!_kingColor && (_target == 4 || _target == 5)))
                        {
                            //Write("\n\tFrom:" + _newPosition + " to:" + _kingPos + " target:" + _target + " kingCol:" + _kingColor);

                            return true; // return that the king is in check
                        }
                        break;  // if a friendly piece is blocking us stop moving further
                    }

                    _xPos = (byte)(_newPosition % 8);                  // New enemy x position

                    if ((i < 1 && _xPos > 0) || i == 1 || i == 2 || (i > 2 && _xPos < 7))
                    {   // Prevent the check calculation from looping around the board (optimised checking)

                        _newPosition += _rookOffsets[i];  // Continue moving in that direction
                    }
                    else _newPosition = 64;
                }
            }
        }

        int[] _knightOffsets = { -10, 6, -17, 15, -15, 17, -6, 10 };  // All possible knight moves
        _xPos = (byte)(_kingPos % 8);                                 // Enemy x position
        for (int i = 0; i < 8; i++)                          // For each square that the knight can attack
        {
            if ((i < 2 && _xPos > 1) || (i > 1 && i < 4 && _xPos > 0) || (i > 3 && i < 6 && _xPos < 7) || (i > 5 && _xPos < 6))
            {   // Prevent the knight from looping around the board (optimised checking)

                int _newPosition = _kingPos + _knightOffsets[i];    //   New position for the knight

                if (_newPosition >= 0 && _newPosition < 64)         //   If the square is on the board
                {
                    byte _target = _board[_newPosition];            //   Target piece

                    if ((_kingColor && _target == 10) || (!_kingColor && _target == 2))  // If we are white and capturing black enemy piece
                        return true;  // return that the king is in check
                }
            }
        }

        int[] _bishopOffsets = { -9, 7, -7, 9 };         // Diagonal moves (1st iteration)
        for (int i = 0; i < 4; i++)                      // For each square that we can move to
        {
            _xPos = (byte)(_kingPos % 8);
            if ((i < 2 && _xPos > 0) || (i > 1 && _xPos < 7))
            {   // Prevent the bishop from looping around the board (optimised checking)

                int _newPosition = _kingPos + _bishopOffsets[i];       // Choose a new direction

                while (_newPosition >= 0 && _newPosition < 64)
                {   // If the square is on the board, and we moved in a single diagonal direction

                    byte _target = _board[_newPosition];                          // Target piece
                    if (_target != 0)                      // If we are moving to an empty square
                    {
                        if (_kingColor)
                        {
                            if (_target == 13 || _target == 11 || 
                                ((_newPosition - _kingPos == 7 || _newPosition - _kingPos == 9) && _target == 9))
                                return true;  // Return that the king is in check
                        }
                        else if(_target == 5 || _target == 3 ||
                                ((_newPosition - _kingPos == -7 || _newPosition - _kingPos == -9) && _target == 1))
                        {
                            return true;      // Return that the king is in check
                        }
                        break;  // if a friendly piece is blocking us stop checking further
                    }

                    _xPos = (byte)(_newPosition % 8);                 // new Diagonal x position

                    if ((i < 2 && _xPos > 0) || (i > 1 && _xPos < 7))
                    {   // Prevent the bishop from looping around the board (optimised checking)

                        _newPosition += _bishopOffsets[i]; // Continue moving diagonally in that direction
                    }
                    else _newPosition = 64;                                    // Break but more efficient
                }
            }
        }


        return false;                 // return that the king is not in check
    }
    


    private static Move AlphaBetaSearch(byte[] _board, int _depth, bool _maximizingPlayer)
    {
        Move _bestMove = null;

        int _maxEval = -999999;
        int _minEval = 999999;

        List<Move> _moves = GenerateAllMoves(_board, _maximizingPlayer);
        if (_maximizingPlayer)
        {
            foreach (Move _move in _moves)
            {
                byte[] _newBoard = SimulateMove(_board, _move);

                int _eval = AlphaBetaEvalSearch(_newBoard, _depth - 1, _maxEval, _minEval, false);
                if (_eval > _maxEval)
                {
                    _maxEval = _eval;
                    _bestMove = _move;
                }
            }
            return _bestMove;
        }
        else
        {
            foreach (Move _move in _moves)
            {
                byte[] _newBoard = SimulateMove(_board, _move);
                int _eval = AlphaBetaEvalSearch(_newBoard, _depth - 1, _maxEval, _minEval, true);
                if (_eval < _minEval)
                {
                    _minEval = _eval;
                    _bestMove = _move;
                }
            }
            return _bestMove;
        }
    }
    private static int  AlphaBetaEvalSearch(byte[] _board, int _depth, int _alpha, int _beta, bool _maximizingPlayer)
    {
        if (_depth < 1)  // return eval result after the search ended
            return AdvEvaluate(_board, _maximizingPlayer);
        

        List<Move> _moves = GenerateAllMoves(_board, _maximizingPlayer);
        if (_maximizingPlayer)
        {
            foreach (Move _move in _moves)
            {
                byte[] _newBoard = SimulateMove(_board, _move);

                int _eval = AlphaBetaEvalSearch(_newBoard, _depth - 1, _alpha, _beta, false);
                if (_eval > _alpha)
                {
                    _alpha = _eval;  // Set new max eval
                }
                if (_beta <= _alpha)
                {
                    gSkippedPositions++;
                    return _alpha;
                }
            }
            return _alpha;
        }
        else
        {
            foreach (Move _move in _moves)
            {
                byte[] _newBoard = SimulateMove(_board, _move);
                int _eval = AlphaBetaEvalSearch(_newBoard, _depth - 1, _alpha, _beta, true);
                if (_eval < _beta)
                {
                    _beta = _eval;
                }
                if (_beta <= _alpha)
                {
                    gSkippedPositions++;
                    return _beta;
                }
            }
            return _beta;
        }
    }

    private static List<Move> GeneratePawnMoves  (byte[] _board, byte _position, bool _isWhite)
    {
        List<Move> _generatedMoves = new List<Move>();  // We will store the generated moves here
        
        int  _direction    = _isWhite ? -8 : 8;         // Movement direction is different for each players pawns
        byte _startRow     = (byte)(_isWhite ? 6 : 1);  // First row for counting the double pawn moves
        byte _promotionRow = (byte)(_isWhite ? 0 : 7);  // Pawn promotion row
        byte _xPos = (byte)(_position % 8);             // Pawn x position

        // Basic pawn move
        int _newPosition = _position + _direction;
        if (_newPosition >= 0 && _newPosition < 64 && _board[_newPosition] == 0)
        {   // Check if we are moving to a valid empty square

            if (_newPosition / 8 == _promotionRow)
            {
                // Pawn promotion  (High priority in the checking list)
                _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wn1 : bn1), CapturedPiece = 0 });  // Promoting to a knight
                _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wb1 : bb1), CapturedPiece = 0 });  // Promoting to a bishop
                _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wr1 : br1), CapturedPiece = 0 });  // Prompting to a rook
                _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wq1 : bq1), CapturedPiece = 0 });  // Promoting to a queen
            }
            else
            {   // Generic move up by one square
                _generatedMoves.Add(new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wp1 : bp1), CapturedPiece = 0 });
            }

            // Double move for pawns in the start position
            if (_position / 8 == _startRow && _board[_newPosition + _direction] == 0)
            {
                _generatedMoves.Add(new Move { From = _position, To = (byte)(_newPosition + _direction), Piece = _board[_position], CapturedPiece = 0 });
            }
        }

        // Diagonal attack to the left (capture to the left)
        if (_xPos > 0)
        {
            byte _attackPosition = (byte)(_isWhite ? _position - 9 : _position + 7);         // Target position
            if (_attackPosition >= 0 && _attackPosition < 64)
            {
                byte _target = _board[_attackPosition];                   // Target piece
                if (_target != 0 && ((_isWhite && _target > 8) || (!_isWhite && _target < 8)))
                {
                    if (_attackPosition / 8 == _promotionRow)
                    {
                        // Check for promotion after attack
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wn1 : bn1), CapturedPiece = 0 });  // Promoting to a knight
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wb1 : bb1), CapturedPiece = 0 });  // Promoting to a bishop
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wr1 : br1), CapturedPiece = 0 });  // Prompting to a rook
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wq1 : bq1), CapturedPiece = 0 });  // Promoting to a queen
                    }
                    else
                    {
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_attackPosition, Piece = (byte)(_isWhite ? wp1 : bp1), CapturedPiece = _target });
                    }
                }
            }
        }
        // Diagonal attack to the right (capture to the right)
        if (_xPos < 7)
        {
            byte _attackPosition = (byte)(_isWhite ? _position - 7 : _position + 9);         // Target position
            if (_attackPosition >= 0 && _attackPosition < 64)
            {
                byte _target = _board[_attackPosition];                   // Target piece
                if (_target != 0 && ((_isWhite && _target > 8) || (!_isWhite && _target < 8)))
                {
                    if (_attackPosition / 8 == _promotionRow)
                    {
                        // Check for promotion after attack
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wn1 : bn1), CapturedPiece = 0 });  // Promoting to a knight
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wb1 : bb1), CapturedPiece = 0 });  // Promoting to a bishop
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wr1 : br1), CapturedPiece = 0 });  // Prompting to a rook
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = (byte)(_isWhite ? wq1 : bq1), CapturedPiece = 0 });  // Promoting to a queen
                    }
                    else
                    {
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_attackPosition, Piece = (byte)(_isWhite ? wp1 : bp1), CapturedPiece = _target });
                    }
                }
            }
        }

        return _generatedMoves;
    }
    private static List<Move> GenerateKnightMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
    {
        List<Move> _moves = new List<Move>();            // We will store the generated moves here
        int[] _knightOffsets = { -10, 6, -17, 15, -15, 17, -6, 10 };  // All possible knight moves
        byte _xPos = (byte)(_position % 8);                               // The knight x position

        for(int i = 0; i < 8; i++)                          // For each square that we can move to
        {
            if ((i < 2 && _xPos > 1) || (i > 1 && i < 4 && _xPos > 0) || (i > 3 && i < 6 && _xPos < 7) || (i > 5 && _xPos < 6))
            {   // Prevent the knight from looping around the board (optimised checking)

                int _newPosition = _position + _knightOffsets[i]; //   New position for the knight

                if (_newPosition >= 0 && _newPosition < 64)       // If the square is on the board
                {   
                    byte _target = _board[_newPosition];          //   Target piece

                    if (_target == 0) // If we are moving to an empty square
                    {   
                        // Save the generated move
                        _moves.Add(new Move { From = _position, To = (byte)_newPosition, Piece = _piece, CapturedPiece = 0 });
                    }
                    else if ((_isWhite && _target > 8) || (!_isWhite && _target < 8))  // If we are white and capturing black enemy piece
                    {
                        // Save the generated move   (Higher priority in the list)
                        _moves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = _piece, CapturedPiece = _target });
                    }
                }
            }
        }
        return _moves;  // Return all possible moves
    }
    private static List<Move> GenerateBishopMoves(byte[] _board, byte _position, byte _piece, bool _isWhite)
    {
        List<Move> _generatedMoves = new List<Move>();   // We will store the generated moves here
        int[] _bishopOffsets = { -9, 7, -7, 9 };         // Diagonal moves (1 iteration)
        byte _xPos;                                      // Bishop x position
        
        for (int i = 0; i < 4; i++)                        // For each square that we can move to
        {
            _xPos = (byte)(_position % 8);
            if ((i < 2 && _xPos > 0) || (i > 1 && _xPos < 7))
            {   // Prevent the bishop from looping around the board (optimised checking)

                int _newPosition = _position + _bishopOffsets[i];       // Choose a new direction

                while (_newPosition >= 0 && _newPosition < 64)
                {   // If the square is on the board, and we moved in a single diagonal direction

                    byte _target = _board[_newPosition];                          // Target piece
                    if (_target == 0)                      // If we are moving to an empty square
                    {
                        // Save the move, captured piece = none (empty = 0)
                        _generatedMoves.Add(new Move { From = _position, To = (byte)_newPosition, Piece = _piece, CapturedPiece = 0 });
                    }
                    else    // If we are blocked by a piece
                    {
                        if ((_isWhite && _target > 8) || (!_isWhite && _target < 8))  // If we are blocked by an enemy piece
                        {
                            // Save the move with capturing an enemy piece  (Higher priority for the capture)
                            _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = _piece, CapturedPiece = _target });
                        }
                        break;  // if a piece is blocking us stop moving further
                    }

                    _xPos = (byte)(_newPosition % 8);                    // Bishop new x position

                    if ((i < 2 && _xPos > 0) || (i > 1 && _xPos < 7))
                    {   // Prevent the bishop from looping around the board (optimised checking)

                        _newPosition += _bishopOffsets[i]; // Continue moving diagonally in that direction
                    }
                    else _newPosition = 64;                                    // Break but more efficient
                }
            }
        }

        return _generatedMoves;  // return the generated moves
    }
    private static List<Move> GenerateRookMoves  (byte[] _board, byte _position, byte _piece, bool _isWhite)
    {
        List<Move> _generatedMoves = new List<Move>();  // We will store the generated moves here
        int[] _rookOffsets = { -1, -8, 8, 1 };          // Horizontal (-1, 1) and vertical (-8, 8) rook moves (1 iteration)
        byte _xPos = (byte)(_position % 8);             // Rook x position

        for (int i = 0; i < 4; i++)
        {
            if ((i < 1 && _xPos > 0) || i == 1 || i == 2 || (i > 2 && _xPos < 7))
            {   // Prevent the rook from looping around the board (optimised checking)

                int _newPosition = _position + _rookOffsets[i];   // Choose a new diagonal direction for moving
                while (_newPosition >= 0 && _newPosition < 64)
                {
                    byte _target = _board[_newPosition];  // Target piece
                    if (_target == 0)                     // If we are moving to an empty square
                    {
                        // Save the move, captured piece = none (empty = 0)
                        _generatedMoves.Add(new Move { From = _position, To = (byte)_newPosition, Piece = _piece, CapturedPiece = 0 });
                    }
                    else  // If we are blocked by a piece
                    {
                        if ((_isWhite && _target > 8) || (!_isWhite && _target < 8)) // If we are blocked by an enemy piece
                        {
                            // Save the move with capturing an enemy piece
                            _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = _piece, CapturedPiece = _target });
                        }
                        break; // if a piece is blocking us stop moving further
                    }

                    _xPos = (byte)(_newPosition % 8);                   // Rook new x position

                    if ((i < 1 && _xPos > 0) || i == 1 || i == 2 || (i > 2 && _xPos < 7))
                    {   // Prevent the rook from looping around the board (optimised checking)

                        _newPosition += _rookOffsets[i];  // Continue moving in that direction
                    }
                    else _newPosition = 64;
                }
            }
        }
        return _generatedMoves;  // return the generated moves
    }
    private static List<Move> GenerateQueenMoves (byte[] _board, byte _position, byte _piece, bool _isWhite)
    {
        List<Move> _generatedMoves = new List<Move>();

        // The queen is literally just a rook + bishop
        // So we calculate as if the piece is a bishop or a rook and add all moves together
        _generatedMoves = GenerateBishopMoves(_board, _position, _piece, _isWhite);          // Diagonal
        _generatedMoves.AddRange(GenerateRookMoves  (_board, _position, _piece, _isWhite));  // Horizontal and vertical

        return _generatedMoves;  // Return generated moves
    }
    private static List<Move> GenerateKingMoves  (byte[] _board, byte _position, byte _piece, bool _isWhite)
    {
        List<Move> _generatedMoves = new List<Move>();        // We will store the generated moves here
        int[] _kingOffsets = { -9, -1, 7, -8, 8, -7, 1, 9 };  // All posible king moves
        byte _xPos = (byte)(_position % 8);                   // King x position

        for (int i = 0; i < 8; i++)                 // For each square that we can move to
        {
            if ((i < 3 && _xPos > 0) || (i > 4 && i < 8 && _xPos < 7) || i == 3 || i == 4)
            {   // Prevent the king from looping around the board     (optimised checking)

                int _newPosition = _position + _kingOffsets[i];
                if (_newPosition >= 0 && _newPosition < 64)
                {   // If the square on the board, and we moved in a single diagonal direction
                    // Additional check to prevent moving out of range (useless ? I commented it temporary, probably remove it)


                    byte _target = _board[_newPosition];  // Target piece
                    if (_target == 0)
                    {   // If we are moving to an empty square, or we are capturing a piece of the opposite color

                        // Save the generated move
                        _generatedMoves.Add(new Move { From = _position, To = (byte)_newPosition, Piece = _piece, CapturedPiece = _target });
                    }
                    else if (_isWhite && _target > 8)
                    {
                        // Save the generated move   (Higher priority)
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = _piece, CapturedPiece = _target });
                    }
                    else if(!_isWhite && _target < 8)
                    {
                        // Save the generated move   (Higher priority)
                        _generatedMoves.Insert(0, new Move { From = _position, To = (byte)_newPosition, Piece = _piece, CapturedPiece = _target });
                    }
                }
            }
        }
        return _generatedMoves;  // Return the generated moves;
    }


    private static void ApplyMove(byte[] _board, Move move)
    {
        if (move == null) gCheckMate++;
        else
        {
            if (_board[move.To] != 0)      // If the moved to square is not empty
            {                              // Print  which piece was captured
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

                    case wk1:
                        Write("Opps, seems like the white king was captured");
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

                    case bk1:
                        Write("Opps, seems like the black king was captured");
                        break;
                }
                Write(" was taken");
                gPieceGotEaten = true;
            }
            _board[move.To] = move.Piece;  // Move  the piece to the new square
            _board[move.From] = 0;         // Clear the previous square

            // If the moved piece was the king
            if (move.Piece == 6) wkPos = move.To;  // Save the new white king position
            else if (move.Piece == 14) bkPos = move.To;  // Save the new black king position
        }
    }
    private static bool GetTurn()
    {
        string _userInput = "";

        Write("\n\tWhose turn? (W / Y / YES / 1  = player white turn): ");
        _userInput = ReadLine().Trim().ToLower();

        if (_userInput == "w" || _userInput == "y" || _userInput == "yes" || _userInput == "1") 
            return true;  // true  = white's turn
        return false;     // false = black's turn
    }
    private static int  GetDepth()
    {
        string _userInput;
        int _depth = -1;
        Write("\tEnter the depth for the algorithm search (only from 1 to 8): ");
        while (_depth < 1 || _depth > 8)
        {
            _userInput = ReadLine();
            Clear();
            if (!int.TryParse(_userInput, out _depth)) Write("\tInvalid input, please try again: ");
            else if (_depth < 1 || _depth > 8) Write("\tOut of bounds, please enter a valid number from the interval: ");
        }
        Write("\n");
        return _depth;
    }

    private static bool GetBoardDisplayType()
    {
        Write("\n\tDisplay chess unicode pieces? (Yes/Y/1/Да = display unicode, else = numbers): ");
        string _userInput = ReadLine().ToLower().Replace(" ", "");
        if (_userInput == "yes" || _userInput == "y" || _userInput == "1" || _userInput == "да") return true;
        return false;
    }

    private static bool TryParseUserMove(byte[] _board, string _userInput)
    {
        if (_userInput.Length == 4)
        {
            byte _from = 0;
            byte _to = 0;

            switch (_userInput[0])
            {
                case 'a':              break;
                case 'b': _from += 1;  break;
                case 'c': _from += 2;  break;
                case 'd': _from += 3;  break;
                case 'e': _from += 4;  break;
                case 'f': _from += 5;  break;
                case 'g': _from += 6;  break;
                case 'h': _from += 7;  break;

                default: return false;
            }
            switch (_userInput[1])
            {
                case '1': _from += 56; break;
                case '2': _from += 48; break;
                case '3': _from += 40; break;
                case '4': _from += 32; break;
                case '5': _from += 24; break;
                case '6': _from += 16; break;
                case '7': _from += 8;  break;
                case '8': break;

                default: return false;
            }

            switch (_userInput[2])
            {
                case 'a':            break;
                case 'b': _to += 1;  break;
                case 'c': _to += 2;  break;
                case 'd': _to += 3;  break;
                case 'e': _to += 4;  break;
                case 'f': _to += 5;  break;
                case 'g': _to += 6;  break;
                case 'h': _to += 7;  break;

                default: return false;
            }
            switch (_userInput[3])
            {
                case '1': _to += 56; break;
                case '2': _to += 48; break;
                case '3': _to += 40; break;
                case '4': _to += 32; break;
                case '5': _to += 24; break;
                case '6': _to += 16; break;
                case '7': _to += 8;  break;
                case '8':            break;

                default: return false;
            }

            Move _userMove = new Move { From = _from, To = _to, Piece = _board[_from], CapturedPiece = _board[_to] };
            ApplyMove(_board, _userMove);
            return true;
        }
        else return false;
    }
}
