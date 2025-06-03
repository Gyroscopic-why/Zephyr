using System.Collections.Generic;


using static Zephyr.Configs;
using static Zephyr.BoardMoveLogic;


namespace Zephyr
{
    internal class MoveValidation
    {

        static public List<Move> IsMoveLegalNoCheckCriteria(byte[] board, List<Move> moves, bool isKingWhite)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].Piece == 6 || moves[i].Piece == 14)  // The king has moved
                {
                    /*if (moves[i].Piece == 14)
                    {
                        Write("\nB-King moved from: " + moves[i].From + " to: " + moves[i].To);
                        //Write("\nCan attack: " + CountAttacks(SimulateMove(board, moves[i]), moves[i].To, isKingWhite));
                        Write("\nIn check: " + IsKingInCheck(SimulateMove(board, moves[i]), moves[i].To, isKingWhite));
                    }
                    if (moves[i].Piece == 6)
                    {
                        Write("\nW-King moved from: " + moves[i].From + " to: " + moves[i].To);
                        //Write("\nCan attack: " + CountAttacks(SimulateMove(board, moves[i]), moves[i].To, isKingWhite));
                        Write("\nIn check: " + IsKingInCheck(SimulateMove(board, moves[i]), moves[i].To, isKingWhite));
                    }*/
                    if (IsKingInCheck(SimulateMove(board, moves[i]), moves[i].To, isKingWhite))
                    {
                        moves.RemoveAt(i);  // Clear the moves where the king is still in check
                        i--;                 // Decrease by one so by the end of the loop (i++) we dont hop over a move
                    }
                }
                else
                {
                    if (IsKingInCheck(SimulateMove(board, moves[i]), 64, isKingWhite)) // The king hasnt moved
                    {
                        moves.RemoveAt(i);  // Clear the moves where the king is still in check
                        i--;                 // Decrease by one so by the end of the loop (i++) we dont hop over a move
                    }
                }
            }

            return moves;
        }
        static public bool IsKingInCheck(byte[] board, byte kingPos, bool kingColor)
        {
            if (kingPos == 64)
            {
                for (int i = 0; i < 63; i++)
                {
                    if (kingColor && board[i] == 6 || !kingColor && board[i] == 14)
                    {
                        kingPos = (byte)i;
                        i += 64;
                    }
                }
            }
            int[] rookOffsets = { -1, -8, 8, 1 };          // Horizontal (-1, 1) and vertical (-8, 8) moves (1st iteration)
            byte xPos = (byte)(kingPos % 8);              // Enemy x position

            for (int i = 0; i < 4; i++)
            {
                if ((i < 1 && xPos > 0) || i == 1 || i == 2 || (i > 2 && xPos < 7))
                {   // Prevent the check calculation from looping around the board (optimised checking)

                    int newPosition = kingPos + rookOffsets[i];   // Choose a new direction for checking
                    while (newPosition >= 0 && newPosition < 64)
                    {
                        byte target = board[newPosition];  // target piece
                        if (target != 0)
                        {

                            if (kingColor)
                            {
                                //Write("\n\tFrom:" + newPosition + " to:" + kingPos + " target:" + target + " kingCol:" + kingColor);
                                if (target == 12 || target == 13) return true;

                                else if (target == 14 && (
                                newPosition - kingPos == -1 ||
                                newPosition - kingPos == -8 ||
                                newPosition - kingPos == 8 ||
                                newPosition - kingPos == 1)
                                )

                                    //  Check for being attacked by an enemy king
                                    return true; // return that the king is in check
                            }
                            else
                            {
                                if (target == 4 || target == 5) return true;

                                else if (target == 6 && (
                                newPosition - kingPos == -1 ||
                                newPosition - kingPos == -8 ||
                                newPosition - kingPos == 8 ||
                                newPosition - kingPos == 1)
                                )

                                    //  Check for being attacked by an enemy king
                                    return true;
                            }

                            break;  // if a friendly piece is blocking us stop moving further
                        }

                        xPos = (byte)(newPosition % 8);                  // New enemy x position

                        if ((i < 1 && xPos > 0) || i == 1 || i == 2 || (i > 2 && xPos < 7))
                        {   // Prevent the check calculation from looping around the board (optimised checking)

                            newPosition += rookOffsets[i];  // Continue moving in that direction
                        }
                        else newPosition = 64;
                    }
                }
            }

            int[] knightOffsets = { -10, 6, -17, 15, -15, 17, -6, 10 };  // All possible knight moves
            xPos = (byte)(kingPos % 8);                                 // Enemy x position
            for (int i = 0; i < 8; i++)                          // For each square that the knight can attack
            {
                if ((i < 2 && xPos > 1) || (i > 1 && i < 4 && xPos > 0) || (i > 3 && i < 6 && xPos < 7) || (i > 5 && xPos < 6))
                {   // Prevent the knight from looping around the board (optimised checking)

                    int newPosition = kingPos + knightOffsets[i];    //   New position for the knight

                    if (newPosition >= 0 && newPosition < 64)         //   If the square is on the board
                    {
                        byte target = board[newPosition];            //   Target piece

                        if ((kingColor && target == 10) || (!kingColor && target == 2))  // If we are white and capturing black enemy piece
                            return true;  // return that the king is in check
                    }
                }
            }

            int[] bishopOffsets = { -9, 7, -7, 9 };         // Diagonal moves (1st iteration)
            for (int i = 0; i < 4; i++)                      // For each square that we can move to
            {
                xPos = (byte)(kingPos % 8);
                if ((i < 2 && xPos > 0) || (i > 1 && xPos < 7))
                {   // Prevent the bishop from looping around the board (optimised checking)

                    int newPosition = kingPos + bishopOffsets[i];       // Choose a new direction

                    while (newPosition >= 0 && newPosition < 64)
                    {   // If the square is on the board, and we moved in a single diagonal direction

                        byte target = board[newPosition];                          // Target piece
                        if (target != 0)                      // If we are moving to an empty square
                        {
                            if (kingColor)             //  For the white king check for black pieces
                            {
                                if (target == 13 || target == 11) return true;
                                //  Check for a diagonal attack from the bishop or queen (unlimitied range)

                                else if (target == 9)
                                {
                                    if (newPosition - kingPos == -7 ||
                                        newPosition - kingPos == -9)

                                        //  Return true if a pawn attacks us 1 square diagonally
                                        return true;
                                }
                                else if (target == 14)
                                {
                                    if (newPosition - kingPos == -7 ||
                                        newPosition - kingPos == -9 ||
                                        newPosition - kingPos == 7 ||
                                        newPosition - kingPos == 9)
                                        return true;    // Return that the king is attacked by another king
                                }
                            }
                            else                              //  For the black king check for white pieces
                            {
                                if (target == 5 || target == 3) return true;
                                //  Check for a diagonal attack from the bishop or queen (unlimitied range)

                                else if (target == 1)
                                {
                                    if (newPosition - kingPos == 7 ||
                                        newPosition - kingPos == 9)

                                        //  Return true if a pawn attacks us 1 square diagonally
                                        return true;
                                }
                                else if (target == 6)
                                {
                                    if (newPosition - kingPos == -7 ||
                                        newPosition - kingPos == -9 ||
                                        newPosition - kingPos == 7 ||
                                        newPosition - kingPos == 9)
                                        return true;  // Return that the king is attacked by another king
                                }
                            }
                            break;  // if a friendly piece is blocking us stop checking further
                        }

                        xPos = (byte)(newPosition % 8);                 // new Diagonal x position

                        if ((i < 2 && xPos > 0) || (i > 1 && xPos < 7))
                        {   // Prevent the bishop from looping around the board (optimised checking)

                            newPosition += bishopOffsets[i]; // Continue moving diagonally in that direction
                        }
                        else newPosition = 64;                                    // Break but more efficient
                    }
                }
            }


            return false;                 // return that the king is not in check
        }

    }
}