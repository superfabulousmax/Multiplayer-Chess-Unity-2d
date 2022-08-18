using UnityEngine;
using static ChessPiece;

public class TakePieceRule : IChessRule
{
    ChessPieceType chessPieceType;
    public TakePieceRule(ChessPieceType chessPieceType)
    {
        this.chessPieceType = chessPieceType;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        takenPiece = false;

        var boardState = board.GetBoardState();

        if (board.CheckPiece(boardState[newPosition.y, newPosition.x], ChessPieceType.King))
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
            Debug.Log($"{chessPieceType} taking new piece");
        }

        return true;
    }
}
