using System.Linq;
using UnityEngine;
using static chess.enums.ChessEnums;

public class BishopCheckRule : ICheckRule
{
    public bool PossibleCheck(IBoard board, int [,] boardState, IChessPiece bishop, Vector3Int position, out IChessPiece king)
    {
        var y = position.y;
        var x = position.x;
        // should get king from the board state not the board
        king = board.GetPiecesWith(GetOppositeColour(bishop.PlayerColour), ChessPieceType.King).First();
        var kingId = (uint)king.PieceId;
        var kingPosition = Vector3Int.zero;

        for (var j = 0; j < GameConstants.BoardLengthDimension; j++)
        {
            for (var i = 0; i < GameConstants.BoardLengthDimension; i++)
            {
                if (boardState[j, i] == kingId)
                {
                    kingPosition.y = j;
                    kingPosition.x = i;
                    break;
                }
            }
        }


        if (kingPosition.y == y)
            return false;
        if (kingPosition.x == x)
            return false;

        var xDiff = Mathf.Abs(kingPosition.x - x);
        var yDiff = Mathf.Abs(kingPosition.y - y);

        if (xDiff != yDiff)
            return false;

        var bishopId = bishop.PieceId;

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
