using UnityEngine;

public class KingChessPiece : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        takenPiece = false;
        var boardState = board.GetBoardState();
        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        var deltaY = Mathf.Abs(newPosition.y - y);
        var deltaX = Mathf.Abs(newPosition.x - x);

        if (deltaX == 0 && deltaY == 0)
            return false;
        if (deltaX > 1)
            return false;
        if (deltaY > 1)
            return false;

        if (board.CheckPiece(boardState[newPosition.y, newPosition.x], ChessPiece.ChessPieceType.King))
        {
            return false;
        }
        if (boardState[newPosition.y, newPosition.x] > 0 && activeColour == board.GetPieceFromId((uint)boardState[newPosition.y, newPosition.x]).PlayerColour)
        {
            return false;
        }
        if (boardState[newPosition.y, newPosition.x] > 0)
        {
            takenPiece = true;
            Debug.Log("King taking new piece");
        }

        return true;
    }
}
