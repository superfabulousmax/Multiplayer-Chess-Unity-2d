using UnityEngine;
using static ChessPiece;

public class CastleRule : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece kingPiece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        takenPiece = false;

        if (kingPiece.ChessRuleBehaviour is ICastleEntity castleRule)
        {
            if(!castleRule.CanCastle(board, kingPiece))
            {
                return false;
            }
        }

        var boardState = board.GetBoardState();
        var y = kingPiece.TilePosition.y;
        var x = kingPiece.TilePosition.x;

        var dy = Mathf.Abs(newPosition.y - y);
        var dx = Mathf.Abs(newPosition.x - x);
        if (dx != 2 || dy != 0)
        {
            return false;
        }

        if (kingPiece.PlayerColour == PlayerColour.PlayerOne)
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
            if (kingPiece == checkedKing)
            {
                return false;
            }
        }

        var smallestX = Mathf.Min(newPosition.x, x);
        var biggestX = Mathf.Max(newPosition.x, x);
        for (int i = smallestX; i <= biggestX; ++i)
        {
            if (i == x)
            {
                continue;
            }
            var attacked = board.CheckSpaceAttacked(activeColour, new Vector3Int(i, y, 0));
            if (attacked)
            {
                return false;
            }
        }

        return MoveRooks(board, kingPiece, newPosition, isSimulation);
    }

    public bool MoveRooks(Board board, ChessPiece piece, Vector3Int newPosition, bool isSimulation = false)
    {
        var rooks = board.GetPieceWith(piece.PlayerColour, ChessPieceType.Rook);
        foreach (var rook in rooks)
        {
            if (rook.ChessRuleBehaviour is RookChessPiece rookPiece)
            {
                var castleEntity = rookPiece as ICastleEntity;
                if (castleEntity.CanCastle(board, rook) == false)
                {
                    continue;
                }
                var rookDeltaX = Mathf.Abs(newPosition.x - rook.TilePosition.x);
                var distToKing = Mathf.Abs(piece.TilePosition.x - rook.TilePosition.x);

                if (rookDeltaX == 1 && distToKing == 3)
                {
                    if(!isSimulation)
                    {
                        rook.SyncDataServerRpc(rookPiece.MoveCount + 1, default, default, default);
                        var newRookPosition = new Vector3Int(newPosition.x - 1, newPosition.y, 0);
                        rook.SetTilePositionServerRpc(newRookPosition);
                    }
                    return true;
                }
                if (rookDeltaX == 2 && distToKing == 4)
                {
                    if(!isSimulation)
                    {
                        rook.SyncDataServerRpc(rookPiece.MoveCount + 1, default, default, default);
                        var newRookPosition = new Vector3Int(newPosition.x + 1, newPosition.y, 0);
                        rook.SetTilePositionServerRpc(newRookPosition);
                    }
                    return true;
                }
            }
        }
        return false;
    }
}
