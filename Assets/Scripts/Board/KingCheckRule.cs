using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingCheckRule : ICheckRule
{
    public bool PossibleCheck(Board board, int[,] boardState, ChessPiece kingPiece, Vector3Int position, out ChessPiece otherKing)
    {
        position = board.GetIdPosition((uint)kingPiece.NetworkObjectId, boardState);
        var y = position.y;
        var x = position.x;
        otherKing = board.GetOppositeKing(kingPiece.PlayerColour);
        var kingId = (uint)otherKing.NetworkObjectId;
        var kingPosition = board.GetIdPosition(kingId, boardState);

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

        if (xDiff == 1 || yDiff == 1)
        {
            return true;
        }

        return false;
    }
}
