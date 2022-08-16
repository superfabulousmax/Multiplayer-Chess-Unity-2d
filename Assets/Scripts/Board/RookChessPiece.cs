using UnityEngine;

public class RookChessPiece : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        takenPiece = false;
        var boardState = board.GetBoardState();
        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;
        if (newPosition.y > y && newPosition.x > x)
            return false;
        if (newPosition.y < y && newPosition.x < x)
            return false;
        if (newPosition.y == y && newPosition.x == x)
            return false;

        if (newPosition.x == x)
        {
            // moving up or down
            // check piece in the way
            if (newPosition.y > y)
            {
                for (int i = y + 1; i < newPosition.y; i++)
                {
                    if (boardState[i, x] >= 0)
                        return false;
                }
            }
            else if (newPosition.y < y)
            {
                for (int i = y - 1; i > newPosition.y; i--)
                {
                    if (boardState[i, x] >= 0)
                        return false;
                }
            }

        }
        else
        {
            // moving left or right
            if (y != newPosition.y)
            {
                return false;
            }
            if (newPosition.x < x)
            {
                // moving left

                for (int i = newPosition.x + 1; i < x; i++)
                {
                    if (boardState[y, i] >= 0)
                        return false;
                }
            }
            else
            {
                // moving right
                for (int i = x + 1; i < newPosition.x; i++)
                {
                    if (boardState[y, i] >= 0)
                        return false;
                }
            }
        }


        if(board.CheckPiece(boardState[newPosition.y, newPosition.x], ChessPiece.ChessPieceType.King))
        {
            return false;
        }   
        if (boardState[newPosition.y, newPosition.x] > 0  && activeColour == board.GetPieceFromId((uint)boardState[newPosition.y, newPosition.x]).PlayerColour)
        {
            return false;
        }
        if (boardState[newPosition.y, newPosition.x] > 0)
        {
            takenPiece = true;
            Debug.Log("rook taking new piece");
        }

        return true;
    }
}
