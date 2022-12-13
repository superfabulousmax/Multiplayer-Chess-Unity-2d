using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static chess.enums.ChessEnums;
public class QueenChessPiece : IChessRule, IMoveList
{
    IChessRule bishopChessRule;
    IChessRule rookChessRule;
    IMoveList bishopMoves;
    IMoveList rookMoves;

    public QueenChessPiece(IChessRule takePieceRule)
    {
        bishopChessRule = new BishopChessPiece(takePieceRule, new MoveToStopCheck());
        bishopMoves = bishopChessRule as IMoveList;
        rookChessRule = new RookChessPiece(takePieceRule, new MoveToStopCheck());
        rookMoves = rookChessRule as IMoveList;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        return bishopChessRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece, isSimulation) || rookChessRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece, isSimulation);
    }

    public IReadOnlyList<Vector3Int> GetPossibleMoves(PlayerColour activeColour, Board board, ChessPiece piece)
    {
        return bishopMoves.GetPossibleMoves(activeColour, board, piece).Concat(rookMoves.GetPossibleMoves(activeColour, board, piece)).ToList();
    }

}
