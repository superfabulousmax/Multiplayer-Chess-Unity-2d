using System.Linq;
using UnityEngine;
using static chess.enums.ChessEnums;

public class KnightCheckRule : ICheckRule
{
    public bool PossibleCheck(IBoard board, int[,] boardState, IChessPiece knight, Vector3Int position, out IChessPiece king)
    {
        var y = position.y;
        var x = position.x;

        // should get king from the board state not the board
        king = board.GetPiecesWith(GetOppositeColour(knight.PlayerColour), ChessPieceType.King).First();
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
