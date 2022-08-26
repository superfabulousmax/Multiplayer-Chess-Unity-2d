using System.Collections.Generic;
using UnityEngine;

public class KnightChessPiece : IChessRule, IMoveList
{
    IChessRule takePieceRule;
    IChessRule moveToStopCheckRule;

    public KnightChessPiece(IChessRule takePieceRule, IChessRule moveToStopCheckRule)
    {
        this.takePieceRule = takePieceRule;
        this.moveToStopCheckRule = moveToStopCheckRule;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        takenPiece = false;

        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        var deltaY = Mathf.Abs(newPosition.y - y);
        var deltaX = Mathf.Abs(newPosition.x - x);

        if (deltaX > 2 || deltaX == 0)
        {
            return false;
        }
        if (deltaY > 2 || deltaY == 0)
        {
            return false;
        }
        if (deltaX == 1 && deltaY != 2)
        {
            return false;
        }
        if (deltaX == 2 && deltaY != 1)
        {
            return false;
        }

        // check if move piece to stop check or if moving piece causes check
        var result = moveToStopCheckRule.PossibleMove(activeColour, board, piece, newPosition, out var _);
        if (!result)
        {
            return false;
        }

        takePieceRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece);

        return true;
    }

    public IReadOnlyList<Vector3Int> GetPossibleMoves(PlayerColour activeColour, Board board, ChessPiece piece)
    {
        var result = new List<Vector3Int>();

        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        var boardState = board.GetBoardState();

        List<Vector3Int> possiblePositions = new List<Vector3Int>();
        // left 2 up 1
        possiblePositions.Add(new Vector3Int(x - 2, y + 1));
        // left 2 down 1
        possiblePositions.Add(new Vector3Int(x - 2, y - 1));
        // left 1 up 2
        possiblePositions.Add(new Vector3Int(x - 1, y + 2));
        // left 1 down 2
        possiblePositions.Add(new Vector3Int(x - 1, y - 2));
        // right 2 up 1
        possiblePositions.Add(new Vector3Int(x + 2, y + 1));
        // right 2 down 1
        possiblePositions.Add(new Vector3Int(x + 2, y - 1));
        // right 1 up 2
        possiblePositions.Add(new Vector3Int(x + 1, y + 2));
        // right 1 down 2
        possiblePositions.Add(new Vector3Int(x + 1, y - 2));

        foreach(var position in possiblePositions)
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
            if (boardState[position.y, position.x] >= 0)
            {
                var takenPiece = takePieceRule.PossibleMove(activeColour, board, piece, position, out var _);
                if (takenPiece)
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
