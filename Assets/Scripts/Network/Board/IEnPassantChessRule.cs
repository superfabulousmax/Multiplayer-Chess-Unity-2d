using UnityEngine;
using static chess.enums.ChessEnums;

public interface IEnPassantChessRule
{
    public bool CheckEnPassant(int direction, PlayerColour activeColour, Board board, ChessPiece piece, ChessPiece lastMovedPawn, Vector3Int newPosition, out Vector3Int takenPiecePosition);
}
