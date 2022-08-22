using UnityEngine;
using static ChessPiece;

public class CastleRule : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {

        takenPiece = false;

        if (piece.ChessRuleBehaviour is ICastleEntity castleRule)
        {
            if(!castleRule.CanCastle(board, piece))
            {
                return false;
            }
        }

        var boardState = board.GetBoardState();
        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        var deltaY = Mathf.Abs(newPosition.y - y);
        var deltaX = Mathf.Abs(newPosition.x - x);

        if (deltaX != 2)
        {
            return false;
        }
        if (deltaY != 0)
        {
            return false;
        }

        if (piece.PlayerColour == PlayerColour.PlayerOne)
        {
            if (y != GameConstants.PlayerOneKingStartY)
            {
                return false;
            }
        }
        else
        {
            if (y != GameConstants.PlayerTwoKingStartY)
            {
                return false;
            }
        }

        if (newPosition.x > x)
        {
            for (int i = x + 1; i <= newPosition.x; ++i)
            {
                if (boardState[y, i] >= 0)
                {
                    return false;
                }
            }
        }
        else
        {
            for (int i = newPosition.x; i < x; ++i)
            {
                if (boardState[y, i] >= 0)
                {
                    return false;
                }
            }
        }

        if (board.IsInCheck(out var checkedKing))
        {
            if (piece == checkedKing)
            {
                return false;
            }
        }

        var rooks = board.GetPieceWith(piece.PlayerColour, ChessPieceType.Rook);
        if (rooks.Count == 0)
        {
            return false;
        }

        foreach (var rook in rooks)
        {
            // todo improve this
            if (rook.ChessRuleBehaviour is RookChessPiece rookPiece)
            {
                var castleEntity = rookPiece as ICastleEntity;
                if (castleEntity.CanCastle(board, rook) == false)
                {
                    continue;
                }
                var rookDeltaX = Mathf.Abs (newPosition.x - rook.TilePosition.x);
                var distToKing = Mathf.Abs(piece.TilePosition.x - rook.TilePosition.x);

                if (rookDeltaX == 1 && distToKing == 3)
                {
                    rook.SyncDataServerRpc(rookPiece.MoveCount + 1, default, default, default);
                    var newRookPosition = new Vector3Int(newPosition.x - 1, newPosition.y, 0);
                    rook.SetTilePositionServerRpc(newRookPosition);
                    return true;
                }
                if (rookDeltaX == 2 && distToKing == 4)
                {
                    rook.SyncDataServerRpc(rookPiece.MoveCount + 1, default, default, default);
                    var newRookPosition = new Vector3Int(newPosition.x + 1, newPosition.y, 0);
                    rook.SetTilePositionServerRpc(newRookPosition);
                    return true;
                }
            }
        }
        return false;
    }
}
