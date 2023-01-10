using UnityEngine;
using System.Collections.Generic;
using static chess.enums.ChessEnums;

public interface ICastleEntity
{
    public bool CanCastle(IBoard board, IChessPiece piece);
    public IReadOnlyList<Vector3Int> GetCastleMoves(PlayerColour activeColour, IBoard board, IChessPiece kingPiece);

    public bool CastleWithKing(PlayerColour activeColour, IBoard board, IChessPiece kingPiece, Vector3Int position);
}
