using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static chess.enums.ChessEnums;

public class ActivePlayerInput : IPlayerInput
{
    IBoard board;
    IChessPiece selectedChessPiece;
    IChessPiece lastMovedPiece;

    BoardTileHighlighter tileHighlighter;

    Action onFinish;

    List<Vector3Int> possibleMoves;

    public ActivePlayerInput(BoardNetworked board, Action onFinish)
    {
        this.board = board;
        this.tileHighlighter = board.TileHighlighter;
        this.onFinish = onFinish;
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
                        tileHighlighter.SetTileColour(selectedChessPiece.Position, tileHighlighter.ClearColour);
                        TogglePossibleMoves(tileHighlighter.ClearColour);
                    }

                    selectedChessPiece = chessPiece;
                    tileHighlighter.SetTileColour(selectedChessPiece.Position, tileHighlighter.HighlightColour);
                    if (selectedChessPiece.Position != board.CheckedPos)
                    {
                        tileHighlighter.SetTileColour(board.CheckedPos, tileHighlighter.CheckedColour);
                    }

                    if (selectedChessPiece?.MoveList != null)
                    {
                        possibleMoves = selectedChessPiece.MoveList.GetPossibleMoves(activeColour, board, selectedChessPiece).ToList();
                        if (selectedChessPiece.PieceRuleBehaviour is ICastleEntity castleInfo)
                        {
                            var castleMoves = castleInfo.GetCastleMoves(activeColour, board, selectedChessPiece);
                            possibleMoves.AddRange(castleMoves);
                        }
                        TogglePossibleMoves(tileHighlighter.PossibleMoveColour);
                    }

                    return;
                }
            }
            if (selectedChessPiece == null)
            {
                return;
            }

            var tilePosition = GetTileAtMousePosition(Input.mousePosition, board.BoardTileMap);
            if(selectedChessPiece.Position == tilePosition)
            {
                return;
            }

            var validateMove = board.ValidateMove(activeColour, selectedChessPiece, tilePosition, out bool isPieceTaken);
            var validateCastle = ValidateCastle(activeColour, selectedChessPiece, tilePosition);
            if (validateMove || validateCastle)
            {
                tileHighlighter.SetTileColour(tilePosition, tileHighlighter.HighlightColour);
                tileHighlighter.StartWaitThenSetColour(tilePosition, tileHighlighter.ClearColour);
                tileHighlighter.StartWaitThenSetColour(selectedChessPiece.Position, tileHighlighter.ClearColour);

                if (isPieceTaken)
                {
                    board.TakePiece(selectedChessPiece, tilePosition);
                }
                else
                {
                    var oldX = selectedChessPiece.Position.x;
                    selectedChessPiece.SetPosition(tilePosition);
                    if (validateCastle)
                    {
                        // move rook too
                        IChessPiece rook;
                        if (tilePosition.x > oldX)
                        { 
                            rook = board.GetPieceAtPosition(new Vector3Int(tilePosition.x + 1, tilePosition.y));
                        }
                        else
                        {
                            rook = board.GetPieceAtPosition(new Vector3Int(tilePosition.x - 2, tilePosition.y));
                        }
                        if (rook != null && rook.PieceRuleBehaviour is ICastleEntity rookCastleInfo)
                        {
                            rookCastleInfo.CastleWithKing(activeColour, board, rook, tilePosition);
                        }
                    }
                }

                if (lastMovedPiece != null)
                {
                    tileHighlighter.SetTileColour(lastMovedPiece.Position, tileHighlighter.ClearColour);
                }

                lastMovedPiece = selectedChessPiece;
                selectedChessPiece = null;
                TogglePossibleMoves(tileHighlighter.ClearColour);
                onFinish.Invoke();

            }
            else
            {
                tileHighlighter.SetTileColour(tilePosition, tileHighlighter.ClearColour);
                tileHighlighter.SetTileColour(selectedChessPiece.Position, tileHighlighter.ClearColour);
                tileHighlighter.SetTileColour(board.CheckedPos, tileHighlighter.CheckedColour);
                TogglePossibleMoves(tileHighlighter.ClearColour);
                selectedChessPiece = null;
                Debug.Log("Invalid move");
            }
        }
    }

    public Vector3Int GetTileAtMousePosition(Vector3 mousePosition, Tilemap tilemap)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        var plane = new Plane(Vector3.back, Vector3.zero);
        plane.Raycast(ray, out var hitDist);
        var point = ray.GetPoint(hitDist);
        return tilemap.WorldToCell(point);
    }

    private bool ValidateCastle(PlayerColour activeColour, IChessPiece selectedChessPiece, Vector3Int tilePosition)
    {
        if (selectedChessPiece.PieceType == ChessPieceType.King && selectedChessPiece.PieceRuleBehaviour is ICastleEntity castleInfo)
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

    public bool GetChessPiece(out ChessPieceNetworked chessPiece)
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

        tileHighlighter.SetTileColour(board.CheckedPos, tileHighlighter.ClearColour);
    }
}
