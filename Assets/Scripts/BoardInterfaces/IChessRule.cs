using UnityEngine;
using static chess.enums.ChessEnums;

public interface IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, IBoard board, IChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false);
}
