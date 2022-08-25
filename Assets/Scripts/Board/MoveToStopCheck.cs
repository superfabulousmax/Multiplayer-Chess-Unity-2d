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
        var simulatedBoardState = new int[boardState.Length, boardState.Length];
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < 8; i++)
            {
                simulatedBoardState[j, i] = boardState[j, i];
            }
        }

        simulatedBoardState[y, x] = -1;
        simulatedBoardState[newPosition.y, newPosition.x] = (int)piece.NetworkObjectId;

        //board.PrintOutBoardState(simulatedBoardState);

        if (board.IsInCheck(out var king))
        {
            if (king.PlayerColour == piece.PlayerColour)
            {
                if (board.IsInCheck(simulatedBoardState, out var _))
                {
                    return false;
                }
            }
        }
        else
        {
            if (board.IsInCheck(simulatedBoardState, out var newKing))
            {
                if (piece.PieceType == ChessPiece.ChessPieceType.King)
                {
                    return false;
                }
                if (newKing.PlayerColour == piece.PlayerColour)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
