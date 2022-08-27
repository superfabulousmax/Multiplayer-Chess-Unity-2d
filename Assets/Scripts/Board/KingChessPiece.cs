using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KingChessPiece : IChessRule, ICastleEntity, IMoveList
{
    IChessRule moveToStopCheckRule;
    IChessRule castleRule;

    int moveCount;
    public int MoveCount { get => moveCount; set => moveCount = value; }

    public KingChessPiece(IChessRule moveToStopCheckRule, IChessRule castleRule)
    {
        this.moveToStopCheckRule = moveToStopCheckRule;
        this.castleRule = castleRule;
    }

    public bool CanCastle(Board board, ChessPiece kingPiece)
    {
        return moveCount == 0;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        takenPiece = false;

        var boardState = board.GetBoardState();

        var possibleMoves = GetPossibleMoves(activeColour, board, piece);
        if (!possibleMoves.Contains(newPosition))
        {
            return false;
        }

        if (board.CheckPiece(boardState[newPosition.y, newPosition.x], ChessPiece.ChessPieceType.King))
        {
            return false;
        }
        if (boardState[newPosition.y, newPosition.x] > 0 && activeColour == board.GetPieceFromId((uint)boardState[newPosition.y, newPosition.x]).PlayerColour)
        {
            return false;
        }
        if (boardState[newPosition.y, newPosition.x] > 0)
        {
            takenPiece = true;
            Debug.Log("King taking new piece");
        }

        if (!isSimulation)
        {
            moveCount++;
            piece.SyncDataServerRpc(moveCount, default, default, default);
        }

        return true;
    }

    bool TakePiece(PlayerColour activeColour, Board board, int [,] boardState, Vector3Int newPosition)
    {
        if (board.CheckPiece(boardState[newPosition.y, newPosition.x], ChessPiece.ChessPieceType.King))
        {
            return false;
        }
        if (boardState[newPosition.y, newPosition.x] > 0 && activeColour == board.GetPieceFromId((uint)boardState[newPosition.y, newPosition.x]).PlayerColour)
        {
            return false;
        }
        if (boardState[newPosition.y, newPosition.x] > 0)
        {
            return true;
        }
        return true;
    }

    public IReadOnlyList<Vector3Int> GetPossibleMoves(PlayerColour activeColour, Board board, ChessPiece piece)
    {
        var result = new List<Vector3Int>();

        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        var boardState = board.GetBoardState();

        var possiblePositions = new List<Vector3Int>();
        // left 1 up 1
        possiblePositions.Add(new Vector3Int(x - 1, y + 1));
        // left 1 down 1
        possiblePositions.Add(new Vector3Int(x - 1, y - 1));
        // right 1 up 1
        possiblePositions.Add(new Vector3Int(x + 1, y + 1));
        // right 1 down 1
        possiblePositions.Add(new Vector3Int(x + 1, y - 1));
        // right 
        possiblePositions.Add(new Vector3Int(x + 1, y));
        // left
        possiblePositions.Add(new Vector3Int(x - 1, y));
        // up 
        possiblePositions.Add(new Vector3Int(x, y + 1));
        // down
        possiblePositions.Add(new Vector3Int(x, y - 1));
        // castle left
        possiblePositions.Add(new Vector3Int(x - 2, y));
        // castle right
        possiblePositions.Add(new Vector3Int(x + 2, y));

        foreach (var position in possiblePositions)
        {
            if (!board.IsValidPosition(position))
            {
                continue;
            }
            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, position, out var _);
            if (!canMove)
            {
                continue;
            }
            var canCastle = castleRule.PossibleMove(activeColour, board, piece, position, out var _, true);
            if (canCastle)
            {
                result.Add(position);
                continue;
            }
            else
            {
                var dx = Mathf.Abs(x - position.x);
                var dy = Mathf.Abs(y - position.y);
                if (dx == 2 && dy == 0)
                {
                    continue;
                }
            }
            if (boardState[position.y, position.x] >= 0)
            {
                var couldTakePiece = TakePiece(activeColour, board, boardState, position);
                if (couldTakePiece)
                {
                    result.Add(position);
                }
                continue;
            }
            else
            {
                result.Add(position);
            }
        }
        return result;
    }
}
