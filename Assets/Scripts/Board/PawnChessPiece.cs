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
    IChessRule moveToStopCheckRule;

    public PawnChessPiece(IChessRule pawnPromotion, IChessRule moveToStopCheckRule, PlayerColour pawnColour, Vector3Int tilePosition, bool isFirstMove = true, bool firstMoveTwo = false)
    {
        if(pawnColour == PlayerColour.PlayerOne)
        {
            if (tilePosition.y > 1)
            {
                isFirstMove = false;
            }
        }
        else if(pawnColour == PlayerColour.PlayerTwo)
        {
            if (tilePosition.y < 6)
            {
                isFirstMove = false;
            }
        }

        this.isFirstMove.Value = isFirstMove;
        this.firstMoveTwo.Value = firstMoveTwo;
        this.pawnColour = pawnColour;

        if (isFirstMove)
        {
            moveCount.Value = 0;
        }
        else
        {
            moveCount.Value = 1;
        }

        enPassant = new EnPassant();
        this.pawnPromotion = pawnPromotion;
        this.moveToStopCheckRule = moveToStopCheckRule;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece)
    {
        takenPiece = false;

        var boardState = board.GetBoardState();
        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;
        var yDiff = Mathf.Abs(newPosition.y - y);

        if (Mathf.Abs(newPosition.x - x) > 1)
        {
            return false;
        }
        if (yDiff > 2 || yDiff < 1)
        {
            return false;
        }

        ChessPiece lastMovedPawn = board.GetPieceFromId(lastMovedPawnId.Value);
        var takenWithEnPassant = false;
        if (activeColour == PlayerColour.PlayerOne)
        {
            if (newPosition.y <= y)
            {
                return false;
            }

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
                    {
                        return false;
                    }
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

        // check if move piece to stop check or if moving piece causes check
        var result = moveToStopCheckRule.PossibleMove(activeColour, board, piece, newPosition, out var _);
        if (!result)
        {
            takenPiece = false;
            return false;
        }

        if (pawnPromotion.PossibleMove(activeColour, board, piece, newPosition, out var _))
        {
            board.AskPawnPromotionServerRpc(piece);
        }

        return true;
    }
}
