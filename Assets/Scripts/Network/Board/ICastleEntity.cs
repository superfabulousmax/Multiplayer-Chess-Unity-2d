using UnityEngine;
using System.Collections.Generic;
using static chess.enums.ChessEnums;
public interface ICastleEntity
{
    public bool CanCastle(Board board, ChessPiece piece);
    public IReadOnlyList<Vector3Int> GetCastleMoves(PlayerColour activeColour, Board board, ChessPiece kingPiece);

    public bool CastleWithKing(PlayerColour activeColour, Board board, ChessPiece kingPiece, Vector3Int position);
}
