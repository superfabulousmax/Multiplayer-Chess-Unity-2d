using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class PawnChessPiece : IChessRule
{
    PlayerColour pawnColour;

    int moveCount;
    bool isFirstMove;
    bool firstMoveTwo;
    public static uint lastMovedPawnId;
    public int MoveCount { get => moveCount; set => moveCount = value; }
    public bool IsFirstMove { get => isFirstMove; set {
            Debug.Log($"Setting pawn at {tilePosition} to {value}");
            isFirstMove = value; 
        
        } }
    public bool FirstMoveTwo { get => firstMoveTwo; set => firstMoveTwo = value; }
    public uint LastMovedPawnID { get => lastMovedPawnId; set => lastMovedPawnId = value; }

    IEnPassantChessRule enPassant;
    IChessRule pawnPromotion;
    IChessRule moveToStopCheckRule;
    Vector3Int tilePosition;

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

        this.isFirstMove = isFirstMove;
        this.firstMoveTwo = firstMoveTwo;
        this.pawnColour = pawnColour;
        this.tilePosition = tilePosition;

        if (isFirstMove)
        {
            moveCount = 0;
        }
        else
        {
            moveCount = 1;
        }

        enPassant = new EnPassant();
        this.pawnPromotion = pawnPromotion;
        this.moveToStopCheckRule = moveToStopCheckRule;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
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

        ChessPiece lastMovedPawn = board.GetPieceFromId(lastMovedPawnId);
        var takenWithEnPassant = false;
        if (activeColour == PlayerColour.PlayerOne)
        {
            if (newPosition.y <= y)
            {
                return false;
            }

            if (newPosition.x == x)
            {

                if (yDiff > 2 || (!isFirstMove && yDiff == 2 ))
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
                // TODO bug with isfirstmove is false
                if (yDiff > 2 || (!isFirstMove && yDiff == 2))
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
            lastMovedPawn.SyncDataServerRpc(lastPawn.moveCount, lastPawn.isFirstMove, lastPawn.firstMoveTwo, lastMovedPawnId);
        }

        if (!isSimulation)
        {
            if (isFirstMove && yDiff == 2)
            {
                firstMoveTwo = true;
            }
            if (isFirstMove)
            {
                Debug.Log($"Setting pawn at {tilePosition} to {false} {isSimulation}");
                isFirstMove = false;
            }
        }
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

        if(!isSimulation)
        {
            lastMovedPawnId = (uint)piece.NetworkObjectId;
            moveCount++;
            piece.SyncDataServerRpc(moveCount, isFirstMove, firstMoveTwo, lastMovedPawnId);
        }

        return true;
    }
}
