using UnityEngine;
using static chess.enums.ChessEnums; 

public class PawnPromotionRule : IChessRule
{
    public bool PossibleMove(PlayerColour activeColour, IBoard board, IChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        takenPiece = false;

        var y = newPosition.y;
        if (activeColour == PlayerColour.PlayerOne)
        {
            return y == GameConstants.PawnPromotionEndOfBoardPlayerOne;
        }
        else if (activeColour == PlayerColour.PlayerTwo)
        {
            return y == GameConstants.PawnPromotionEndOfBoardPlayerTwo;
        }
        return false;
    }
}
