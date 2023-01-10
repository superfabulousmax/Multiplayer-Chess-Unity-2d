using UnityEngine;
using static chess.enums.ChessEnums;

public interface IChessPiece
{
    public int PieceId { get; }
    public ChessPieceType PieceType { get; }
    public char Symbol { get; }
    public PlayerColour PlayerColour { get; }
    public Vector3Int Position { get; }
    public Transform PieceTransform { get; }
    public IChessRule PieceRuleBehaviour { get; }

    public ICheckRule CheckRuleBehaviour { get; }

    public IMoveList MoveList { get; }

    public void SetPosition(Vector3Int position);
    public void ChangePieceTo(ChessPieceType chessPieceType);
    public void SyncData(int moveCount, bool isFirstMove, bool firstMoveTwo, uint lastMovedPawnId);
}
