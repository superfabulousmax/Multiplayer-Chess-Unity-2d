using UnityEngine;

public class PawnPromotionRule : IChessRule
{

    private const int EndOfBoardPlayerTwo = 0;
    private const int EndOfBoardPlayerOne = 7;

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        takenPiece = false;

        var y = newPosition.y;

        if (activeColour == PlayerColour.PlayerOne)
        {
            if (y != EndOfBoardPlayerOne)
            {
                return false;
            }
        }
        else if (activeColour == PlayerColour.PlayerTwo)
        {
            if (y != EndOfBoardPlayerTwo)
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
