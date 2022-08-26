using System.Collections.Generic;
using UnityEngine;

public class BishopChessPiece : IChessRule, IMoveList
{
    IChessRule takePieceRule;
    IChessRule moveToStopCheckRule;
    public BishopChessPiece(IChessRule takePieceRule, IChessRule moveToStopCheckRule)
    {
        this.takePieceRule = takePieceRule;
        this.moveToStopCheckRule = moveToStopCheckRule;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        takenPiece = false;

        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        if (newPosition.y == y)
            return false;
        if (newPosition.x == x)
            return false;

        int xDiff = Mathf.Abs(newPosition.x - x);
        int yDiff = Mathf.Abs(newPosition.y - y);

        if (xDiff != yDiff)
            return false;

        var boardState = board.GetBoardState();

        if (newPosition.x < x)
        {
            if (newPosition.y > y)
            {
                int i = newPosition.x + 1;
                int j = newPosition.y - 1;

                while (i != x && j != y)
                {
                    if (boardState[j, i] >= 0)
                        return false;
                    i++;
                    j--;
                }
            }
            else
            {
                int i = newPosition.x + 1;
                int j = newPosition.y + 1;
                while (i != x && j != y)
                {
                    if (boardState[j, i] >= 0)
                        return false;
                    i++;
                    j++;
                }
            }
        }
        else
        {
            if (newPosition.y > y)
            {
                int i = x + 1;
                int j = y + 1;

                while (i != newPosition.x && j != newPosition.y)
                {
                    if (boardState[j, i] >= 0)
                        return false;
                    i++;
                    j++;
                }
            }
            else
            {
                int i = x + 1;
                int j = y - 1;
                while (i != newPosition.x && j != newPosition.y)
                {
                    if (boardState[j, i] >= 0)
                        return false;
                    i++;
                    j--;
                }
            }
        }

        takePieceRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece);
        // check if move piece to stop check or if moving piece causes check
        var result = moveToStopCheckRule.PossibleMove(activeColour, board, piece, newPosition, out var _);
        if (!result)
        {
            takenPiece = false;
            return false;
        }

        return true;
    }

    public IReadOnlyList<Vector3Int> GetPossibleMoves(PlayerColour activeColour, Board board, ChessPiece piece)
    {
        var result = new List<Vector3Int>();
        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        var boardState = board.GetBoardState();
        int i, j;
        Vector3Int boardPosition;

        // left and down
        i = x - 1;
        j = y - 1;
        boardPosition = new Vector3Int(i, j);
        while (board.IsValidPosition(boardPosition))
        {
            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
            if (!canMove)
            {
                break;
            }
            if (boardState[j, i] >= 0)
            {
                var takenPiece = takePieceRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
                if (takenPiece)
                {
                    result.Add(boardPosition);
                }
                break;
            }
            else
            {
                result.Add(boardPosition);
            }
            i--;
            j--;
            boardPosition = new Vector3Int(i, j);
        }
        // left and up
        i = x - 1;
        j = y + 1;
        boardPosition = new Vector3Int(i, j);
        while (board.IsValidPosition(boardPosition))
        {
            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
            if (!canMove)
            {
                break;
            }
            if (boardState[j, i] >= 0)
            {
                var takenPiece = takePieceRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
                if (takenPiece)
                {
                    result.Add(boardPosition);
                }
                break;
            }
            else
            {
                result.Add(boardPosition);
            }
            i--;
            j++;
            boardPosition = new Vector3Int(i, j);
        }
        // right and down
        i = x + 1;
        j = y - 1;
        boardPosition = new Vector3Int(i, j);
        while (board.IsValidPosition(boardPosition))
        {
            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
            if (!canMove)
            {
                break;
            }
            if (boardState[j, i] >= 0)
            {
                var takenPiece = takePieceRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
                if (takenPiece)
                {
                    result.Add(boardPosition);
                }
                break;
            }
            else
            {
                result.Add(boardPosition);
            }
            i++;
            j--;
            boardPosition = new Vector3Int(i, j);
        }
        // right and up
        i = x + 1;
        j = y + 1;
        boardPosition = new Vector3Int(i, j);
        while (board.IsValidPosition(boardPosition))
        {
            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
            if(!canMove)
            {
                break;
            }
            if (boardState[j, i] >= 0)
            {
                var takenPiece = takePieceRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
                if(takenPiece)
                {
                    result.Add(boardPosition);
                }
                break;
            }
            else
            {
                result.Add(boardPosition);
            }
            i++;
            j++;
            boardPosition = new Vector3Int(i, j);
        }
        return result;
    }
}
