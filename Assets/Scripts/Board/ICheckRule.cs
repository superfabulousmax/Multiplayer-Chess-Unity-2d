using UnityEngine;

public interface ICheckRule
{
    public bool PossibleCheck(Board board, int[,] boardState, ChessPiece piece, Vector3Int position, out ChessPiece king);
}
