using UnityEngine;

public class QueenChessPiece : IChessRule
{
    IChessRule takePieceRule;
    IChessRule bishopChessRule;
    IChessRule rookChessRule;

    public QueenChessPiece(IChessRule takePieceRule)
    {
        this.takePieceRule = takePieceRule;
        bishopChessRule = new BishopChessPiece(takePieceRule, new MoveToStopCheck());
        rookChessRule = new RookChessPiece(takePieceRule, new MoveToStopCheck());
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        return bishopChessRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece, isSimulation) || rookChessRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece, isSimulation);
    }
}
