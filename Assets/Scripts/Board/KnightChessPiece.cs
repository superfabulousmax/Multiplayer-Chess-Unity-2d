using UnityEngine;

public class KnightChessPiece : IChessRule
{

    IChessRule takePieceRule;

    public KnightChessPiece(IChessRule takePieceRule)
    {
        this.takePieceRule = takePieceRule;
    }
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        takenPiece = false;
        var boardState = board.GetBoardState();
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

        takePieceRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece);

        return true;
    }
}
