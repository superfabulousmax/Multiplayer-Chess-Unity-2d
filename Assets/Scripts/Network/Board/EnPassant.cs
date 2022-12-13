using UnityEngine;
using static chess.enums.ChessEnums;

public class EnPassant : IEnPassantChessRule
{

    public EnPassant()
    {
    }

    public bool CheckEnPassant(int direction, PlayerColour activeColour, Board board, ChessPiece piece, ChessPiece lastMovedPawn, Vector3Int newPosition, out Vector3Int takenPiecePosition)
    {
        // check taking a piece
        takenPiecePosition = Vector3Int.zero;
        var result = CheckEnPassantCapture(direction, piece, lastMovedPawn, newPosition, board);
        if (result.isEnPassant)
        {
            Debug.Log("Taking new piece with en passant");
            takenPiecePosition = result.takenPosition;
            return true;
        }

        return false;
    }

    public (bool isEnPassant, Vector3Int takenPosition) CheckEnPassantCapture(int direction, ChessPiece piece, ChessPiece lastMovedPawn, Vector3Int newPosition, Board board)
    {
        if (lastMovedPawn == piece)
        {
            return (false, -Vector3Int.one);
        } 

        if (lastMovedPawn.ChessRuleBehaviour is PawnChessPiece pawn)
        {
            if(pawn.FirstMoveTwo != true && pawn.MoveCount != 1)
            {
                return (false, -Vector3Int.one);
            }
        }
        var boardState = board.GetBoardState();
        // check empty square
        if (boardState[newPosition.y, newPosition.x] >= 0)
            return (false, -Vector3Int.one);
        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;
        if (Mathf.Abs(y - newPosition.y) != 1)
        {
            return (false, -Vector3Int.one);
        }
        if (Mathf.Abs(x - newPosition.x) != 1)
        {
            return (false, -Vector3Int.one);
        }
        var targetPos = lastMovedPawn.TilePosition;
        if (newPosition.x != targetPos.x)
        {
            return (false, -Vector3Int.one);
        }
        if(direction > 0)
        {
            if (newPosition.y != targetPos.y + 1)
                return (false, -Vector3Int.one);
        }
        else
        {
            if (newPosition.y != targetPos.y - 1)
                return (false, -Vector3Int.one);
        }
        var comparePosition = new Vector3Int(targetPos.x, targetPos.y + direction, targetPos.z);
        if(comparePosition != newPosition)
            return (false, -Vector3Int.one);
        var takenPawnPosition = new Vector3Int(newPosition.x, newPosition.y - direction, newPosition.z);
        return (true, takenPawnPosition);
    }
}
