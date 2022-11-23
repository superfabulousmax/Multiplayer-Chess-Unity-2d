using System.Collections.Generic;
using System.Linq;
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

    public bool CanCastleWithKing(Board board, ChessPiece rook, ChessPiece king, Vector3Int newPosition)
    {
        if(!CanCastle(board, rook))
        {
            return false;
        }
        var rookDeltaX = Mathf.Abs(newPosition.x - rook.TilePosition.x);
        var distToKing = Mathf.Abs(king.TilePosition.x - rook.TilePosition.x);

        if (rookDeltaX == 1 && distToKing == 3)
        {
            return true;
        }
        if (rookDeltaX == 2 && distToKing == 4)
        {
            return true;
        }
        return false;
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
        if (!isSimulation)
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
            boardPosition = new Vector3Int(i, j);
        }
        // right
        i = x + 1;
        j = y;
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
            boardPosition = new Vector3Int(i, j);
        }
        // up
        i = x;
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
            j++;
            boardPosition = new Vector3Int(i, j);
        }
        // down
        i = x;
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
            j--;
            boardPosition = new Vector3Int(i, j);
        }
        return result;
    }

    IReadOnlyList<Vector3Int> ICastleEntity.GetCastleMoves(PlayerColour activeColour, Board board, ChessPiece kingPiece)
    {
        return new List<Vector3Int>();
    }

    public bool CastleWithKing(PlayerColour activeColour, Board board, ChessPiece rook, Vector3Int newKingPosition)
    {
        var rookDeltaX = Mathf.Abs(newKingPosition.x - rook.TilePosition.x);
        if(rookDeltaX == 1)
        {
            rook.SyncDataServerRpc(MoveCount + 1, default, default, default);
            var newRookPosition = new Vector3Int(newKingPosition.x - 1, newKingPosition.y, 0);
            rook.SetTilePositionServerRpc(newRookPosition);
        }
        else if (rookDeltaX == 2)
        {
            rook.SyncDataServerRpc(MoveCount + 1, default, default, default);
            var newRookPosition = new Vector3Int(newKingPosition.x + 1, newKingPosition.y, 0);
            rook.SetTilePositionServerRpc(newRookPosition);
        }
        return false;
    }

}
