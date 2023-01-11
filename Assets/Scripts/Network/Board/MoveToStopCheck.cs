using UnityEngine;
using System.Linq;
using static chess.enums.ChessEnums;

public class MoveToStopCheck : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, IBoard board, IChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        takenPiece = false;

        var boardState = board.GetBoardState();

        var y = piece.Position.y;
        var x = piece.Position.x;
        
        // simulate new move
        var simulatedBoardState = new int[GameConstants.BoardLengthDimension, GameConstants.BoardLengthDimension];
        for (var j = 0; j < GameConstants.BoardLengthDimension; j++)
        {
            for (var i = 0; i < GameConstants.BoardLengthDimension; i++)
            {
                simulatedBoardState[j, i] = boardState[j, i];
            }
        }

        simulatedBoardState[y, x] = -1;
        simulatedBoardState[newPosition.y, newPosition.x] = piece.PieceId;

        if (board.IsInCheck(out var king))
        {
            if (king.PlayerColour == piece.PlayerColour)
            {
                if (board.IsInCheck(simulatedBoardState, out var checkedKings))
                {
                    if (checkedKings.Any(obj => obj.PlayerColour == piece.PlayerColour))
                    {
                        return false;
                    }
                }
            }
        }
        else
        {
            if (board.IsInCheck(simulatedBoardState, out var checkedKings))
            {
                if (piece.PieceType == ChessPieceType.King)
                {
                    return false;
                }
                if (checkedKings.Any(obj => obj.PlayerColour == piece.PlayerColour))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
