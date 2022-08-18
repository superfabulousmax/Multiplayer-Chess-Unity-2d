using UnityEngine;

public class BishopChessPiece : IChessRule
{
    IChessRule takePieceRule;
    IChessRule moveToStopCheckRule;
    public BishopChessPiece(IChessRule takePieceRule, IChessRule moveToStopCheckRule)
    {
        this.takePieceRule = takePieceRule;
        this.moveToStopCheckRule = moveToStopCheckRule;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, out bool checkedKing)
    {
        takenPiece = false;
        checkedKing = false;

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

        var boardState = board.GetBoardState();

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

        takePieceRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece, out var _);
        // check if move piece to stop check or if moving piece causes check
        var result = moveToStopCheckRule.PossibleMove(activeColour, board, piece, newPosition, out var _, out var _);
        if (!result)
        {
            takenPiece = false;
            return false;
        }

        return true;
    }
}
