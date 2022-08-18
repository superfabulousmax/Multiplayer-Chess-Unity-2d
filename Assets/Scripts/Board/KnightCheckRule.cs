using UnityEngine;

public class KnightCheckRule : ICheckRule
{
    public bool PossibleCheck(Board board, ChessPiece knight, Vector3Int position, out ChessPiece king)
    {
        var y = position.y;
        var x = position.x;

        king = board.GetOppositeKing(knight.PlayerColour);

        var kingPosition = king.TilePosition;
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
