using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static chess.enums.ChessEnums;

public class ActivePlayerInput : IPlayerInput
{
    Board board;
    BoardTileHighlighter tileHighlighter;
    ChessPiece selectedChessPiece;
    ChessPiece lastMovedPiece;
    Action onFinish;

    private Color highlightColour;
    private Color clearColour;
    private Color checkedColour;
    private Color possibleMoveColour;

    List<Vector3Int> possibleMoves;

    public ActivePlayerInput(Board board, Action onFinish)
    {
        this.board = board;
        this.tileHighlighter = board.TileHighlighter;
        this.onFinish = onFinish;
        // perwinkle
        this.highlightColour = new Color(204 / 255.0f, 204 / 255.0f, 255 / 255.0f, 200 / 255.0f);
        // mint
        this.possibleMoveColour = new Color(152 / 255.0f, 251 / 255.0f, 152 / 255.0f, 200 / 255.0f);
        this.clearColour = Color.clear;
        this.checkedColour = Color.red;
        possibleMoves = new List<Vector3Int>();
    }

    public void HandleInput(int id, PlayerColour activeColour, PlayerColour currentColour, bool isOwner)
    {
        if (!isOwner || (activeColour != currentColour))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (GetChessPiece(out var chessPiece))
            {
                if(chessPiece.PlayerColour == activeColour)
                {
                    // clear previous selected
                    if (selectedChessPiece != null)
                    {
                        tileHighlighter.SetTileColour(selectedChessPiece.TilePosition, clearColour);
                        TogglePossibleMoves(clearColour);
                    }

                    selectedChessPiece = chessPiece;
                    tileHighlighter.SetTileColour(selectedChessPiece.TilePosition, highlightColour);
                    if (selectedChessPiece.TilePosition != board.CheckedPos)
                    {
                        tileHighlighter.SetTileColour(board.CheckedPos, checkedColour);
                    }

                    if (selectedChessPiece?.MoveListGenerator != null)
                    {
                        possibleMoves = selectedChessPiece.MoveListGenerator.GetPossibleMoves(activeColour, board, selectedChessPiece).ToList();
                        if (selectedChessPiece.ChessRuleBehaviour is ICastleEntity castleInfo)
                        {
                            var castleMoves = castleInfo.GetCastleMoves(activeColour, board, selectedChessPiece);
                            possibleMoves.AddRange(castleMoves);
                        }
                        TogglePossibleMoves(possibleMoveColour);
                    }

                    return;
                }
            }
            if (selectedChessPiece == null)
            {
                return;
            }

            var tilePosition = board.GetTileAtMousePosition(Input.mousePosition);
            if(selectedChessPiece.TilePosition == tilePosition)
            {
                return;
            }

            var validateMove = board.ValidateMove(activeColour, selectedChessPiece, tilePosition, out bool isPieceTaken);
            var validateCastle = ValidateCastle(activeColour, selectedChessPiece, tilePosition);
            if (validateMove || validateCastle)
            {
                tileHighlighter.SetTileColour(tilePosition, highlightColour);
                tileHighlighter.StartWaitThenSetColour(tilePosition, clearColour);
                tileHighlighter.StartWaitThenSetColour(selectedChessPiece.TilePosition, clearColour);

                if (isPieceTaken)
                {
                    board.TakePieceServerRpc(selectedChessPiece, tilePosition);
                }
                else
                {
                    var oldX = selectedChessPiece.TilePosition.x;
                    selectedChessPiece.SetTilePositionServerRpc(tilePosition);
                    if (validateCastle)
                    {
                        // move rook too
                        ChessPiece rook;
                        if (tilePosition.x > oldX)
                        { 
                            rook = board.GetPieceAtPosition(new Vector3Int(tilePosition.x + 1, tilePosition.y));
                        }
                        else
                        {
                            rook = board.GetPieceAtPosition(new Vector3Int(tilePosition.x - 2, tilePosition.y));
                        }
                        if (rook && rook.ChessRuleBehaviour is ICastleEntity rookCastleInfo)
                        {
                            rookCastleInfo.CastleWithKing(activeColour, board, rook, tilePosition);
                        }
                    }
                }

                if (lastMovedPiece != null)
                {
                    tileHighlighter.SetTileColour(lastMovedPiece.TilePosition, clearColour);
                }

                lastMovedPiece = selectedChessPiece;
                selectedChessPiece = null;
                TogglePossibleMoves(clearColour);
                onFinish.Invoke();

            }
            else
            {
                tileHighlighter.SetTileColour(tilePosition, clearColour);
                tileHighlighter.SetTileColour(selectedChessPiece.TilePosition, clearColour);
                tileHighlighter.SetTileColour(board.CheckedPos, checkedColour);
                TogglePossibleMoves(clearColour);
                selectedChessPiece = null;
                Debug.Log("Invalid move");
            }
        }
    }

    private bool ValidateCastle(PlayerColour activeColour, ChessPiece selectedChessPiece, Vector3Int tilePosition)
    {
        if (selectedChessPiece.PieceType == ChessPieceType.King && selectedChessPiece.ChessRuleBehaviour is ICastleEntity castleInfo)
        {
            var castleMoves = castleInfo.GetCastleMoves(activeColour, board, selectedChessPiece);
            if (castleMoves.Contains(tilePosition))
            {
                return true;
            }
        }
        return false;
    }

    public void TogglePossibleMoves(Color colour)
    {
        foreach (var move in possibleMoves)
        {
            tileHighlighter.SetTileColour(move, colour);
        }
    }

    public bool GetChessPiece(out ChessPiece chessPiece)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

        if (hit.collider != null) 
        {
            if (hit.collider.transform.TryGetComponent(out chessPiece))
            {
                return true;
            }

        }
        chessPiece = null;
        return false;
    }

    public void ClearHighlights(PlayerColour activeColour, PlayerColour currentColour, bool isOwner)
    {
        if (!isOwner || (activeColour != currentColour))
        {
            return;
        }

        tileHighlighter.SetTileColour(board.CheckedPos, clearColour);
    }
}
