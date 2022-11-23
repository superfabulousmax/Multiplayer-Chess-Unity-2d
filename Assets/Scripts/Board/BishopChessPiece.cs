using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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

        var possibleMoves = GetPossibleMoves(activeColour, board, piece);
        if (!possibleMoves.Contains(newPosition))
        {
            takenPiece = false;
            return false;
        }
        takePieceRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece);
        return true;
    }

    public IReadOnlyList<Vector3Int> GetPossibleMoves(PlayerColour activeColour, Board board, ChessPiece piece)
    {
        var result = new List<Vector3Int>();
        var x = piece.TilePosition.x;
        var y = piece.TilePosition.y;

        var boardState = board.GetBoardState();

        // left and down
        var i = x - 1;
        var j = y - 1;
        var boardPosition = new Vector3Int(i, j);

        while (board.IsValidPosition(boardPosition))
        {
            if (boardState[j, i] >= 0)
            {
                var pieceAtIJ = board.GetPieceAtPosition(boardPosition);
                if (pieceAtIJ.PlayerColour == piece.PlayerColour)
                {
                    break;
                }
            }
            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
            if (!canMove)
            {
                i--;
                j--;
                boardPosition = new Vector3Int(i, j);
                continue;
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
            if (boardState[j, i] >= 0)
            {
                var pieceAtIJ = board.GetPieceAtPosition(boardPosition);
                if (pieceAtIJ.PlayerColour == piece.PlayerColour)
                {
                    break;
                }
            }
            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
            if (!canMove)
            {
                i--;
                j++;
                boardPosition = new Vector3Int(i, j);
                continue;
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
            if (boardState[j, i] >= 0)
            {
                var pieceAtIJ = board.GetPieceAtPosition(boardPosition);
                if (pieceAtIJ.PlayerColour == piece.PlayerColour)
                {
                    break;
                }
            }
            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
            if (!canMove)
            {
                i++;
                j--;
                boardPosition = new Vector3Int(i, j);
                continue;
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
            if (boardState[j, i] >= 0)
            {
                var pieceAtIJ = board.GetPieceAtPosition(boardPosition);
                if (pieceAtIJ.PlayerColour == piece.PlayerColour)
                {
                    break;
                }
            }
            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, boardPosition, out var _);
            if(!canMove)
            {
                i++;
                j++;
                boardPosition = new Vector3Int(i, j);
                continue;
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
