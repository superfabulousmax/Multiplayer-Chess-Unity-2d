using System.Text;
using static chess.enums.ChessEnums;

public static class DebugUtils
{
    public static string GetEncoding(PlayerColour colour, ChessPieceType chessPieceType)
    {
        var sb = new StringBuilder(2);
        switch (colour)
        {
            case PlayerColour.PlayerOne:
                sb.Append("w");
                break;
            case PlayerColour.PlayerTwo:
                sb.Append("b");
                break;
        }
        switch (chessPieceType)
        {
            case ChessPieceType.Pawn:
                sb.Append("p");
                break;
            case ChessPieceType.King:
                sb.Append("k");
                break;
            case ChessPieceType.Queen:
                sb.Append("r");
                break;
            case ChessPieceType.Rook:
                sb.Append("r");
                break;
            case ChessPieceType.Knight:
                sb.Append("n");
                break;
            case ChessPieceType.Bishop:
                sb.Append("b");
                break;
        }

        return sb.ToString();
    }
}
