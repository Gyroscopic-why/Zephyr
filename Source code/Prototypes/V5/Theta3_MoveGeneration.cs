using System.Collections.Generic;


using static Zephyr.Configs;
using static Zephyr.MoveValidation;


namespace Zephyr
{
    internal class MoveGeneration
    {

        static public List<Move> GenerateAllMoves(byte[] thisboard, bool isWhiteTurn)
        {
            List<Move> moves = new List<Move>();
            for (byte i = 0; i < 64; i++)
            {
                byte piece = thisboard[i];
                if (piece != 0 && (isWhiteTurn && piece < 8) || (!isWhiteTurn && piece > 8))
                {
                    switch (piece)
                    {
                        case 1:  // White pawn
                        case 9:  // Black pawn
                            moves.AddRange(                                                // Thirdly we save all the legal moves
                                IsMoveLegalNoCheckCriteria(thisboard,                      // Secondly we kill of the pawn moves where the king is in check
                                GeneratePawnMoves(thisboard, i, isWhiteTurn),             // First we generate all pawn moves
                                isWhiteTurn));                                             // King color for the legal checking
                            break;
                        case 2:  // White knight
                        case 10: // Black knight
                            moves.AddRange(                                                // Thirdly we save all the legal moves
                                IsMoveLegalNoCheckCriteria(thisboard,                      // Secondly we kill of the moves where the king is in check
                                GenerateKnightMoves(thisboard, i, piece, isWhiteTurn),   // First we generate all knight moves
                                isWhiteTurn));                                             // King color for the legal checking
                            break;

                        case 3:  // White bishop
                        case 11: // Black bishop
                            moves.AddRange(                                                // Thirdly we save all the legal moves
                                IsMoveLegalNoCheckCriteria(thisboard,                      // Secondly we kill of the moves where the king is in check
                                GenerateBishopMoves(thisboard, i, piece, isWhiteTurn),   // First we generate all bishop moves
                                isWhiteTurn));                                             // King color for the legal checking
                            break;

                        case 4:  // White rook
                        case 12: // Black rook
                            moves.AddRange(                                                // Thirdly we save all the legal moves
                                IsMoveLegalNoCheckCriteria(thisboard,                      // Secondly we kill of the moves where the king is in check
                                GenerateRookMoves(thisboard, i, piece, isWhiteTurn),     // First we generate all rook moves
                                isWhiteTurn));                                             // King color for the legal checking
                            break;

                        case 5:  // White queen
                        case 13: // Black queen
                            moves.AddRange(                                                // Thirdly we save all the legal moves
                                 IsMoveLegalNoCheckCriteria(thisboard,                     // Secondly we kill of the moves where the king is in check
                                 GenerateQueenMoves(thisboard, i, piece, isWhiteTurn),   // First we generate all queen moves
                                 isWhiteTurn));                                            // King color for the legal checking
                            break;

                        case 6:  // White king
                        case 14: // Black king
                            moves.AddRange(                                                // Thirdly we save all the legal moves
                                 IsMoveLegalNoCheckCriteria(thisboard,                     // Secondly we kill of the moves where the king is in check
                                 GenerateKingMoves(thisboard, i, piece, isWhiteTurn),    // First we generate all king moves
                                 isWhiteTurn));                                            // King color for the legal checking
                            break;
                    }
                }
            }
            return moves;
        }



        static public List<Move> GeneratePawnMoves(byte[] board, byte position, bool isWhite)
        {
            List<Move> generatedMoves = new List<Move>();  // We will store the generated moves here

            int direction = isWhite ? -8 : 8;         // Movement direction is different for each players pawns
            byte startRow = (byte)(isWhite ? 6 : 1);  // First row for counting the double pawn moves
            byte promotionRow = (byte)(isWhite ? 0 : 7);  // Pawn promotion row
            byte xPos = (byte)(position % 8);             // Pawn x position

            // Basic pawn move
            int newPosition = position + direction;
            if (newPosition >= 0 && newPosition < 64 && board[newPosition] == 0)
            {   // Check if we are moving to a valid empty square

                if (newPosition / 8 == promotionRow)
                {
                    // Pawn promotion  (High priority in the checking list)
                    generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wn1 : bn1), CapturedPiece = 0 });  // Promoting to a knight
                    generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wb1 : bb1), CapturedPiece = 0 });  // Promoting to a bishop
                    generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wr1 : br1), CapturedPiece = 0 });  // Prompting to a rook
                    generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wq1 : bq1), CapturedPiece = 0 });  // Promoting to a queen
                }
                else
                {   // Generic move up by one square
                    generatedMoves.Add(new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wp1 : bp1), CapturedPiece = 0 });
                }

                // Double move for pawns in the start position
                if (position / 8 == startRow && board[newPosition + direction] == 0)
                {
                    generatedMoves.Add(new Move { From = position, To = (byte)(newPosition + direction), Piece = board[position], CapturedPiece = 0 });
                }
            }

            // Diagonal attack to the left (capture to the left)
            if (xPos > 0)
            {
                byte attackPosition = (byte)(isWhite ? position - 9 : position + 7);         // Target position
                if (attackPosition >= 0 && attackPosition < 64)
                {
                    byte target = board[attackPosition];                   // Target piece
                    if (target != 0 && ((isWhite && target > 8) || (!isWhite && target < 8)))
                    {
                        if (attackPosition / 8 == promotionRow)
                        {
                            // Check for promotion after attack
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wn1 : bn1), CapturedPiece = 0 });  // Promoting to a knight
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wb1 : bb1), CapturedPiece = 0 });  // Promoting to a bishop
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wr1 : br1), CapturedPiece = 0 });  // Prompting to a rook
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wq1 : bq1), CapturedPiece = 0 });  // Promoting to a queen
                        }
                        else
                        {
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)attackPosition, Piece = (byte)(isWhite ? wp1 : bp1), CapturedPiece = target });
                        }
                    }
                }
            }
            // Diagonal attack to the right (capture to the right)
            if (xPos < 7)
            {
                byte attackPosition = (byte)(isWhite ? position - 7 : position + 9);         // Target position
                if (attackPosition >= 0 && attackPosition < 64)
                {
                    byte target = board[attackPosition];                   // Target piece
                    if (target != 0 && ((isWhite && target > 8) || (!isWhite && target < 8)))
                    {
                        if (attackPosition / 8 == promotionRow)
                        {
                            // Check for promotion after attack
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wn1 : bn1), CapturedPiece = 0 });  // Promoting to a knight
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wb1 : bb1), CapturedPiece = 0 });  // Promoting to a bishop
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wr1 : br1), CapturedPiece = 0 });  // Prompting to a rook
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = (byte)(isWhite ? wq1 : bq1), CapturedPiece = 0 });  // Promoting to a queen
                        }
                        else
                        {
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)attackPosition, Piece = (byte)(isWhite ? wp1 : bp1), CapturedPiece = target });
                        }
                    }
                }
            }

            return generatedMoves;
        }
        static public List<Move> GenerateKnightMoves(byte[] board, byte position, byte piece, bool isWhite)
        {
            List<Move> moves = new List<Move>();            // We will store the generated moves here
            int[] knightOffsets = { -10, 6, -17, 15, -15, 17, -6, 10 };  // All possible knight moves
            byte xPos = (byte)(position % 8);                               // The knight x position

            for (int i = 0; i < 8; i++)                          // For each square that we can move to
            {
                if ((i < 2 && xPos > 1) || (i > 1 && i < 4 && xPos > 0) || (i > 3 && i < 6 && xPos < 7) || (i > 5 && xPos < 6))
                {   // Prevent the knight from looping around the board (optimised checking)

                    int newPosition = position + knightOffsets[i]; //   New position for the knight

                    if (newPosition >= 0 && newPosition < 64)       // If the square is on the board
                    {
                        byte target = board[newPosition];          //   Target piece

                        if (target == 0) // If we are moving to an empty square
                        {
                            // Save the generated move
                            moves.Add(new Move { From = position, To = (byte)newPosition, Piece = piece, CapturedPiece = 0 });
                        }
                        else if ((isWhite && target > 8) || (!isWhite && target < 8))  // If we are white and capturing black enemy piece
                        {
                            // Save the generated move   (Higher priority in the list)
                            moves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = piece, CapturedPiece = target });
                        }
                    }
                }
            }
            return moves;  // Return all possible moves
        }
        static public List<Move> GenerateBishopMoves(byte[] board, byte position, byte piece, bool isWhite)
        {
            List<Move> generatedMoves = new List<Move>();   // We will store the generated moves here
            int[] bishopOffsets = { -9, 7, -7, 9 };         // Diagonal moves (1 iteration)
            byte xPos;                                      // Bishop x position

            for (int i = 0; i < 4; i++)                        // For each square that we can move to
            {
                xPos = (byte)(position % 8);
                if ((i < 2 && xPos > 0) || (i > 1 && xPos < 7))
                {   // Prevent the bishop from looping around the board (optimised checking)

                    int newPosition = position + bishopOffsets[i];       // Choose a new direction

                    while (newPosition >= 0 && newPosition < 64)
                    {   // If the square is on the board, and we moved in a single diagonal direction

                        byte target = board[newPosition];                          // Target piece
                        if (target == 0)                      // If we are moving to an empty square
                        {
                            // Save the move, captured piece = none (empty = 0)
                            generatedMoves.Add(new Move { From = position, To = (byte)newPosition, Piece = piece, CapturedPiece = 0 });
                        }
                        else    // If we are blocked by a piece
                        {
                            if ((isWhite && target > 8) || (!isWhite && target < 8))  // If we are blocked by an enemy piece
                            {
                                // Save the move with capturing an enemy piece  (Higher priority for the capture)
                                generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = piece, CapturedPiece = target });
                            }
                            break;  // if a piece is blocking us stop moving further
                        }

                        xPos = (byte)(newPosition % 8);                    // Bishop new x position

                        if ((i < 2 && xPos > 0) || (i > 1 && xPos < 7))
                        {   // Prevent the bishop from looping around the board (optimised checking)

                            newPosition += bishopOffsets[i]; // Continue moving diagonally in that direction
                        }
                        else newPosition = 64;                                    // Break but more efficient
                    }
                }
            }

            return generatedMoves;  // return the generated moves
        }
        static public List<Move> GenerateRookMoves(byte[] board, byte position, byte piece, bool isWhite)
        {
            List<Move> generatedMoves = new List<Move>();  // We will store the generated moves here
            int[] rookOffsets = { -1, -8, 8, 1 };          // Horizontal (-1, 1) and vertical (-8, 8) rook moves (1 iteration)
            byte xPos;

            for (int i = 0; i < 4; i++)
            {
                xPos = (byte)(position % 8);             // Rook x position

                if ((i < 1 && xPos > 0) || i == 1 || i == 2 || (i > 2 && xPos < 7))
                {   // Prevent the rook from looping around the board (optimised checking)

                    int newPosition = position + rookOffsets[i];   // Choose a new diagonal direction for moving
                    while (newPosition >= 0 && newPosition < 64)
                    {
                        byte target = board[newPosition];  // Target piece
                        if (target == 0)                     // If we are moving to an empty square
                        {
                            // Save the move, captured piece = none (empty = 0)
                            generatedMoves.Add(new Move { From = position, To = (byte)newPosition, Piece = piece, CapturedPiece = 0 });
                        }
                        else  // If we are blocked by a piece
                        {
                            if ((isWhite && target > 8) || (!isWhite && target < 8)) // If we are blocked by an enemy piece
                            {
                                // Save the move with capturing an enemy piece
                                generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = piece, CapturedPiece = target });
                            }
                            break; // if a piece is blocking us stop moving further
                        }

                        xPos = (byte)(newPosition % 8);                   // Rook new x position

                        if ((i < 1 && xPos > 0) || i == 1 || i == 2 || (i > 2 && xPos < 7))
                        {   // Prevent the rook from looping around the board (optimised checking)

                            newPosition += rookOffsets[i];  // Continue moving in that direction
                        }
                        else newPosition = 64;
                    }
                }
            }
            return generatedMoves;  // return the generated moves
        }
        static public List<Move> GenerateQueenMoves(byte[] board, byte position, byte piece, bool isWhite)
        {
            List<Move> generatedMoves;

            // The queen is literally just a rook + bishop
            // So we calculate as if the piece is a bishop or a rook and add all moves together
            generatedMoves = GenerateBishopMoves(board, position, piece, isWhite);          // Diagonal
            generatedMoves.AddRange(GenerateRookMoves(board, position, piece, isWhite));  // Horizontal and vertical

            return generatedMoves;  // Return generated moves
        }
        static public List<Move> GenerateKingMoves(byte[] board, byte position, byte piece, bool isWhite)
        {
            List<Move> generatedMoves = new List<Move>();        // We will store the generated moves here
            int[] kingOffsets = { -9, -1, 7, -8, 8, -7, 1, 9 };  // All posible king moves
            byte xPos = (byte)(position % 8);                   // King x position

            for (int i = 0; i < 8; i++)                 // For each square that we can move to
            {
                if ((i < 3 && xPos > 0) || (i > 4 && i < 8 && xPos < 7) || i == 3 || i == 4)
                {   // Prevent the king from looping around the board     (optimised checking)

                    int newPosition = position + kingOffsets[i];
                    if (newPosition >= 0 && newPosition < 64)
                    {   // If the square on the board, and we moved in a single diagonal direction
                        // Additional check to prevent moving out of range (useless ? I commented it temporary, probably remove it)


                        byte target = board[newPosition];  // Target piece
                        if (target == 0)
                        {   // If we are moving to an empty square, or we are capturing a piece of the opposite color

                            // Save the generated move
                            generatedMoves.Add(new Move { From = position, To = (byte)newPosition, Piece = piece, CapturedPiece = target });
                        }
                        else if (isWhite && target > 8)
                        {
                            // Save the generated move   (Higher priority)
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = piece, CapturedPiece = target });
                        }
                        else if (!isWhite && target < 8)
                        {
                            // Save the generated move   (Higher priority)
                            generatedMoves.Insert(0, new Move { From = position, To = (byte)newPosition, Piece = piece, CapturedPiece = target });
                        }
                    }
                }
            }
            return generatedMoves;  // Return the generated moves;
        }

    }
}