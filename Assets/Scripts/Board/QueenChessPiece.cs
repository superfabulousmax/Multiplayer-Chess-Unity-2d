using UnityEngine;

public class QueenChessPiece : IChessRule
{
    IChessRule takePieceRule;
    IChessRule bishopChessRule;
    IChessRule rookChessRule;

    public QueenChessPiece(IChessRule takePieceRule)
    {
        this.takePieceRule = takePieceRule;
        bishopChessRule = new BishopChessPiece(takePieceRule);
        rookChessRule = new RookChessPiece(takePieceRule);
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        return bishopChessRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece) || rookChessRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece);
    }
}
