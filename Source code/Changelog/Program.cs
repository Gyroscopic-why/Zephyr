using System;
using System.Diagnostics;
using System.Collections.Generic;

using static System.Console;


using static Zephyr.Configs;
using static Zephyr.MoveGeneration;
using static Zephyr.MoveValidation;
using static Zephyr.BoardMoveLogic;



class Program
{
    static void Main()
    {
        OutputEncoding = System.Text.Encoding.Unicode;
        Title = "Zephyr engine Theta3";
        

        Stopwatch timeCounter;
        Move makeBestMove = null;
        string continueGame = "";


        while (continueGame != "exit")
        {
            gBoardState = 0;                                    //  Reset game
            int depth = GetDepth();                             //  Get the depth for the alpha-beta search
            bool boardDisplayType = GetBoardDisplayType();      //  Set the board display type
            GetEncodedBoard(false);                             //  Get the board position (classic storing)
            whiteTurn = GetTurn();                              //  Ask whose turn it is (true = white)

            Clear();                                            //  Clear the console
            DisplayBoard(mainBoard, boardDisplayType);          //  Print the board position

            int eval = AdvEvaluate(mainBoard, true, true);      //  Evaluate the start board position
            Write($"\n\t\t\t\tCurrent eval: {eval}\n\n\n\t");   //  Write the start board position eval result
            Write("White king in check: " + IsKingInCheck(mainBoard, wkPos, true) + ", wkPos: " + wkPos);       // Print info
            Write("\n\tBlack king in check: " + IsKingInCheck(mainBoard, bkPos, false) + ", bkPos: " + bkPos);  //
            Write("\n\n\tBegin game?  (press ENTER): ");


            continueGame = ReadLine().Trim().ToLower();
            // Wait for when user is ready

            if (TryParseUserMove(mainBoard, continueGame))     // Check for a move input from the user
            {
                continueGame = "";

                whiteTurn = !whiteTurn;
                // Change the player turn if user move was valid=
            }


            while (continueGame == "")
            {
                gSkippedPositions = 0;
                gEvaluatedPositions = 0;
                //  Reset search parameters


                timeCounter = Stopwatch.StartNew();
                if (Math.Abs(gBoardState) != 1 && Math.Abs(gBoardState) < 125)
                {   //  If the game hasnt ended yet


                    gBoardState = 0;
                    //  Reset board state for the search


                    for (int i = 1; i <= depth && gBoardState == 0; i++)
                    { 
                        makeBestMove = StartABSearch(mainBoard, i, whiteTurn);
                        //  Start the search


                        Write("\n\t\tTotal evaluated position: " + gEvaluatedPositions);  
                        if(makeBestMove != null) Write("\n\t\tDepth: " + i + ", move from : " + makeBestMove.From + ", move to: " + makeBestMove.To);


                        if (gBoardState != 0)
                        {
                            if (makeBestMove == null || (Math.Abs(gBoardState) - 1 < 0)) Write("\n\n\t\t[i]  - CHECKMATE!  No moves possible.");
                            else
                            {
                                if      (gBoardState ==  127) Write("\n\n\t\t[i]  - Stalemate!  No moves possible.");
                                else if (gBoardState == -127) Write("\n\n\t\t[i]  - Stalemate!  No moves possible.");
                                else if (gBoardState ==  126) Write("\n\n\t\t[i]  - Draw! Checkmates are impossible");
                                else if (gBoardState == -126) Write("\n\n\t\t[i]  - Draw! By 3F repetition");
                                else if (gBoardState ==  125) Write("\n\n\t\t[i]  - Draw! By 50 move no capture move rule");
                                else                          Write("\n\n\t\t[i]  - Found mate in " + (Math.Abs(gBoardState) - 1) + " moves.");
                            }
                        }

                        //Write("\n\n\t\tDepth: " + i + ", result: " + PosAmountTest(mainBoard, i, whiteTurn) + ", time elapsed: " + timeCounter.ElapsedMilliseconds + " ms");
                    }

                    ReadKey();
                    //  Wait for user confirmation

                    ApplyMove(mainBoard, makeBestMove);  //  Apply the best found move
                    whiteTurn = !whiteTurn;              //  Change the player turn
                    Clear();                             //  Prepare to reset board info
                }

                //  Print board state if the game has or is about to end
                if(gBoardState != 0)
                {
                    Clear();
                    if (gBoardState == 127)                 Write("\t\t[i]  - Stalemate!  No moves possible.");
                    else if (gBoardState == -127)           Write("\t\t[i]  - Stalemate!  No moves possible.");
                    else if (gBoardState ==  126)           Write("\t\t[i]  - Draw! Checkmates are impossible");
                    else if (gBoardState == -126)           Write("\t\t[i]  - Draw! By 3F repetition");
                    else if (gBoardState ==  125)           Write("\t\t[i]  - Draw! By 50 move no capture move rule");
                    
                    else if (gBoardState == 1)              Write("\t\t[i]  - CHECKMATE!  Black has won the game.");
                    else if (gBoardState == -1)             Write("\t\t[i]  - CHECKMATE!  White has won the game.");
                    else if (Math.Abs(gBoardState) - 2 > 0) Write("\t\t[i]  - Found mate in " + (Math.Abs(gBoardState) - 2) + " moves.");
                    else                                    Write("\t\t[i]  - Found mate in: 0 moves.");
                }


                timeCounter.Stop();
                if (makeBestMove != null) DisplayBoard(mainBoard, boardDisplayType, makeBestMove.From, makeBestMove.To); // Print the new board
                else DisplayBoard(mainBoard, boardDisplayType);


                eval = AdvEvaluate(mainBoard, whiteTurn, true);    //  Evaluate the new position
                Write($"\n\t\t\t\tCurrent eval: {eval}\n\n\n\t");  //  Write the eval position value
                

                Write("\n\tSearched: " + gEvaluatedPositions + ", AB skipped: " + gSkippedPositions + ", time elapsed: " + timeCounter.ElapsedMilliseconds + " ms\n\t");
                Write("White king in check: " + IsKingInCheck(mainBoard, wkPos, true) + ", wkPos: " + wkPos);       // Print info
                Write("\n\tBlack king in check: " + IsKingInCheck(mainBoard, bkPos, false) + ", bkPos: " + bkPos);  //
                Write("\n\n\tContinue?  (press ENTER): ");

                continueGame = ReadLine().Trim().ToLower();        //  Wait for when user is ready

                if (TryParseUserMove(mainBoard, continueGame))     //  Check for a move input from the user
                {
                    whiteTurn = !whiteTurn;                        //  Change the player turn
                    continueGame = "";                             //  Reset the continue game string
                }
            }
            EncodeBoard(mainBoard);                                //  Print the new board code
            if (continueGame != "exit")
            {
                ForegroundColor = ConsoleColor.Green;              //  Inform about the new game
                Write("\tNew game settings: ");                    //
                ForegroundColor = ConsoleColor.White;              //
            }
        }
        ReadKey();                                                 //  Exit the program
    }


    private static bool GetEncodedBoard(bool type) // Check the type of board
    {                                               // true = optimised board, false = classic board
        bool validEncoding = false, reset;
        string userInput, encodedBoard;
        while (!validEncoding)
        {
            encodedBoard = "";
            Write("\n\tEnter the board (64 characters): (SP: 0 / rnbqkbnrpppppppp32PPPPPPPPRNBQKBNR)\n\t");
            reset = false;

            while (encodedBoard.Length < 64 && !validEncoding && !reset)
            {
                userInput = ReadLine().Replace(" ", "");
                encodedBoard += userInput;

                if (userInput == "0") encodedBoard = "rnbqkbnrpppppppp32PPPPPPPPRNBQKBNR";
                if (encodedBoard.Length <= 64 && encodedBoard.Length > 0)
                {
                    if (TryParseClassicBoard(ref encodedBoard)) validEncoding = true;
                    else if (encodedBoard[encodedBoard.Length - 1] == '#')
                    {
                        Clear();
                        Write("\t[!]  - Error while parsing board: to many characters\n");
                        reset = true;
                    }
                    else if (encodedBoard[encodedBoard.Length - 1] == '!')
                    {
                        Clear();
                        Write("\t[!]  - Error while parsing board: unknown character\n");
                        reset = true;
                    }
                }
                
                Write("\t");
            }
            if (encodedBoard.Length == 64)
            {
                if (type)
                {
                    //if (TryParseOptimisedBoard(encodedBoard, optimisedBoard)) validEncoding = true;
                    //else Write("Error while parsing board: unknown character");
                }

                else
                {
                    if (TryParseClassicBoard(ref encodedBoard)) validEncoding = true;
                    else
                    {
                        Clear();
                        Write("\t[!]  - Error while parsing board: unknown character\n");
                    }
                }
            }
        }


        return validEncoding;
    }

    /*private static bool TryParseOptimisedBoard(string encoded, byte[] board)
    {
        byte tempBuffer;
        for (byte i = 0; i < 32; i++)
        {
            tempBuffer = ConvertBoardToBytes(encoded[i], true);  // Parse the even    square
            if (tempBuffer == decodingError) return false;        // Check for illegal characters
            board[i] = tempBuffer;                               // Save  the parsed  piece

            if (board[i] == 6)   // If the parsed piece is the white king
            {
                wkPos = i;        // Save the white king position
            }
            if (board[i] == 14)  // If the parsed piece is the black king
            {
                bkPos = i;        // Save the black king position
            }


            tempBuffer = ConvertBoardToBytes(encoded[i], false); // Parse the uneven  square
            if (tempBuffer == decodingError) return false; // Check for illegal characters
            board[i] += tempBuffer;               // Save  the parsed  piece

            if (board[i] == 6)   // If the parsed piece is the white king
            {
                wkPos = i;        // Save the white king position
            }
            if (board[i] == 14)  // If the parsed piece is the black king
            {
                bkPos = i;        // Save the black king position
            }
        }
        return true;
    } // Needs an update*/
    private static bool TryParseClassicBoard(ref string encoded)
    {
        byte fenOffset = 0;
        for (byte i = 0; i < encoded.Length; i++)
        {
            if (i + fenOffset < 64)
            {
                mainBoard[i + fenOffset] = ConvertBoardToBytes(encoded[i], true); // Parse the board piece by piece

                if (mainBoard[i + fenOffset] == decodingError) // If encountored an unknown character
                {
                    string temp = "";
                    if (i < encoded.Length - 1)      // Prevent index out of range 
                    {
                        temp += encoded[i];     // Temporary string to stop the strange parsing errors:
                        temp += encoded[i + 1]; // (if you only add .ToString() to the byte.TryParse later)
                    }

                    if (byte.TryParse(temp, out byte freeSpaceForFen))
                    {   // Check for custom short fen - fen number confirmed

                        for (byte j = i; j < freeSpaceForFen + i; j++)
                        {
                            if (i + fenOffset < 64)
                            {
                                mainBoard[i + fenOffset] = emptySquare;  // Fill empty squares
                                fenOffset++;                             // Increase offset for board parsing
                            }
                            else
                            {
                                j += freeSpaceForFen;
                                encoded += "#";
                            }
                        }
                        fenOffset -= 2;  // Account for the offset of the two chars that was converted to bytes
                        i++;
                    }
                    else if (byte.TryParse(encoded[i].ToString(), out freeSpaceForFen))
                    {   // Check for Fen char - Fen char confirmed

                        for (int j = i; j < freeSpaceForFen + i; j++)
                        {
                            if (i + fenOffset < 64)
                            {
                                mainBoard[i + fenOffset] = emptySquare;  // Fill empty squares
                                fenOffset++;                             // Increase offset for board parsing
                            }
                            else
                            {
                                j += freeSpaceForFen;
                                encoded += "#";
                            }
                        }
                        fenOffset--;
                    }
                    else
                    {
                        encoded += "!";
                        return false;       // Return parsing error if the charcter is invalid
                    }
                }

                if (mainBoard[i + fenOffset] == 6)   // If the parsed piece is the white king
                {
                    wkPos = (byte)(i + fenOffset);  // Save the white king position
                }
                if (mainBoard[i + fenOffset] == 14)  // If the parsed piece is the black king
                {
                    bkPos = (byte)(i + fenOffset);  // Save the black king position
                }
            }
            else
            {
                i += 128;
                encoded += "#";
            }
        }
        if (encoded.Length + fenOffset != 64) return false; // If the full board isnt filled return error
        return true;
    }

    private static byte ConvertBoardToBytes(char encodedPiece, bool parsingPos)
    {
        if (parsingPos) // Parsing pos means the color of the squares, 
        {                // it is used to save memory used to store the board up to 2 times 
            switch (encodedPiece)                                       // (only 32 bytes)
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
                case ' ':
                case '=': return emptySquare;
                default: return decodingError;
            }
        }
        else
        {
            switch (encodedPiece)
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
                case ' ':
                case '=': return emptySquare;
                default: return decodingError;
            }
        }
    } // Transform the board encoding from chars to bytes
    private static char ConvertBoardToChars(byte decodedPiece, bool parsingPos)
    {
        if (parsingPos) // Parsing pos means the color of the squares, 
        {                // it is used to save memory used to store the board up to 2 times 
            switch (decodedPiece)                                       // (only 32 bytes)
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
            switch (decodedPiece)
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


    private static void DisplayBoard(byte[] board, bool displayType, byte moveFrom = 64, byte moveTo = 64)
    {
        Write("\n\n");
        if (!displayType) Write("\n\n\n");
        Write("\t\t\t\tParsed board (in bytes): " + board.Length + "\n\n\n\t\t\t"); 
        if (board.Length == 64)
        {
            if (!displayType)  // If we are displaying the board in bytes
            {
                Write("8 | ");
                for (int i = 0; i < 64; i++)
                {
                    if (board[i] == 0) ForegroundColor = ConsoleColor.DarkGray;                             // Highlight empty squares
                    else if (board[i] == 1 || board[i] == 9) ForegroundColor = ConsoleColor.White;         // Highlight pawns
                    else if (board[i] == 6 || board[i] == 14) ForegroundColor = ConsoleColor.Red;          // Highlight kings
                    else if (board[i] == 5 || board[i] == 13) ForegroundColor = ConsoleColor.DarkMagenta;  // Highlight queens
                    else if (board[i] < 8) ForegroundColor = ConsoleColor.DarkGreen;                        // Highlight every other white's pieces
                    else ForegroundColor = ConsoleColor.DarkBlue;                                            // Highlight every other black's pieces

                    // Highlight the square were there previously was a piece
                    if (i == moveFrom) ForegroundColor = ConsoleColor.DarkRed;
                    else if (i == moveTo)
                        ForegroundColor = ConsoleColor.Cyan;  // Highlight the square we the last piece moved

                    if (board[i] > 9) Write(" " + board[i] + "  ");  // Print alligned grid
                    else Write(" 0" + board[i] + "  ");               //
                    if (i % 8 == 7 && i < 63)
                    {
                        //  Reset console colors
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.Gray;

                        //  Move to new line and write the line ID
                        Write("\n\t\t\t  |\n\t\t\t" + (64 - i) / 8 + " | ");
                    }
                }

                //  Reset console colors
                ForegroundColor = ConsoleColor.White;
                BackgroundColor = ConsoleColor.Black;

                //  Write column ID
                Write("\n\t\t\t  +---------------------------------------");
                Write("\n\t\t\t      A    B    C    D    E    F    G    H");
            }
            else
            {
                Write("\t8 | ");
                for (int i = 0; i < 64; i++)
                {
                    // Highlight empty squares
                    if (board[i] == 0) ForegroundColor = ConsoleColor.DarkGray;

                    // Highlight pawns
                    else if (board[i] == 1 || board[i] == 9) ForegroundColor = ConsoleColor.White;

                    // Highlight whiteking
                    else if (board[i] == 6) ForegroundColor = ConsoleColor.Red;

                    // Highlight black king
                    else if (board[i] == 14) ForegroundColor = ConsoleColor.DarkMagenta;

                    // Highlight white queen
                    else if (board[i] == 5) ForegroundColor = ConsoleColor.Red;

                    // Highlight black queen
                    else if (board[i] == 13) ForegroundColor = ConsoleColor.DarkMagenta;

                    // Highlight every other white's pieces
                    else if (board[i] < 8) ForegroundColor = ConsoleColor.DarkGreen;

                    // Highlight every other black's pieces
                    else ForegroundColor = ConsoleColor.DarkBlue;


                    if ((i + i / 8) % 2 == 0) BackgroundColor = ConsoleColor.DarkGray;
                    else BackgroundColor = ConsoleColor.Black;


                    // Highlight the square were there previously was a piece
                    if (i == moveFrom) BackgroundColor = ConsoleColor.DarkRed;
                    else if (i == moveTo)
                        BackgroundColor = ConsoleColor.Cyan;  // Highlight the square we the last piece moved

                    switch (board[i])
                    {
                        case 0: Write("   "); break;
                        case 1: Write(" ♙ "); break;
                        case 2: Write(" ♞ "); break;
                        case 3: Write(" ♝ "); break;
                        case 4: Write(" ♜ "); break;
                        case 5: Write(" ♛ "); break;
                        case 6: Write(" ♚ "); break;

                        case 9: Write(" ♟ "); break;
                        case 10: Write(" ♘ "); break;
                        case 11: Write(" ♗ "); break;
                        case 12: Write(" ♖ "); break;
                        case 13: Write(" ♕ "); break;
                        case 14: Write(" ♔ "); break;
                    }
                    if (i % 8 == 7 && i < 63)
                    {
                        //  Reset console colors
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.Gray;

                        //  Move to the new line and write the column ID
                        Write("\n\t\t\t\t" + (63 - i) / 8 + " | ");
                    }
                }

                //  Reset console colors
                ForegroundColor = ConsoleColor.White;
                BackgroundColor = ConsoleColor.Black;

                //  Write column ID
                Write("\n\t\t\t\t  +-------------------------");
                Write("\n\t\t\t\t     A  B  C  D  E  F  G  H ");
            }
            
            //  Final offset for later info
            Write("\n\n");
        }
        else
        {
            for (int i = 0; i < 32; i++)
            {
                // Print the optimised parsed board here
            }
        }
    }
    
    
    private static void EncodeBoard(byte[] board)
    {
        string fullEncoded = "", fen = "";
        byte emptySquares = 0;

        if (board.Length == 64)
        {
            for (byte i = 0; i < 64; i++)
            {
                fullEncoded += ConvertBoardToChars(board[i], true);
                if (fullEncoded[i] != '+')
                {
                    if (emptySquares != 0)
                    {
                        fen += emptySquares;
                        emptySquares = 0;
                    }
                    fen += fullEncoded[i];
                }
                else emptySquares++; // Count empty squares for the fen

            }   // Encode the position char by char
            if (emptySquares != 0)
            {
                fen += emptySquares;
                emptySquares = 0;
            }
        }
        else
        {
            // Do the encoding for the optimised board
        }
        fullEncoded = fullEncoded.Replace("++++++++", "+++++++/");
        PrintEncodedBoard(fullEncoded, fen);
    }
    private static void PrintEncodedBoard(string encodedBoard, string boardFen)
    {
        Write("\n\tEncoded board code: " + encodedBoard);
        Write("\n\tCustom fen board code: " + boardFen + "\n\n\n");
    }


    private static int  AdvEvaluate(byte[] board, bool isWhite, bool writeInfo = false)
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
        -10, -6, -5, -4, -4, -5, -6, -10,
         -6, -4, -3, -2, -2, -3, -4,  -6,
         -5, -3, -2, -1, -1, -2, -3,  -5,
         -4, -2, -1,  0,  0, -1, -2,  -4,
         -4, -2, -1,  0,  0, -1, -2,  -4,
         -5, -3, -2, -1, -1, -2, -3,  -5,
         -6, -4, -3, -2, -2, -3, -4,  -6,
        -10, -6, -5, -4, -4, -5, -6,  -10
    };                //
        //--------------------------------------//

        int blackMajorPieces = 0;
        int whiteMajorPieces = 0;
        int materialVal = CalculateMaterial(board, ref whiteMajorPieces, ref blackMajorPieces);

        int positionalVal = 0;  // Positional value will be stored here
        int enemyInCheckVal = 0;

        // Enemy king helps attack
        //
        // We punish the player in the endgame if:
        // He is winning
        // His king doesnt help to checkmate his opponent
        int endGameKingAssistVal = 0;


        if (Math.Max(whiteMajorPieces, blackMajorPieces) > 1700)  // Calculate position values for start and middle game
        {
            for (byte i = 0; i < 64; i++)
            {
                byte piece = board[i];
                if (piece == 0) continue;
                switch (piece)
                {
                    case 1: // White Pawn
                        positionalVal += STARTpawnTable[i];
                        break;

                    case 2: // White kNight
                        positionalVal += STARTknightTable[i];
                        break;

                    case 3: // White Bishop
                        positionalVal += STARTbishopTable[i];
                        break;

                    case 4: // White Rook
                        positionalVal += STARTrookTable[i];
                        break;

                    case 5: // White Queen
                        positionalVal += STARTqueenTable[i];
                        break;

                    case 6: // White King
                        positionalVal += STARTkingTable[i];
                        wkPos = i;
                        break;



                    case 9: // Black Pawn
                        positionalVal -= STARTpawnTable[63 - i];
                        break;

                    case 10: // Black kNight
                        positionalVal -= STARTknightTable[63 - i];
                        break;

                    case 11: // BlackBishop
                        positionalVal -= STARTbishopTable[63 - i];
                        break;

                    case 12: // Black Rook
                        positionalVal -= STARTrookTable[63 - i];
                        break;

                    case 13: // Black Queen
                        positionalVal -= STARTqueenTable[63 - i];
                        break;

                    case 14: // Black King
                        positionalVal -= STARTkingTable[63 - i];
                        bkPos = i;
                        break;
                }
                positionalVal *= piecePositionPriority;
            }    // Count the positional value for each piece on the board
        }
        else  // Calculate position value for the end game (with different piece table scores)
        {
            for (byte i = 0; i < 64; i++)
            {
                byte piece = board[i];
                if (piece == 0) continue;
                switch (piece)
                {
                    case 1: // White Pawn
                        positionalVal += ENDpawnTable[i];
                        break;

                    case 2: // White kNight
                        positionalVal += ENDknightTable[i];
                        break;

                    case 3: // White Bishop
                        positionalVal += ENDbishopTable[i];
                        break;

                    case 4: // White Rook
                        positionalVal += ENDrookTable[i];
                        break;

                    case 5: // White Queen
                        positionalVal += ENDqueenTable[i];
                        break;

                    case 6: // White King
                        positionalVal += ENDkingTable[i];
                        wkPos = i;
                        break;



                    case 9: // Black Pawn
                        positionalVal -= ENDpawnTable[63 - i];
                        break;

                    case 10: // Black kNight
                        positionalVal -= ENDknightTable[63 - i];
                        break;

                    case 11: // BlackBishop
                        positionalVal -= ENDbishopTable[63 - i];
                        break;

                    case 12: // Black Rook
                        positionalVal -= ENDrookTable[63 - i];
                        break;

                    case 13: // Black Queen
                        positionalVal -= ENDqueenTable[63 - i];
                        break;

                    case 14: // Black King
                        positionalVal -= ENDkingTable[63 - i];
                        bkPos = i;
                        break;
                }
                positionalVal *= piecePositionPriority;
            }    // Count the positional value for each piece on the board

            //  Add bonus for enemy king in corner
            


            //  Add bonus for the king assisting in the enemy checkmate
            if (whiteMajorPieces > blackMajorPieces)
            {
                endGameKingAssistVal -= Math.Abs(wkPos / 8 - bkPos / 8) + Math.Abs(wkPos % 8 - bkPos % 8);
                endGameKingAssistVal *= kingAggressionInEndgame;

                if (isWhite) positionalVal += ENDkingTable[bkPos] * losingPlayerInCorner;
                else positionalVal          -= ENDkingTable[bkPos] * losingPlayerInCorner;
            }
            else
            {
                endGameKingAssistVal += Math.Abs(wkPos / 8 - bkPos / 8) + Math.Abs(wkPos % 8 - bkPos % 8);
                endGameKingAssistVal *= kingAggressionInEndgame;

                if (isWhite) positionalVal -= ENDkingTable[wkPos] * losingPlayerInCorner;
                else positionalVal          += ENDkingTable[wkPos] * losingPlayerInCorner;
            }
        }

        if (isWhite) if (IsKingInCheck(board, bkPos, false)) enemyInCheckVal += enemyInCheckPriority;  
        // Enemy is in check bonus

        else if (IsKingInCheck(board, wkPos, true)) enemyInCheckVal -= enemyInCheckPriority;
        // Enemy is in check bonus


        // Add capture bonus (plans for later)
        //evaluation += CalculateCaptureBonus(board, isWhite);


        // King safety bonus (simplified)
        int kingSafetyVal = /*CalculateKingSafety(board);*/ 0;

        // Bonus for capitalising on open files
        int openFileVal = CalculateOpenFiles(board, isWhite);

        // Bonus for center control (simplified)
        int centerControlVal = CalculateCenterControl(board);

        // Bonus for the pawn structure (simplified)
        int pawnStructureVal = CalculatePawnStructure(board);


        // CALCULATE FINAL EVALUATION
        int evaluation = 
            materialVal + 
            positionalVal +
            endGameKingAssistVal +
            openFileVal + 
            centerControlVal +
            pawnStructureVal +
            enemyInCheckVal;           // Add sum value to the eval result

        if (writeInfo)  // Write extra info
        {
            Write("\t\t\tMaterial: " + materialVal + " + positional: " + positionalVal + " + eg king assist: " + endGameKingAssistVal);
            Write("\n\t\t\t + open file: " + openFileVal + " + king safety: " + kingSafetyVal + " + enemy in check: " + enemyInCheckVal + " +\n\t\t\t");
            Write("center control: " + centerControlVal + " + pawn structure: " + pawnStructureVal);
        }

        return evaluation;
    }
    private static int  CalculateOpenFiles(byte[] board, bool isWhite)
    {
        int totalBonus = 0;
        for (int x = 0; x < 8; x++)
        {
            bool hasPawn = false;   // Later will be used to assigned punishment

            // Check for pawns in the file, to determine if its open or not
            for (int y = 0; y < 8; y++)
            {
                byte piece = board[y * 8 + x];

                if ((isWhite && piece == 1) || (!isWhite && piece == 9))  // Found a pawn of our color, so the file isnt open
                {
                    hasPawn = true;
                    break;
                }
            }

            // If the file doent contain pawns, its an open file
            if (!hasPawn)
            {
                // Find the rooks and queens in the open file
                for (int y = 0; y < 8; y++)
                {
                    byte piece = board[y * 8 + x];

                    switch(piece)
                    {
                        case 4:
                            totalBonus += rookOpenFileBonus;
                            hasPawn = true; // Reusing an old variable to stop the punishment
                            break;
                        case 5:
                            totalBonus += queenOpenFileBonus;
                            hasPawn = true; // Reusing an old variable to stop the punishment
                            break;

                        case 12:
                            totalBonus -= rookOpenFileBonus;
                            hasPawn = true; // Reusing an old variable to stop the punishment
                            break;
                        case 13:
                            totalBonus -= queenOpenFileBonus;
                            hasPawn = true; // Reusing an old variable to stop the punishment
                            break;
                    }
                }
                if (!hasPawn) totalBonus += isWhite ? openFileNotUsedPunishment : -openFileNotUsedPunishment;
                // Assign a punishment if an open file is not used
            }
        }

        return totalBonus;
    }
    private static int  CalculateMaterial(byte[] board, ref int whiteLargePiecesVal, ref int blackLargePiecesVal)
    {
        int materialEval = 0;

        // Scanning the whole board (optimised board scan not supported yet)
        for (int i = 0; i < 64; i++)
        {
            byte piece = board[i];
            switch (piece)
            {
                case 1: 
                    materialEval += pVal; 
                    break;
                case 2: 
                    materialEval += nVal;
                    whiteLargePiecesVal += nVal;
                    break;
                case 3: 
                    materialEval += bVal;
                    whiteLargePiecesVal += bVal;
                    break;
                case 4: 
                    materialEval += rVal;
                    whiteLargePiecesVal += rVal;
                    break;
                case 5: 
                    materialEval += qVal;
                    whiteLargePiecesVal += qVal;
                    break;
                case 6: 
                    materialEval += kVal; 
                    break;

                case 9:  
                    materialEval -= pVal; 
                    break;
                case 10: 
                    materialEval -= nVal;
                    blackLargePiecesVal -= nVal;
                    break;
                case 11: 
                    materialEval -= bVal;
                    blackLargePiecesVal -= bVal;
                    break;
                case 12: 
                    materialEval -= rVal;
                    blackLargePiecesVal -= rVal;
                    break;
                case 13: 
                    materialEval -= qVal;
                    blackLargePiecesVal -= qVal;
                    break;
                case 14: 
                    materialEval -= kVal; 
                    break;
            }
        }

        return materialEval; // Return the material score
    }
    private static int  CalculateCenterControl(byte[] board)
    {
        int controlScore = 0;
        byte piece = board[27];
        // Central squares are: (e4, d4, e5, d5) (27, 28, 35, 36)
        if (board[35] == 1)
        {
            if (board[42]      == 1) controlScore += 15;
            if (board[44]      == 3) controlScore += 5;
            if (board[45]      == 2) controlScore += 15;
            else if (board[52] == 2) controlScore += 10;
            controlScore += 10;
        }
        if (board[36] == 1)
        {
            if (board[45] == 1)      controlScore += 5;
            if (board[42] == 2)      controlScore += 5;
            else if (board[52] == 2) controlScore += 3;
            controlScore += 5;
        }

        if (board[27] == 9)
        {
            if (board[18]      == 9)  controlScore -= 15;
            if (board[20]      == 11) controlScore -= 5;
            if (board[21]      == 10) controlScore -= 15;
            else if (board[12] == 10) controlScore -= 10;
            controlScore -= 10;
        }
        if (board[36] == 9)
        {
            if (board[21]      == 9)  controlScore -= 5;
            if (board[18]      == 10) controlScore -= 5;
            else if (board[11] == 10) controlScore -= 3;
            controlScore -= 5;
        }
        // Center control calculation


        return controlScore * centerControlPriority;
    }
    /*private static int  CalculateKingSafety(byte[] board)
    {
        int kingSafety = 0;

        // King safety calculation
        kingSafety -= CountAttacks(board, wkPos, true);   // True  = our piece is white
        kingSafety += CountAttacks(board, bkPos, false);  // False = our piece is black

        // Return the value times the KingSafety coefficient
        return kingSafety * kingSafetyPriority;
    }*/
    private static int  CalculatePawnStructure(byte[] board)
    {
        int whitePawnStructure = 0;
        int blackPawnStructure = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int piece = board[i * 8 + j];
                if      (piece == 1) // For the white pawn
                {
                    if (IsIsolatedPawn(board, i, j, true)) whitePawnStructure -= pawnStructurePriority;
                }
                else if (piece == 9) // For the black pawn
                {
                    if (IsIsolatedPawn(board, i, j, false)) blackPawnStructure -= pawnStructurePriority;
                }
            }
        }

        return whitePawnStructure - blackPawnStructure;
    }
    private static bool IsIsolatedPawn(byte[] board, int x, int y, bool isWhite)
    {
        // Check for pawn chain
        if (y > 0 && y < 7)
        {
            bool hasLeftPawn;
            bool hasRightPawn;
            if (isWhite)
            {
                hasLeftPawn  = (board[y * 8 + 9 + x] == 1);
                hasRightPawn = (board[y * 8 + 7 + x] == 1);
            }
            else
            {
                hasLeftPawn  = (board[y * 8 - 9 + x] == 9);
                hasRightPawn = (board[y * 8 - 7 + x] == 9);
            }
            return !hasLeftPawn && !hasRightPawn;
        }
        return true;
    }


    



    private static int  PosAmountTest(byte[] board, int depth, bool isWhiteTurn, bool showInfo = false, byte from = 64, byte to = 64)
    {
        if (depth == 0)
        {
            if (showInfo)
            {
                if(ReadKey().Key == ConsoleKey.Spacebar) DisplayBoard(board, true, from, to);
            }
            return 1;
        }


        int posAmount = 0;
        List<Move> allMoves = GenerateAllMoves(board, isWhiteTurn);

        foreach(Move move in allMoves)
        {
            if(depth != 5 && !showInfo) posAmount += PosAmountTest(SimulateMove(board, move), depth - 1, !isWhiteTurn);
            else posAmount += PosAmountTest(SimulateMove(board, move), depth - 1, !isWhiteTurn, showInfo, move.From, move.To);
        }

        return posAmount;
    }
    private static Move StartABSearch(byte[] board, int depth, bool maximizingPlayer)
    {
        int maxEval = -999999;
        int minEval = 999999;

        List<Move> moves = GenerateAllMoves(board, maximizingPlayer);

        Move bestMove = null;
        if(moves.Count > 0) bestMove = moves[0];

        if (maximizingPlayer)
        {
            foreach (Move move in moves)
            {
                byte[] newBoard = SimulateMove(board, move);
                int eval = AlphaBetaEvalSearch(newBoard, depth - 1, maxEval, minEval, false);

                //Write("\n\tCHOSEN MOVE from: " + move.From + " to: " + move.To);
                //Write("\n\tCur eval: " + eval);
                if (eval > maxEval)
                {
                    maxEval = eval;
                    bestMove = move;
                }
            }
            //Write("\n\t\tMAX Eval: " + maxEval);
            if (maxEval > 9999 || maxEval < -9999)
            {
                //  Update "found mate in" text with new info
                //Write("\nCcH: " + depth + " to state: " + gBoardState);
                if (gBoardState == 0 || depth < Math.Abs(gBoardState)) gBoardState = (sbyte)(depth - 1);

                //  Check for stalemate
                if (Math.Abs(gBoardState) < 2 && !IsKingInCheck(board, wkPos, true)) gBoardState = 127;
            }
            //Write("  State: " + gBoardState);
            return bestMove;
        }
        else
        {
            foreach (Move move in moves)
            {
                byte[] newBoard = SimulateMove(board, move);
                int eval = AlphaBetaEvalSearch(newBoard, depth - 1, maxEval, minEval, true);

                //Write("\n\tCHOSEN from: " + move.From + " to: " + move.To);
                //Write("\n\tCur eval: " + eval);
                if (eval < minEval)
                {
                    minEval = eval;
                    bestMove = move;
                }
            }
            //Write("\n\t\tMIN Eval: " + minEval);
            if (minEval < -9999 || minEval > 9999)
            {
                //  Update "found mate in" text with new info K1k22q38
                //Write("\nCcH: " + depth + " to state: " + gBoardState);
                if (gBoardState == 0 || depth < Math.Abs(gBoardState)) gBoardState = (sbyte)-(depth - 1);

                //  Check for stalemate
                if (Math.Abs(gBoardState) < 2 && !IsKingInCheck(board, bkPos, false)) gBoardState = -127;
            }
            //Write("  State: " + gBoardState);
            return bestMove;
        }
    }
    private static int  AlphaBetaEvalSearch(byte[] board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth < 1)  // return eval result after the search ended
        {
            gEvaluatedPositions++;
            return AdvEvaluate(board, maximizingPlayer);
        }
        

        List<Move> moves = GenerateAllMoves(board, maximizingPlayer);

        if (moves.Count > 0)
        {
            if (maximizingPlayer)
            {
                foreach (Move move in moves)
                {
                    byte[] newBoard = SimulateMove(board, move);
                    int eval = AlphaBetaEvalSearch(newBoard, depth - 1, alpha, beta, false);

                    //if(depth == 1) Write("\n\t\tTry move from: " + move.From + " to: " + move.To + ", calc eval: " + eval);
                    if (eval > alpha)
                    {
                        alpha = eval;  // Set new max eval
                    }
                    if (beta <= alpha)
                    {
                        gSkippedPositions++;
                        return alpha;
                    }
                }
                return alpha;
            }
            else
            {
                foreach (Move move in moves)
                {
                    byte[] newBoard = SimulateMove(board, move);
                    int eval = AlphaBetaEvalSearch(newBoard, depth - 1, alpha, beta, true);

                    //if(depth == 1) Write("\n\t\tTry move from: " + move.From + " to: " + move.To + ", calc eval: " + eval);
                    if (eval < beta)
                    {
                        beta = eval;
                    }
                    if (beta <= alpha)
                    {
                        gSkippedPositions++;
                        return beta;
                    }
                }
                return beta;
            }
        }
        else
        {
            if (!whiteTurn)
            {
                //Write("\n\nCurrent move: " + maximizingPlayer + ", Depth: " + depth + " (Black king in check: " + IsKingInCheck(board, wkPos, true) + ") no moves!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n\n");
                if (IsKingInCheck(board, 64, false)) return 999999;
                else return Math.Max(-999999, AlphaBetaEvalSearch(board, depth - 1, alpha, beta, false));
            }
            else
            {
                //Write("\n\nCurrent move: " + maximizingPlayer + ", Depth: " + depth + " (White king in check: " + IsKingInCheck(board, wkPos, true) + ") no moves!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n\n");
                if (IsKingInCheck(board, 64, true)) return -999999;
                else return Math.Min(999999, AlphaBetaEvalSearch(board, depth - 1, alpha, beta, true));
            }
        }
    }




    private static bool GetTurn()
    {
        string userInput;

        Write("\n\tWhose turn? (W / Y / YES / 1  = player white turn): ");
        userInput = ReadLine().Trim().ToLower();

        if (userInput == "w" || userInput == "y" || userInput == "yes" || userInput == "1") 
            return true;  // true  = white's turn
        return false;     // false = black's turn
    }
    private static int  GetDepth()
    {
        string userInput;
        int depth = -1;
        Write("\tEnter the depth for the algorithm search (only from 1 to 10): ");
        while (depth < 1 || depth > 10)
        {
            userInput = ReadLine();
            Clear();
            if (!int.TryParse(userInput, out depth)) Write("\tInvalid input, please try again: ");
            else if (depth < 1 || depth > 10) Write("\tOut of bounds, please enter a valid number from the interval: ");
        }
        Write("\n");
        return depth;
    }

    private static bool GetBoardDisplayType()
    {
        Write("\n\tDisplay chess unicode pieces? (Yes/Y/1/Да = display unicode, else = numbers): ");
        string userInput = ReadLine().ToLower().Replace(" ", "");
        if (userInput == "yes" || userInput == "y" || userInput == "1" || userInput == "да") return true;
        return false;
    }

    private static bool TryParseUserMove(byte[] board, string userInput)
    {
        if (userInput.Length == 4)
        {
            byte from = 0;
            byte to = 0;

            switch (userInput[0])
            {
                case 'a':              break;
                case 'b': from += 1;  break;
                case 'c': from += 2;  break;
                case 'd': from += 3;  break;
                case 'e': from += 4;  break;
                case 'f': from += 5;  break;
                case 'g': from += 6;  break;
                case 'h': from += 7;  break;

                default: return false;
            }
            switch (userInput[1])
            {
                case '1': from += 56; break;
                case '2': from += 48; break;
                case '3': from += 40; break;
                case '4': from += 32; break;
                case '5': from += 24; break;
                case '6': from += 16; break;
                case '7': from += 8;  break;
                case '8': break;

                default: return false;
            }

            switch (userInput[2])
            {
                case 'a':            break;
                case 'b': to += 1;  break;
                case 'c': to += 2;  break;
                case 'd': to += 3;  break;
                case 'e': to += 4;  break;
                case 'f': to += 5;  break;
                case 'g': to += 6;  break;
                case 'h': to += 7;  break;

                default: return false;
            }
            switch (userInput[3])
            {
                case '1': to += 56; break;
                case '2': to += 48; break;
                case '3': to += 40; break;
                case '4': to += 32; break;
                case '5': to += 24; break;
                case '6': to += 16; break;
                case '7': to += 8;  break;
                case '8':            break;

                default: return false;
            }

            Move userMove = new Move { From = from, To = to, Piece = board[from], CapturedPiece = board[to] };
            ApplyMove(board, userMove);
            return true;
        }
        else return false;
    }
}
    