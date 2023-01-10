using System.Linq;
using UnityEngine;
using static chess.enums.ChessEnums;

public class KnightCheckRule : ICheckRule
{
    public bool PossibleCheck(IBoard board, int[,] boardState, IChessPiece knight, Vector3Int position, out IChessPiece king)
    {
        var y = position.y;
        var x = position.x;

        king = board.GetPiecesWith(GetOppositeColour(knight.PlayerColour), ChessPieceType.King).First();
        var kingId = (uint)king.PieceId;
        var kingPosition = board.GetIdPosition(kingId);

        var deltaY = Mathf.Abs(kingPosition.y - y);
        var deltaX = Mathf.Abs(kingPosition.x - x);

        if (deltaX > 2 || deltaX == 0)
        {
            return false;
        }
        if (deltaY > 2 || deltaY == 0)
        {
            return false;
        }
        if (deltaX == 1)
        {
            if (deltaY != 2)
            {
                return false;
            }
        }
        if (deltaX == 2)
        {
            if (deltaY != 1)
            {
                return false;
            }
        }

        return true;
    }
}
