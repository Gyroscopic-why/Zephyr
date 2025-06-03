namespace Zephyr
{
    internal class Configs
    {

        public const byte emptySquare = 0;
        // This thing will be asigned to multiple symbols which resemble an empty square

        public const byte decodingError = 8;  // Error for entering an invalid character



        public const byte wp1 = 1;    // White Pawn   at uneven (black) squares, code: P
        public const byte wn1 = 2;    // White kNight at uneven (black) squares, code: N
        public const byte wb1 = 3;    // White Bishop at uneven (black) squares, code: B
        public const byte wr1 = 4;    // White Rook   at uneven (black) squares, code: R
        public const byte wq1 = 5;    // White Queen  at uneven (black) squares, code: Q
        public const byte wk1 = 6;    // White King   at uneven (black) squares, code: K

        public const byte wp2 = 16;   // White Pawn   at even   (white) squares, code: P
        public const byte wn2 = 32;   // White kNight at even   (white) squares, code: N
        public const byte wb2 = 48;   // White Bishop at even   (white) squares, code: B
        public const byte wr2 = 64;   // White Rook   at even   (white) squares, code: R
        public const byte wq2 = 80;   // White Queen  at even   (white) squares, code: Q
        public const byte wk2 = 96;   // White King   at even   (white) squares, code: K


        public const byte bp1 = 9;    // Black Pawn   at uneven (black) squares, code: p
        public const byte bn1 = 10;   // Black kNight at uneven (black) squares, code: n
        public const byte bb1 = 11;   // Black Bishop at uneven (black) squares, code: b
        public const byte br1 = 12;   // Black Rook   at uneven (black) squares, code: r
        public const byte bq1 = 13;   // Black Queen  at uneven (black) squares, code: q
        public const byte bk1 = 14;   // Black King   at uneven (black) squares, code: k

        public const byte bp2 = 144;  // Black Pawn   at even   (white) squares, code: p
        public const byte bn2 = 160;  // Black kNight at even   (white) squares, code: n
        public const byte bb2 = 176;  // Black Bishop at even   (white) squares, code: b
        public const byte br2 = 192;  // Black Rook   at even   (white) squares, code: r
        public const byte bq2 = 208;  // Black Queen  at even   (white) squares, code: q
        public const byte bk2 = 224;  // Black King   at even   (white) squares, code: k




        //  Material value constants
        public const int pVal = 101;     // Pawn   value
        public const int nVal = 300;     // kNight value
        public const int bVal = 321;     // Bishop value
        public const int rVal = 500;     // Rook   value
        public const int qVal = 915;     // Queen  value
        public const int kVal = 100000;  // King   value




        // Additional factors for evaluating the position
        public const int rookOpenFileBonus         = 30 ;
        public const int queenOpenFileBonus        = 25 ;
        public const int openFileNotUsedPunishment = -5 ;


        public const int centerControlPriority   = 1   ;
        public const int enemyInCheckPriority    = 250 ;
        public const int piecePositionPriority   = 1   ;
        public const int losingPlayerInCorner    = 50  ;
        public const int kingAggressionInEndgame = 20  ;
        public const int pawnStructurePriority   = 15  ;

        //public const int kingSafetyPriority        = 40    ;
        //public const int pieceActivityPriority     = 5     ;





        // 32 byte optimised board and 64 byte classic board are stored here
        public static byte[] /*optimisedBoard = new byte[32],*/ mainBoard = new byte[64];

        static public byte wkPos;                   //  White king position
        static public byte bkPos;                   //  Black king position
        static public bool whiteTurn;               //  Storing the player turn
        static public bool gPieceGotEaten = false;  //  Storing if the last move was a capture




        //  0         = normal game
        //
        //   2 -  8   = checkmate for white in 1-7 moves
        //  -2 - -8   = checkmate for black in 1-7 moves
        //
        //   1        = black won the game
        //  -1        = white won the game
        //
        //   127      = stalemate for white (draw)
        //  -127      = stalemate for black (draw)
        //
        //   126      = standart draw (impossible to checkmate)
        //  -126      = draw by repetition
        //
        //   125      = draw by 50 rule
        static public sbyte gBoardState;        //  Storing the game state




        static public int gSkippedPositions;       //  Storing the amount of skipped positions
        static public int gEvaluatedPositions;     //  Storing the amount of evaluated positions
    }
}