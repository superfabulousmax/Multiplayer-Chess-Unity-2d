using UnityEngine;
using System.Collections.Generic;
using static chess.enums.ChessEnums;

public class CastleMoves : IMoveList
{
    public IReadOnlyList<Vector3Int> GetPossibleMoves(PlayerColour activeColour, IBoard board, IChessPiece kingPiece)
    {
        var result = new List<Vector3Int>();

        if (kingPiece.PieceRuleBehaviour is ICastleEntity castleRule)
        {
            if (!castleRule.CanCastle(board, kingPiece))
            {
                return result;
            }
        }
        if (board.IsInCheck(out var checkedKing))
        {
            if (kingPiece == checkedKing)
            {
                return result;
            }
        }

        var y = kingPiece.Position.y;
        var x = kingPiece.Position.x;

        var possiblePositions = new List<Vector3Int>();
        // castle left
        possiblePositions.Add(new Vector3Int(x - 2, y));
        // castle right
        possiblePositions.Add(new Vector3Int(x + 2, y));
        // TODO
        var positionsToValidate = new List<Vector3Int>();
        foreach (var position in possiblePositions)
        {
            if (CanCastleAnyRook(board, kingPiece, position))
            {
                positionsToValidate.Add(position);
            }
        }
       

        var boardState = board.GetBoardState();
        foreach(var position in positionsToValidate)
        {
            var smallestX = Mathf.Min(position.x, x);
            var biggestX = Mathf.Max(position.x, x);
            if (biggestX == x)
            {
                // check one more past position clicked
                smallestX--;
            }
            var pieceInWay = false;
            for (var i = smallestX; i <= biggestX; ++i)
            {
                if (boardState[y, i] >= 0
                    && !board.CheckPiece(boardState[y, i], ChessPieceType.King)
                    && !board.CheckPiece(boardState[y, i], ChessPieceType.Rook))
                {
                    pieceInWay = true;
                    break;
                }
            }

            if (pieceInWay)
            {
                continue;
            }

            smallestX = Mathf.Min(position.x, x);
            biggestX = Mathf.Max(position.x, x);
            var attacked = false;
            for (var i = smallestX; i <= biggestX; ++i)
            {
                if (i == x)
                {
                    continue;
                }
                attacked = board.CheckSpaceAttacked(kingPiece.PieceId, activeColour, new Vector3Int(i, y, 0));
                if (attacked)
                {
                    break;
                }
            }

            if (!attacked)
            {
                result.Add(position);
            }

        }

        return result;
    }
    public bool CanCastleAnyRook(IBoard board, IChessPiece piece, Vector3Int newPosition)
    {
        var rooks = board.GetPiecesWith(piece.PlayerColour, ChessPieceType.Rook);
        foreach (var rook in rooks)
        {
            if (rook.PieceRuleBehaviour is RookChessPiece rookPiece)
            {
                if(rookPiece.CanCastleWithKing(board, rook, piece, newPosition) == false)
                {
                    continue;
                }
                return true;
            }
        }
        return false;
    }

}
