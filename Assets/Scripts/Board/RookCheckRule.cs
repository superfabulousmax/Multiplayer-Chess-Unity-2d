using UnityEngine;

public class RookCheckRule : ICheckRule
{
    public bool PossibleCheck(Board board, ChessPiece rook, Vector3Int position, out ChessPiece king)
    {
        var y = position.y;
        var x = position.x;

        king = board.GetOppositeKing(rook.PlayerColour);
        var kingPosition = king.TilePosition;

        if (kingPosition.y > y && kingPosition.x > x)
            return false;
        if (kingPosition.y < y && kingPosition.x < x)
            return false;
        if (kingPosition.y == y && kingPosition.x == x)
            return false;

        var boardState = board.GetBoardState();
        var kingId = (uint)king.NetworkObjectId;

        if (kingPosition.x == x)
        {
            // moving up or down

            if (kingPosition.y > y)
            {
                for (int i = y + 1; i <= kingPosition.y; i++)
                {
                    if (boardState[i, x] != kingId && boardState[i, x] >= 0)
                    {
                        return false;
                    }
                    else if(boardState[i, x] == kingId)
                    {
                        return true;
                    }
                }
            }
            else if (kingPosition.y < y)
            {
                for (int i = y - 1; i >= kingPosition.y; i--)
                {
                    if (boardState[i, x] != kingId && boardState[i, x] >= 0)
                    {
                        return false;
                    }
                    else if (boardState[i, x] == kingId)
                    {
                        return true;
                    }
                }
            }

        }
        else
        {
            // moving left or right
            if (y != kingPosition.y)
            {
                return false;
            }
            if (kingPosition.x < x)
            {
                // moving left

                for (int i = kingPosition.x + 1 ; i < x; i++)
                {
                    if (boardState[y, i] != (int)rook.NetworkObjectId && boardState[y, i] >= 0)
                    {
                        return false;
                    }
                    else if (boardState[y, i] == (int)rook.NetworkObjectId)
                    {
                        return true;
                    }
                }
            }
            else
            {
                // moving right
                for (int i = x + 1; i <= kingPosition.x; i++)
                {
                    if (boardState[y, i] != kingId && boardState[y, i] >= 0)
                    {
                        return false;
                    }
                    else if (boardState[y, i] == kingId)
                    {
                        return true;
                    }
                }
            }
        }

        return false;

    }
}
