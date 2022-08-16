using System;
using UnityEngine;

public class ActivePlayerInput : IPlayerInput
{
    Board board;
    ChessPiece selectedChessPiece;
    Action onFinish;
    public ActivePlayerInput(Board board, Action onFinish)
    {
        this.board = board;
        this.onFinish = onFinish;
        board.onValidateMove += OnValidateMove;
    }

    private bool OnValidateMove(ChessPiece selectedChessPiece, bool isValid, bool isPieceTaken, Vector3Int tilePosition)
    {
        if (isValid)
        {
            if (isPieceTaken)
            {
                board.TakePieceServerRpc(selectedChessPiece, tilePosition);
            }
            else
            {
                selectedChessPiece.SetTilePositionServerRpc(tilePosition);
            }

            this.selectedChessPiece = null;
            onFinish.Invoke();
        }
        else
        {
            Debug.Log("Invalid move");
            return false;
        }
        return true;
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
                    this.selectedChessPiece = chessPiece;
                    Debug.Log($"selected chess piece {selectedChessPiece}");
                    return;
                }
            }
            if (this.selectedChessPiece == null)
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
                if (isPieceTaken)
                {
                    board.TakePieceServerRpc(selectedChessPiece, tilePosition);
                }
                else
                {
                    selectedChessPiece.SetTilePositionServerRpc(tilePosition);
                }

                this.selectedChessPiece = null;
                onFinish.Invoke();
            }
            else
            {
                Debug.Log("Invalid move");
            }
        }
    }

    public bool GetChessPiece(out ChessPiece chessPiece)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

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
