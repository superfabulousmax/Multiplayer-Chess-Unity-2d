using System.Linq;
using UnityEngine;
using static chess.enums.ChessEnums;

public class PawnCheckRule : ICheckRule
{
    public bool PossibleCheck(IBoard board, int[,] boardState, IChessPiece pawn, Vector3Int position, out IChessPiece king)
    {
        var y = position.y;
        var x = position.x;
        // should get king from the board state not the board
        king = board.GetPiecesWith(GetOppositeColour(pawn.PlayerColour), ChessPieceType.King).First();
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
