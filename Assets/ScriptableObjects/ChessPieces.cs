using UnityEngine;
using static ChessPiece;

[CreateAssetMenu(fileName = "ChessPieces", menuName = "ScriptableObjects/Create Chess Piece Set", order = 1)]
public class ChessPieces : ScriptableObject
{
    public PlayerColour playerColour;
    public Sprite king;
    public Sprite queen;
    public Sprite rook;
    public Sprite knight;
    public Sprite bishop;
    public Sprite pawn;

    public Sprite GetSprite(ChessPieceType chessPieceType)
    {
        switch(chessPieceType)
        {
            case ChessPieceType.King:
                return king;
            case ChessPieceType.Queen:
                return queen;
            case ChessPieceType.Rook:
                return rook;
            case ChessPieceType.Knight:
                return knight;
            case ChessPieceType.Bishop:
                return bishop;
            case ChessPieceType.Pawn:
                return pawn;
            default:
                return null;
        }
    }
}
