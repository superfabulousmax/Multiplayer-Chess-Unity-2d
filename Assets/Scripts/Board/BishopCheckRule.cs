using UnityEngine;

public class BishopCheckRule : ICheckRule
{
    public bool PossibleCheck(Board board, ChessPiece bishop, Vector3Int position, out ChessPiece king)
    {
        var y = position.y;
        var x = position.x;

        king = board.GetOppositeKing(bishop.PlayerColour);

        var kingPosition = king.TilePosition;

        if (kingPosition.y == y)
            return false;
        if (kingPosition.x == x)
            return false;

        int xDiff = Mathf.Abs(kingPosition.x - x);
        int yDiff = Mathf.Abs(kingPosition.y - y);

        if (xDiff != yDiff)
            return false;

        var boardState = board.GetBoardState();

        var kingId = (uint)king.NetworkObjectId;
        var bishopId = (int)bishop.NetworkObjectId;

        if (kingPosition.x < x)
        {
            if (kingPosition.y > y)
            {
                int i = kingPosition.x + 1;
                int j = kingPosition.y - 1;
                // from king to bishop position
                while (i <= x && j >= y)
                {
                    if (boardState[j, i] >= 0 && boardState[j, i] != bishopId)
                    {
                        return false;
                    }
                    else if(boardState[j, i] == bishopId)
                    {
                        return true;
                    }
                    i++;
                    j--;
                }
            }
            else
            {
                int i = kingPosition.x + 1;
                int j = kingPosition.y + 1;

                // from king to bishop position
                while (i <= x && j <= y)
                {
                    if (boardState[j, i] >= 0 && boardState[j, i] != bishopId)
                    {
                        return false;
                    }
                    else if (boardState[j, i] == bishopId)
                    {
                        return true;
                    }
                    i++;
                    j++;
                }
            }
        }
        else
        {
            if (kingPosition.y > y)
            {
                int i = x + 1;
                int j = y + 1;
                // from bishop to king
                while (i <= kingPosition.x && j <= kingPosition.y)
                {
                    if (boardState[j, i] != kingId && boardState[j, i] >= 0)
                    {
                        return false;
                    }
                    else if (boardState[j, i] == kingId)
                    {
                        return true;
                    }
                    i++;
                    j++;
                }
            }
            else
            {
                int i = x + 1;
                int j = y - 1;
                // from bishop to king
                while (i <= kingPosition.x && j >= kingPosition.y)
                {
                    if (boardState[j, i] != kingId && boardState[j, i] >= 0)
                    {
                        return false;
                    }
                    else if (boardState[j, i] == kingId)
                    {
                        return true;
                    }
                    i++;
                    j--;
                }
            }
        }

        return false;
    }
}
