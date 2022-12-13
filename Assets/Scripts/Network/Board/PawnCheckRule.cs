using UnityEngine;
using static chess.enums.ChessEnums;

public class PawnCheckRule : ICheckRule
{
    public bool PossibleCheck(Board board, int[,] boardState, ChessPiece pawn, Vector3Int position, out ChessPiece king)
    {
        var y = position.y;
        var x = position.x;

        king = board.GetOppositeKing(pawn.PlayerColour);
        var kingId = (uint)king.NetworkObjectId;
        var kingPosition = board.GetIdPosition(kingId, boardState);

        var yDiff = Mathf.Abs(kingPosition.y - y);
        var xDiff = Mathf.Abs(kingPosition.x - x);

        if (yDiff != 1)
        {
            return false;
        }
        if (xDiff != 1)
        {
            return false;
        }


        if (pawn.PlayerColour == PlayerColour.PlayerOne)
        {
            if (kingPosition.y <= y)
            {
                return false;
            }
        }
        else
        {
            if (kingPosition.y >= y)
            {
                return false;
            }
        }

        return true;
    }
}
