using UnityEngine;
using static chess.enums.ChessEnums;

public interface IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false);
}
