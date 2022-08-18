using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class PawnChessPiece : IChessRule
{
    [SerializeField]
    NetworkVariable<bool> isFirstMove = new(true, writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField]
    NetworkVariable<bool> firstMoveTwo = new(false, writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField]
    NetworkVariable<int> moveCount = new(0, writePerm: NetworkVariableWritePermission.Owner);
    public static NetworkVariable<uint> lastMovedPawnId = new(0, writePerm: NetworkVariableWritePermission.Owner);
    PlayerColour pawnColour;

    public bool IsFirstMove { get => isFirstMove.Value; set => isFirstMove.Value = value; }
    public int MoveCount { get => moveCount.Value; set => moveCount.Value = value; }
    public bool FirstMoveTwo { get => firstMoveTwo.Value; set => firstMoveTwo.Value = value; }
    public uint LastMovedPawnID { get => lastMovedPawnId.Value; set => lastMovedPawnId.Value = value; }

    IEnPassantChessRule enPassant;
    IChessRule pawnPromotion;

    public PawnChessPiece(IChessRule pawnPromotion, PlayerColour pawnColour, Vector3Int tilePosition, bool isFirstMove = true, bool firstMoveTwo = false)
    {
        this.isFirstMove.Value = isFirstMove;
        this.firstMoveTwo.Value = firstMoveTwo;
        this.pawnColour = pawnColour;
        if (isFirstMove)
        {
            moveCount.Value = 0;
        }

        enPassant = new EnPassant();
        this.pawnPromotion = pawnPromotion;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, out bool checkedKing)
    {
        takenPiece = false;
        checkedKing = false;

        var boardState = board.GetBoardState();
        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;
        var yDiff = Mathf.Abs(newPosition.y - y);
        if (Mathf.Abs(newPosition.x - x) > 1)
            return false;
        if (yDiff > 2 || yDiff < 1)
            return false;
        //Debug.Log($"possible move {piece.TilePosition} to {newPosition}");
        ChessPiece lastMovedPawn = board.GetPieceFromId(lastMovedPawnId.Value);
        var takenWithEnPassant = false;
        if (activeColour == PlayerColour.PlayerOne)
        {
            if (newPosition.y <= y)
                return false;

            if (newPosition.x == x)
            {

                if (yDiff > 2 || (!isFirstMove.Value && yDiff == 2 ))
                {
                    return false;
                }
                // check piece in the way
                for (int i = y + 1; i  <= newPosition.y; i++)
                {
                    if (boardState[i, x] >= 0)
                        return false;
                }
            }
            else
            {
                if (yDiff > 1)
                {
                    return false;
                }
                if (lastMovedPawn != null && enPassant.CheckEnPassant(1, pawnColour, board, piece, lastMovedPawn, newPosition, out var takenPiecePosition))
                {
                    board.TakePieceServerRpc(piece, takenPiecePosition);
                    takenWithEnPassant = true;
                }
                else if (boardState[newPosition.y, newPosition.x] < 0 || board.CheckPiece(boardState[newPosition.y, newPosition.x], ChessPiece.ChessPieceType.King))
                {
                    return false;
                }
                else
                {
                    takenPiece = true;
                    Debug.Log("Taking new piece");
                }
            }
        }
        else
        {
            if (newPosition.y >= y)
                return false;
            if (newPosition.x == x)
            {
                if (yDiff > 2 || (!isFirstMove.Value && yDiff == 2))
                {
                    return false;
                }
                // check piece in the way
                for (int i = y - 1; i >= newPosition.y; i--)
                {
                    if (boardState[i, x] >= 0)
                        return false;
                }
            }
            else
            {
                // check taking a piece
                if (yDiff > 1)
                {
                    return false;
                }
                if (lastMovedPawn != null && enPassant.CheckEnPassant(-1, pawnColour, board, piece, lastMovedPawn, newPosition, out var takenPiecePosition))
                {
                    board.TakePieceServerRpc(piece, takenPiecePosition);
                    takenWithEnPassant = true;
                }
                else if (boardState[newPosition.y, newPosition.x] < 0 || board.CheckPiece(boardState[newPosition.y, newPosition.x], ChessPiece.ChessPieceType.King))
                {
                    return false;
                }
                else
                {
                    takenPiece = true;
                    Debug.Log("Taking new piece");
                }
            }
        }

        if (!takenWithEnPassant && lastMovedPawn != null && lastMovedPawn.ChessRuleBehaviour is PawnChessPiece lastPawn)
        {
            lastPawn.FirstMoveTwo = false;
            lastMovedPawn.SyncDataServerRpc(lastPawn.moveCount.Value, lastPawn.isFirstMove.Value, lastPawn.firstMoveTwo.Value, lastMovedPawnId.Value);
        }

        if (isFirstMove.Value && yDiff == 2)
        {
            firstMoveTwo.Value = true;
        }
        if (isFirstMove.Value)
        {
            isFirstMove.Value = false;
        }

        lastMovedPawnId.Value = (uint)piece.NetworkObjectId;
        moveCount.Value++;
        piece.SyncDataServerRpc(moveCount.Value, isFirstMove.Value, firstMoveTwo.Value, lastMovedPawnId.Value);

        if (pawnPromotion.PossibleMove(activeColour, board, piece, newPosition, out var _, out var _))
        {
            board.HandlePawnPromotionServerRpc(piece, ChessPiece.ChessPieceType.Queen);
        }

        return true;
    }
}
