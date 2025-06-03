using static System.Console;


using static Zephyr.Configs;



namespace Zephyr
{
    internal class BoardMoveLogic
    {

        static public byte[] ApplyMove(byte[] board, Move move)
        {
            if (move != null)
            {
                if (board[move.To] != 0)      // If the moved to square is not empty
                {                              // Print  which piece was captured
                    Write("\n\n\n\t\t\t   [!]  - ");
                    switch (board[move.To])
                    {
                        case wp1:
                            Write("White pawn was taken");
                            break;

                        case wn1:
                            Write("White knight was taken");
                            break;

                        case wb1:
                            Write("White bishop was taken");
                            break;

                        case wr1:
                            Write("White rook was taken");
                            break;

                        case wq1:
                            Write("White queen was taken");
                            break;

                        case wk1:
                            Write("errcode: 2, white king was captured");
                            break;



                        case bp1:
                            Write("Black pawn was taken");
                            break;

                        case bn1:
                            Write("Black knight was taken");
                            break;

                        case bb1:
                            Write("Black bishop was taken");
                            break;

                        case br1:
                            Write("Black rook was taken");
                            break;

                        case bq1:
                            Write("Black queen was taken");
                            break;

                        case bk1:
                            Write("errcode: 3, black king was captured");
                            break;
                    }
                    gPieceGotEaten = true;
                }
                board[move.To] = move.Piece;  // Move  the piece to the new square
                board[move.From] = 0;         // Clear the previous square

                // If the moved piece was the king
                if (move.Piece == 6) wkPos = move.To;  // Save the new white king position
                else if (move.Piece == 14) bkPos = move.To;  // Save the new black king position
                
            }
            return board;
        }

        static public byte[] UndoMove(byte[] board, Move move)
        {
            //  Return moved piece to the old position
            board[move.From] = move.Piece;

            //  Restore the captured piece
            board[move.To] = move.CapturedPiece;

            return board;
        }

        static public byte[] SimulateMove(byte[] oldBoard, Move move)
        {
            byte[] newBoard = (byte[])oldBoard.Clone();

            if      (move.To == 6)  wkPos = move.To;  // If white king moves, update the king position
            else if (move.To == 14) bkPos = move.To;  // If black king moves, update the king position

            newBoard [move.To]   = move.Piece;  //
            newBoard [move.From] = 0;           // Simulating the move

            return newBoard;        // Returning the new board with the simulated move
        }
    }
}