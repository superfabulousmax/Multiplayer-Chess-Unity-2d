using UnityEngine;
using static chess.enums.ChessEnums;

public class ChessPiecesContainer : MonoBehaviour
{
    [SerializeField]
    ChessPieces playerOnePieces;

    [SerializeField]
    ChessPieces playerTwoPieces;

    [SerializeField]
    string fenString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    FENChessNotation startingSetup;

    const string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public FENChessNotation StartingSetup { get => startingSetup; }
    public ChessPieces PlayerOnePieces { get => playerOnePieces; }
    public ChessPieces PlayerTwoPieces { get => playerTwoPieces; }

    public static ChessPiecesContainer Singleton;

    public void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
    }

    private void Start()
    {
        startingSetup = FENReader.ReadFENInput(fenString);
    }

    public Sprite GetSpriteForPiece(PlayerColour playerColour, ChessPieceType chessPieceType)
    {
        if (playerColour == PlayerColour.PlayerOne)
        {
            return PlayerOnePieces.GetSprite(chessPieceType);
        }
        return PlayerTwoPieces.GetSprite(chessPieceType);
    }
}
