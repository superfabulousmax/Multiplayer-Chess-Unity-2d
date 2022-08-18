using UnityEngine;

public class KnightChessPiece : IChessRule
{
    IChessRule takePieceRule;
    IChessRule moveToStopCheckRule;

    public KnightChessPiece(IChessRule takePieceRule, IChessRule moveToStopCheckRule)
    {
        this.takePieceRule = takePieceRule;
        this.moveToStopCheckRule = moveToStopCheckRule;
    }
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, out bool checkedKing)
    {
        takenPiece = false;
        checkedKing = false;

        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        var deltaY = Mathf.Abs(newPosition.y - y);
        var deltaX = Mathf.Abs(newPosition.x - x);

        if (deltaX > 2 || deltaX == 0)
            return false;
        if (deltaY > 2 || deltaY == 0)
            return false;
        if (deltaX == 1)
        {
            if (deltaY != 2)
                return false;
        }
        if (deltaX == 2)
        {
            if (deltaY != 1)
                return false;
        }

        takePieceRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece, out var _);

        // check if move piece to stop check
        if (!moveToStopCheckRule.PossibleMove(activeColour, board, piece, newPosition, out var _, out var isCheckedKing))
        {
            if (isCheckedKing)
            {
                takenPiece = false;
            }
            return false;
        }


        return true;
    }
}
