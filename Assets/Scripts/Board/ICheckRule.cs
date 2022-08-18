using UnityEngine;

public interface ICheckRule
{
    public bool PossibleCheck(Board board, ChessPiece piece, Vector3Int position, out ChessPiece king);
}
