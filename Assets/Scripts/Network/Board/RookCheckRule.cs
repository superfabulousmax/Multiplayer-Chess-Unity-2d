using System.Linq;
using UnityEngine;
using static chess.enums.ChessEnums;
public class RookCheckRule : ICheckRule
{
    public bool PossibleCheck(IBoard board, int[,] boardState, IChessPiece rook, Vector3Int position, out IChessPiece king)
    {
        var y = position.y;
        var x = position.x;
        // opposite king
        king = board.GetPiecesWith(GetOppositeColour(rook.PlayerColour), ChessPieceType.King).First();
        var kingId = (uint)king.PieceId;
        var kingPosition = board.GetIdPosition(kingId);

        if (kingPosition.y > y && kingPosition.x > x)
            return false;
        if (kingPosition.y < y && kingPosition.x < x)
            return false;
        if (kingPosition.y == y && kingPosition.x == x)
            return false;

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

                for (int i = kingPosition.x + 1 ; i <= x; i++)
                {
                    if (boardState[y, i] != rook.PieceId && boardState[y, i] >= 0)
                    {
                        return false;
                    }
                    else if (boardState[y, i] == rook.PieceId)
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
