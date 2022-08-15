using UnityEngine;

public class EnPassant : IEnPassantChessRule
{

    public EnPassant()
    {
    }

    public bool CheckEnPassant(int direction, PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out Vector3Int takenPiecePosition)
    {
        // check taking a piece
        takenPiecePosition = Vector3Int.zero;
        var result = CheckEnPassantCapture(direction, piece, newPosition, board);
        if (result.isEnPassant)
        {
            Debug.Log("Taking new piece with en passant");
            takenPiecePosition = result.takenPosition;
            return true;
        }

        return false;
    }

    public (bool isEnPassant, Vector3Int takenPosition) CheckEnPassantCapture(int direction, ChessPiece piece, Vector3Int newPosition, Board board)
    {
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
        if (direction > 0)
        {
            if (newPosition.y < y)
                return (false, -Vector3Int.one);
        }
        else
        {
            if (newPosition.y > y)
                return (false, -Vector3Int.one);
        }
        var pawnPosition = new Vector3Int(newPosition.x, newPosition.y - direction, newPosition.z);
        var id = boardState[pawnPosition.y, pawnPosition.x];
        if (board.CheckPiece(id, ChessPiece.ChessPieceType.Pawn))
        {
            var pawn = board.GetPieceFromId((uint)id);
            if (pawn != null)
            {
                var chessRule = pawn.ChessRuleBehaviour as PawnChessPiece;
                if (chessRule.FirstMoveTwo && chessRule.MoveCount == 1)
                {
                    return (true, pawnPosition);
                }
            }
        }

        return (false, pawnPosition);
    }
}
