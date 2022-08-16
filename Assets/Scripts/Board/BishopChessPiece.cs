using UnityEngine;

public class BishopChessPiece : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        takenPiece = false;
        var boardState = board.GetBoardState();
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

        if (board.CheckPiece(boardState[newPosition.y, newPosition.x], ChessPiece.ChessPieceType.King))
        {
            return false;
        }
        if (boardState[newPosition.y, newPosition.x] > 0 && activeColour == board.GetPieceFromId((uint)boardState[newPosition.y, newPosition.x]).PlayerColour)
        {
            return false;
        }
        if (boardState[newPosition.y, newPosition.x] > 0)
        {
            takenPiece = true;
            Debug.Log("bishop taking new piece");
        }

        return true;
    }
}
