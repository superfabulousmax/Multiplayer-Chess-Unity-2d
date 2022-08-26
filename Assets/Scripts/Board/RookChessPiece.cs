using System.Collections.Generic;
using UnityEngine;

public class RookChessPiece : IChessRule, ICastleEntity, IMoveList
{
    IChessRule takePieceRule;
    IChessRule moveToStopCheckRule;

    int moveCount;
    public int MoveCount { get => moveCount; set => moveCount = value; }

    public RookChessPiece(IChessRule takePieceRule, IChessRule moveToStopCheckRule)
    {
        this.takePieceRule = takePieceRule;
        this.moveToStopCheckRule = moveToStopCheckRule;
    }

    public bool CanCastle(Board board, ChessPiece rook)
    {
        if (moveCount > 0)
        {
            return false;
        }

        var castlingRights = board.PlacementSystem.StartingSetup.castlingRights;
        var king = board.GetKingForColour(rook.PlayerColour);
        var kingCastleEntity = king.ChessRuleBehaviour as ICastleEntity;

        if (kingCastleEntity == null)
        {
            return false;
        }

        if (!kingCastleEntity.CanCastle(board, king))
        {
            return false;
        }

        var distToKing = Mathf.Abs(king.TilePosition.x - rook.TilePosition.x);

        foreach(var letter in castlingRights)
        {
            if (rook.PlayerColour == PlayerColour.PlayerOne)
            {
                if(char.IsUpper(letter))
                {
                    if (distToKing == 3 && letter == 'K')
                    {
                        return true;
                    }
                    else if(distToKing == 4 && letter == 'Q')
                    {
                        return true;
                    }
                }
            }
            if (rook.PlayerColour == PlayerColour.PlayerTwo)
            {
                if (char.IsLower(letter))
                {
                    if (distToKing == 3 && letter == 'k')
                    {
                        return true;
                    }
                    else if (distToKing == 4 && letter == 'q')
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        takenPiece = false;

        var boardState = board.GetBoardState();
        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;
        if (newPosition.y > y && newPosition.x > x)
            return false;
        if (newPosition.y < y && newPosition.x < x)
            return false;
        if (newPosition.y == y && newPosition.x == x)
            return false;

        if (newPosition.x == x)
        {
            // moving up or down
            // check piece in the way
            if (newPosition.y > y)
            {
                for (int i = y + 1; i < newPosition.y; i++)
                {
                    if (boardState[i, x] >= 0)
                        return false;
                }
            }
            else if (newPosition.y < y)
            {
                for (int i = y - 1; i > newPosition.y; i--)
                {
                    if (boardState[i, x] >= 0)
                        return false;
                }
            }

        }
        else
        {
            // moving left or right
            if (y != newPosition.y)
            {
                return false;
            }
            if (newPosition.x < x)
            {
                // moving left

                for (int i = newPosition.x + 1; i < x; i++)
                {
                    if (boardState[y, i] >= 0)
                        return false;
                }
            }
            else
            {
                // moving right
                for (int i = x + 1; i < newPosition.x; i++)
                {
                    if (boardState[y, i] >= 0)
                        return false;
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

        if(!isSimulation)
        {
            moveCount++;
            piece.SyncDataServerRpc(moveCount, default, default, default);
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

        // left
        i = x - 1;
        j = y;
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
            boardPosition = new Vector3Int(i, j);
        }
        // right
        i = x + 1;
        j = y;
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
            boardPosition = new Vector3Int(i, j);
        }
        // up
        i = x;
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
            j++;
            boardPosition = new Vector3Int(i, j);
        }
        // down
        i = x;
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
            j--;
            boardPosition = new Vector3Int(i, j);
        }
        return result;
    }
}
