using UnityEngine;

public class RookChessPiece : IChessRule
{
    IChessRule takePieceRule;
    IChessRule moveToStopCheckRule;

    public RookChessPiece(IChessRule takePieceRule, IChessRule moveToStopCheckRule)
    {
        this.takePieceRule = takePieceRule;
        this.moveToStopCheckRule = moveToStopCheckRule;
    }


    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, out bool checkedKing)
    {
        takenPiece = false;
        checkedKing = false;

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
