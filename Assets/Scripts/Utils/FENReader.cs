using UnityEngine.Assertions;

/// <summary>
/// Reads FEN from a file or string and populates <seealso cref="FENChessNotation"/>
/// </summary>
public class FENReader
{
    private const int FenArgsNumber = 6;
    public static FENChessNotation ReadFENInput(string input)
    {
        if(string.IsNullOrEmpty(input))
        {
            throw new System.ArgumentNullException("Invalid FEN input string");
        }
        string[] args = input.Split();

        Assert.IsTrue(args.Length == FenArgsNumber);

        var fen = new FENChessNotation
        {
            piecePlacement = args[0],
            activeColour = args[1][0],
            castlingRights = args[2],
            enPassantTargets = args[3],
            halfMoveClock = int.Parse(args[4]),
            fullMoveNumber = int.Parse(args[5])

        };
        return fen;
    }
}
