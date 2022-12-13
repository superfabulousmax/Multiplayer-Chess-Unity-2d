using UnityEngine;
using System.Collections.Generic;
using static chess.enums.ChessEnums;

public interface IMoveList
{
    public IReadOnlyList<Vector3Int> GetPossibleMoves(PlayerColour activeColour, Board board, ChessPiece piece);
}
