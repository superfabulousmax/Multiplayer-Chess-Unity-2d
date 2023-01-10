using UnityEngine;
using static chess.enums.ChessEnums;

public interface IEnPassantChessRule
{
    public bool CheckEnPassant(int direction, PlayerColour activeColour, IBoard board, IChessPiece piece, IChessPiece lastMovedPawn, Vector3Int newPosition, out Vector3Int takenPiecePosition);
}
