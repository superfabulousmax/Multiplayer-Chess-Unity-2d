using UnityEngine;

public interface ICheckRule
{
    public bool PossibleCheck(IBoard board, int[,] boardState, IChessPiece piece, Vector3Int position, out IChessPiece king);
}
