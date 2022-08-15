using System;
using UnityEngine;

public class ActivePlayerInput : IPlayerInput
{
    Board board;
    ChessPiece selectedChessPiece;

    public ActivePlayerInput(Board board)
    {
        this.board = board;
    }

    public void HandleInput(int id, PlayerColour activeColour, PlayerColour currentColour, bool isOwner, Action onFinish)
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
            //if (this.selectedChessPiece.PlayerColour != activeColour)
            //{
            //    this.selectedChessPiece = null;
            //    return;
            //}
            var tilePosition = board.GetTileAtMousePosition(Input.mousePosition);
            if(selectedChessPiece.TilePosition == tilePosition)
            {
                return;
            }
            if (board.ValidateMove(activeColour, selectedChessPiece, tilePosition, out bool takenPiece))
            {
                if(takenPiece)
                {
                    board.TakePieceServerRpc(selectedChessPiece, tilePosition);
                }
                else
                {
                    selectedChessPiece.SetTilePositionServerRpc(tilePosition);
                }
             
                this.selectedChessPiece = null;
                onFinish?.Invoke();
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
