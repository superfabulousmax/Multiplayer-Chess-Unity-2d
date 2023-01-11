using System.Linq;
using UnityEngine;
using static chess.enums.ChessEnums;

public class KingCheckRule : ICheckRule
{
    public bool PossibleCheck(IBoard board, int[,] boardState, IChessPiece kingPiece, Vector3Int position, out IChessPiece otherKing)
    {
        position = board.GetIdPosition((uint)kingPiece.PieceId);
        var y = position.y;
        var x = position.x;

        otherKing = board.GetPiecesWith(GetOppositeColour(kingPiece.PlayerColour), ChessPieceType.King).First();
        var kingId = (uint)otherKing.PieceId;
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

        var xDiff = Mathf.Abs(kingPosition.x - x);
        var yDiff = Mathf.Abs(kingPosition.y - y);

        if (xDiff > 1)
        {
            return false;
        }
        if (yDiff > 1)
        {
            return false;
        }

        if (xDiff == 1 ||
            yDiff == 1)
        {
            return true;
        }

        return false;
    }
}
