namespace chess.enums
{
    public class ChessEnums
    {
        public enum PlayerColour { PlayerOne = 0, PlayerTwo = 1, Unassigned = 3, Assigned = 4 }
        public enum ChessPieceType { Pawn, King, Queen, Knight, Rook, Bishop };

        public static PlayerColour GetOppositeColour(PlayerColour colour)
        {
            if (colour == PlayerColour.PlayerOne)
            {
                return PlayerColour.PlayerTwo;
            }
            return PlayerColour.PlayerOne;
        }
    }
}
