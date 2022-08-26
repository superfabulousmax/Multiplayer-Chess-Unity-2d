using UnityEngine;
using System.Collections.Generic;

public interface IMoveList
{
    public IReadOnlyList<Vector3Int> GetPossibleMoves(PlayerColour activeColour, Board board, ChessPiece piece);
}
