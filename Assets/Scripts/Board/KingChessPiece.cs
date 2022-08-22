using UnityEngine;

public class KingChessPiece : IChessRule, ICastleEntity
{
    IChessRule moveToStopCheckRule;
    IChessRule castleRule;

    int moveCount;
    public int MoveCount { get => moveCount; set => moveCount = value; }

    public KingChessPiece(IChessRule moveToStopCheckRule, IChessRule castleRule)
    {
        this.moveToStopCheckRule = moveToStopCheckRule;
        this.castleRule = castleRule;
    }

    public bool CanCastle(Board board, ChessPiece kingPiece)
    {
        return moveCount == 0;
    }


    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        takenPiece = false;

        var boardState = board.GetBoardState();
        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        var deltaY = Mathf.Abs(newPosition.y - y);
        var deltaX = Mathf.Abs(newPosition.x - x);

        if (deltaX == 0 && deltaY == 0)
            return false;
        if (deltaX > 2)
            return false;
        if (deltaY > 1)
            return false;

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
            Debug.Log("King taking new piece");
        }

        // check if move piece to stop check or if moving piece causes check
        var result = moveToStopCheckRule.PossibleMove(activeColour, board, piece, newPosition, out var _);
        if (!result)
        {
            takenPiece = false;
            return false;
        }

        if (deltaX == 2 && castleRule.PossibleMove(activeColour, board, piece, newPosition, out var _))
        {
            // check rook
        }
        else if (deltaX > 1)
        {
            takenPiece = false;
            return false;
        }

        moveCount++;
        piece.SyncDataServerRpc(moveCount, default, default, default);
        return true;
    }
}
