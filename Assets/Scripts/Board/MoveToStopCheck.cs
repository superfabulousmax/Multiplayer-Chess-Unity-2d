using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToStopCheck : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, out bool checkedKing)
    {
        checkedKing = false;
        takenPiece = false;

        var boardState = board.GetBoardState();

        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        if (board.IsInCheck(out var king))
        {
            if (king.PlayerColour == piece.PlayerColour)
            {
                checkedKing = true;
                boardState[y, x] = -1;
                boardState[newPosition.y, newPosition.x] = (int)piece.NetworkObjectId;
                if (board.IsInCheck(boardState))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
