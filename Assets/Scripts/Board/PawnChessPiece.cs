using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PawnChessPiece : IChessRule
{
    [SerializeField]
    bool isFirstMove;
    [SerializeField]
    bool firstMoveTwo;

    public PawnChessPiece(bool isFirstMove = true, bool firstMoveTwo = false)
    {
        this.isFirstMove = isFirstMove;
        this.firstMoveTwo = firstMoveTwo;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        takenPiece = false;
        var boardState = board.GetBoardState();
        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;
        var yDiff = Mathf.Abs(newPosition.y - y);
        if (Mathf.Abs(newPosition.x - x) > 1)
            return false;
        //Debug.Log($"possible move {piece.TilePosition} to {newPosition}");
        if (activeColour == PlayerColour.PlayerOne)
        {
            if (newPosition.y <= y)
                return false;

            if (newPosition.x == x)
            {

                if (yDiff > 2 || (!isFirstMove && yDiff == 2 ))
                {
                    return false;
                }
                // check piece in the way
                for (int i = y + 1; i  <= newPosition.y; i++)
                {
                    if (boardState[i, x] >= 0)
                        return false;
                }
            }
            else
            {
                // check taking a piece
                if (boardState[newPosition.y, newPosition.x] < 0)
                {
                    return false;
                }
                else
                {
                    takenPiece = true;
                    Debug.Log("Taking new piece");
                }
            }
        }
        else
        {
            if (newPosition.y >= y)
                return false;
            if (newPosition.x == x)
            {
                if (yDiff > 2 || (!isFirstMove && yDiff == 2))
                {
                    return false;
                }
                // check piece in the way
                for (int i = y - 1; i >= newPosition.y; i--)
                {
                    if (boardState[i, x] >= 0)
                        return false;
                }
            }
            else
            {
                // check taking a piece
                if (boardState[newPosition.y, newPosition.x] < 0)
                    return false;
                else
                {
                    takenPiece = true;
                    Debug.Log("Taking new piece");
                }
            }
        }

        if (isFirstMove)
        {
            isFirstMove = false;
        }

        return true;
    }
}
