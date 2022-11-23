using UnityEngine;

public class MoveToStopCheck : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        takenPiece = false;

        var boardState = board.GetBoardState();

        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;
        
        // simulate new move
        var simulatedBoardState = new int[GameConstants.BoardLengthDimension, GameConstants.BoardLengthDimension];
        for (int j = 0; j < GameConstants.BoardLengthDimension; j++)
        {
            for (int i = 0; i < GameConstants.BoardLengthDimension; i++)
            {
                simulatedBoardState[j, i] = boardState[j, i];
            }
        }

        simulatedBoardState[y, x] = -1;
        simulatedBoardState[newPosition.y, newPosition.x] = (int)piece.NetworkObjectId;

        if (board.IsInCheck(out var king))
        {
            if (king.PlayerColour == piece.PlayerColour)
            {
                if (board.IsInCheck(simulatedBoardState, out var checkedKing))
                {
                    if (checkedKing.PlayerColour == piece.PlayerColour)
                    {
                        return false;
                    }
                }
            }
        }
        else
        {
            if (board.IsInCheck(simulatedBoardState, out var checkedKing))
            {
                if (piece.PieceType == ChessPiece.ChessPieceType.King)
                {
                    return false;
                }
                if (checkedKing.PlayerColour == piece.PlayerColour)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
