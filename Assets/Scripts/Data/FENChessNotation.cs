/// <summary>
/// FEN for placing pieces on board and tracking turns, castling rights, en passant and draw conditions
/// <see cref="https://www.chess.com/terms/fen-chess#what-is-fen"/>
/// <seealso cref="https://lichess.org/editor"/>
/// </summary>
public struct FENChessNotation
{
    public const char delimeter = '/';
    public const char nothing = '-';
    // String sequence describing position of pieces
    // e.g. the sequence "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"
    // describes the piece placement field of the starting position of a game of chess.
    public string piecePlacement;

    // Who move next
    // w -> white's turn
    // b -> black's turn
    // Always in lowercase
    public char activeColour;

    // If the players can castle and to what side (kingside or queenside).
    // Uppercase letters come first to indicate White's castling availability
    // e.g. - -> neither side may castle
    // e.g. KQ -> kingside and queenside for white only
    // e.g. kq -> kingside and queenside for black only
    // e.g. k -> kingside only for black
    public string castlingRights;

    // If a pawn has moved two squares immediately before a position is reached and
    // is thus a possible target for an en passant capture
    // the FEN string adds the square behind the pawn in algebraic notation in its fourth field.
    // If no en passant targets are available, the "-" symbol is used.
    public string enPassantTargets;

    // How many moves both players have made since the last pawn advance or piece capture—known 
    // AKA the number of halfmoves.
    // This field is useful to enforce the 50-move draw rule.
    // When this counter reaches 100 the game ends in a draw.
    public int halfMoveClock;

    // The sixth and last field of the FEN code shows the number of completed turns in the game.
    // This number is incremented by one every time Black moves. Chess programmers call this a fullmove.
    public int fullMoveNumber;
}
