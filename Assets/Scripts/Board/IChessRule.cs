using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece);
}
