using UnityEngine;
using static chess.enums.ChessEnums; 

public class PawnPromotionRule : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        takenPiece = false;

        var y = newPosition.y;

        if (activeColour == PlayerColour.PlayerOne)
        {
            if (y != GameConstants.PawnPromotionEndOfBoardPlayerOne)
            {
                return false;
            }
        }
        else if (activeColour == PlayerColour.PlayerTwo)
        {
            if (y != GameConstants.PawnPromotionEndOfBoardPlayerTwo)
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        return true;
    }
}
