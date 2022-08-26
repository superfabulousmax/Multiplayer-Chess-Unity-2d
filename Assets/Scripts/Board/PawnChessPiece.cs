using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PawnChessPiece : IChessRule, IMoveList
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

    public IReadOnlyList<Vector3Int> GetPossibleMoves(PlayerColour activeColour, Board board, ChessPiece piece)
    {
        var result = new List<Vector3Int>();

        var y = piece.TilePosition.y;
        var x = piece.TilePosition.x;

        var boardState = board.GetBoardState();

        var direction = -1;
        if (activeColour == PlayerColour.PlayerOne)
        {
            direction = 1;
        }

        var possiblePositions = new List<Vector3Int>();
        // up one
        possiblePositions.Add(new Vector3Int(x, y + (direction * 1)));
        if (isFirstMove)
        {
            // up 2
            possiblePositions.Add(new Vector3Int(x, y + (direction * 2)));
        }
        // take left
        possiblePositions.Add(new Vector3Int(x - 1, y + (direction * 1)));
        // take right
        possiblePositions.Add(new Vector3Int(x + 1, y + (direction * 1)));

        foreach (var position in possiblePositions)
        {
            if (!board.IsValidPosition(position))
            {
                continue;
            }
            var dx = Mathf.Abs(x - position.x);
            var dy = Mathf.Abs(y - position.y);
            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, position, out var _);
            // check take piece
            if (dx == 1 && dy == 1)
            {
                if (boardState[position.y, position.x] < 0 || board.CheckPiece(boardState[position.y, position.x], ChessPiece.ChessPieceType.King))
                {
                    continue;
                }
                if (canMove && boardState[position.y, position.x] >= 0 && activeColour != board.GetPieceFromId((uint)boardState[position.y, position.x]).PlayerColour)
                {
                    result.Add(position);
                    continue;
                }
                else
                {
                    continue;
                }
            }

            if (!canMove)
            {
                continue;
            }

            if (dy == 2)
            {
                if (activeColour == PlayerColour.PlayerOne)
                {
                    for (int i = y + 1; i <= position.y; i++)
                    {
                        if (boardState[i, x] >= 0)
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    for (int i = y - 1; i >= position.y; i--)
                    {
                        if (boardState[i, x] >= 0)
                        {
                            continue;
                        }
                    }
                }

            }
            if (boardState[position.y, position.x] >= 0)
            {
                continue;
            }
            else
            {
                result.Add(position);
            }
        }

        // check en passant
        var possibleEnpassants = new List<Vector3Int>();
        possibleEnpassants.Add(new Vector3Int(x + 1, y + (direction * 1)));
        possibleEnpassants.Add(new Vector3Int(x - 1, y + (direction * 1)));

        var lastMovedPawn = board.GetPieceFromId(lastMovedPawnId);

        foreach (var position in possibleEnpassants)
        {
            if (!board.IsValidPosition(position))
            {
                continue;
            }

            var canMove = moveToStopCheckRule.PossibleMove(activeColour, board, piece, position, out var _);
            if (!canMove)
            {
                continue;
            }

            if (lastMovedPawn != null && enPassant.CheckEnPassant(direction, pawnColour, board, piece, lastMovedPawn, position, out var _))
            {
                result.Add(position);
            }
        }

        return result;
    }
}
