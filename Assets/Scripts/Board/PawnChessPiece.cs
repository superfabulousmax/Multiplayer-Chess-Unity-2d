using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PawnChessPiece : IChessRule, IMoveList
{
    PlayerColour pawnColour;
    Vector3Int tilePosition;
    int moveCount;
    bool isFirstMove;
    bool firstMoveTwo;
    public static uint lastMovedPawnId;

    // Rules
    IEnPassantChessRule enPassant;
    IChessRule pawnPromotion;
    IChessRule moveToStopCheckRule;
    IChessRule takePieceRule;

    public int MoveCount { get => moveCount; set => moveCount = value; }
    public bool IsFirstMove { get => isFirstMove; set {
            Debug.Log($"Setting pawn at {tilePosition} to {value}");
            isFirstMove = value; 
        
        } }
    public bool FirstMoveTwo { get => firstMoveTwo; set => firstMoveTwo = value; }
    public uint LastMovedPawnID { get => lastMovedPawnId; set => lastMovedPawnId = value; }

    public PawnChessPiece(IChessRule pawnPromotion, IChessRule moveToStopCheckRule, IChessRule takePieceRule, 
        PlayerColour pawnColour, Vector3Int tilePosition, bool isFirstMove = true, bool firstMoveTwo = false)
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
        this.takePieceRule = takePieceRule;
    }

    public bool PossibleMove(PlayerColour activeColour, Board board, ChessPiece piece, Vector3Int newPosition, out bool takenPiece, bool isSimulation = false)
    {
        takenPiece = false;
        var possibleMoves = GetPossibleMoves(activeColour, board, piece);
        if (!possibleMoves.Contains(newPosition))
        {
            return false;
        }
        var direction = -1;
        if (activeColour == PlayerColour.PlayerOne)
        {
            direction = 1;
        }

        var takenWithEnPassant = false;
        var lastMovedPawn = board.GetPieceFromId(lastMovedPawnId);
        // check taking a piece
        if (!isSimulation && lastMovedPawn != null && enPassant.CheckEnPassant(direction, pawnColour, board, piece, lastMovedPawn, newPosition, out var takenPiecePosition))
        {
            board.TakePieceServerRpc(piece, takenPiecePosition);
            takenWithEnPassant = true;
        }
        else
        {
            takePieceRule.PossibleMove(activeColour, board, piece, newPosition, out takenPiece);
        }

        if (!isSimulation && !takenWithEnPassant && lastMovedPawn != null && lastMovedPawn.ChessRuleBehaviour is PawnChessPiece lastPawn)
        {
            lastPawn.FirstMoveTwo = false;
            lastMovedPawn.SyncDataServerRpc(lastPawn.moveCount, lastPawn.isFirstMove, lastPawn.firstMoveTwo, lastMovedPawnId);
        }

        if (!isSimulation && pawnPromotion.PossibleMove(activeColour, board, piece, newPosition, out var _))
        {
            board.AskPawnPromotionServerRpc(piece);
        }

        if(!isSimulation)
        {
            var y = piece.TilePosition.y;
            var dy = Mathf.Abs(newPosition.y - y);
            if (isFirstMove && dy == 2)
            {
                firstMoveTwo = true;
            }
            if (isFirstMove)
            {
                isFirstMove = false;
            }
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
            var pieceInWay = false;
            if (dy == 2)
            {
                if (activeColour == PlayerColour.PlayerOne)
                {
                    for (int i = y + 1; i <= position.y; i++)
                    {
                        if (boardState[i, x] >= 0)
                        {
                            pieceInWay = true;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = y - 1; i >= position.y; i--)
                    {
                        if (boardState[i, x] >= 0)
                        {
                            pieceInWay = true;
                            break;
                        }
                    }
                }

            }
            if (boardState[position.y, position.x] >= 0)
            {
                continue;
            }
            else if(!pieceInWay)
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
