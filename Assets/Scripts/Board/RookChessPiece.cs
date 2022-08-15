using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RookChessPiece : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        takenPiece = false;
        return true;
    }
}
