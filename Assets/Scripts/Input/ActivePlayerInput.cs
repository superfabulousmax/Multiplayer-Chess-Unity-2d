using System;
using UnityEngine;

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

    public ActivePlayerInput(Board board, Action onFinish)
    {
        this.board = board;
        this.tileHighlighter = board.TileHighlighter;
        this.onFinish = onFinish;
        // perwinkle color!
        this.highlightColour = new Color(204 / 255.0f, 204 / 255.0f, 255 / 255.0f, 200 / 255.0f);
        this.clearColour = Color.clear;
        this.checkedColour = Color.red;
    }

    public void HandleInput(int id, PlayerColour activeColour, PlayerColour currentColour, bool isOwner)
    {
        if (!isOwner || (activeColour != currentColour))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Handle input for {currentColour} active {activeColour}");
            if (GetChessPiece(out var chessPiece))
            {
                if(chessPiece.PlayerColour == activeColour)
                {
                    if (selectedChessPiece != null)
                    {
                        tileHighlighter.SetTileColour(selectedChessPiece.TilePosition, clearColour);
                    }

                    selectedChessPiece = chessPiece;
                    tileHighlighter.SetTileColour(chessPiece.TilePosition, highlightColour);
                    if (selectedChessPiece.TilePosition != board.CheckedPos)
                    {
                        tileHighlighter.SetTileColour(board.CheckedPos, checkedColour);
                    }
                    Debug.Log($"selected chess piece {selectedChessPiece}");
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

            if (board.ValidateMove(activeColour, selectedChessPiece, tilePosition, out bool isPieceTaken))
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
                    selectedChessPiece.SetTilePositionServerRpc(tilePosition);
                }

                if (lastMovedPiece != null)
                {
                    tileHighlighter.SetTileColour(lastMovedPiece.TilePosition, clearColour);
                }

                lastMovedPiece = selectedChessPiece;
                selectedChessPiece = null;

                onFinish.Invoke();

            }
            else
            {
                tileHighlighter.SetTileColour(tilePosition, clearColour);
                tileHighlighter.SetTileColour(selectedChessPiece.TilePosition, clearColour);
                tileHighlighter.SetTileColour(board.CheckedPos, checkedColour);
                selectedChessPiece = null;
                Debug.Log("Invalid move");
            }
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
}
