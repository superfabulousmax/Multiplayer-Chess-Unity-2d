using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using static ChessPiece;

[RequireComponent(typeof(Tilemap))]
public class Board : NetworkBehaviour
{
    Tilemap tilemap;

    [SerializeField]
    List<ChessPiece> chessPiecesList;

    PiecePlacementSystem piecePlacementSystem;

    ChessPiece playerOneKing;
    ChessPiece playerTwoKing;

    public Tilemap BoardTileMap { get => tilemap; }
    public IReadOnlyDictionary<uint, ChessPiece> ChessPieces { get => chessPiecesMap; }
    public IReadOnlyList<ChessPiece> ChessPiecesList { get => chessPiecesList; }
    public PiecePlacementSystem PlacementSystem { get => piecePlacementSystem; set => piecePlacementSystem = value; }
    public ChessPiece PlayerOneKing { get => playerOneKing; }
    public ChessPiece PlayerTwoKing { get => playerTwoKing; }

    internal ChessPiece GetOppositeKing(PlayerColour activeColour)
    {
        if (activeColour == PlayerColour.PlayerOne)
        {
            return GetKingForColour(PlayerColour.PlayerTwo);
        }
        return GetKingForColour(PlayerColour.PlayerOne);
    }

    internal ChessPiece GetKingForColour(PlayerColour colour)
    {
        if (colour == PlayerColour.PlayerOne)
        {
            return PlayerOneKing;
        }
        return playerTwoKing;
    }


    Dictionary<uint, ChessPiece> chessPiecesMap;

    public event Action onFinishedBoardSetup;
    public event Func<ChessPiece, bool, bool, Vector3Int, bool> onValidateMove;

    int [,] board;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        chessPiecesMap = new Dictionary<uint, ChessPiece>();
        chessPiecesList = new List<ChessPiece>(GameConstants.MaxPieces);

        board = new int[GameConstants.BoardLengthDimension, GameConstants.BoardLengthDimension];
    }

    public BoundsInt.PositionEnumerator GetAllPositions()
    {
        return tilemap.cellBounds.allPositionsWithin;
    }

    internal IReadOnlyList<ChessPiece> GetPieceWith(PlayerColour playerColour, ChessPieceType chessPieceType)
    {
        var result = new List<ChessPiece>();

        foreach (var piece in chessPiecesList)
        {
            if (piece.PlayerColour == playerColour && piece.PieceType == chessPieceType)
            {
                result.Add(piece);
            }
        }

        return result;
    }

    public Vector3Int GetTileAtMousePosition(Vector3 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        var plane = new Plane(Vector3.back, Vector3.zero);
        plane.Raycast(ray, out var hitDist);
        var point = ray.GetPoint(hitDist);
        return tilemap.WorldToCell(point);
    }

    internal bool ValidateMove(PlayerColour activePlayer, ChessPiece selectedChessPiece, Vector3Int tilePosition, out bool takenPiece)
    {
        return selectedChessPiece.ChessRuleBehaviour.PossibleMove(activePlayer, this, selectedChessPiece, tilePosition, out takenPiece);
    }


    internal void AddPieceToBoard(ChessPiece chessPiece)
    {
        if (chessPiece.PieceType == ChessPieceType.King && chessPiece.PlayerColour == PlayerColour.PlayerOne)
        {
            playerOneKing = chessPiece;
        }
        else if (chessPiece.PieceType == ChessPieceType.King && chessPiece.PlayerColour == PlayerColour.PlayerTwo)
        {
            playerTwoKing = chessPiece;
        }

        chessPiecesMap.Add((uint)chessPiece.NetworkObjectId, chessPiece);
        chessPiecesList.Add(chessPiece);
    }

    internal void RemovePieceToBoard(ChessPiece chessPiece)
    {
        chessPiecesMap.Remove((uint)chessPiece.NetworkObjectId);
        chessPiecesList.Remove(chessPiece);
    }

    [ServerRpc(RequireOwnership =false)]
    internal void TakePieceServerRpc(NetworkBehaviourReference target, Vector3Int tilePosition)
    {
        var id = GetBoardState()[tilePosition.y, tilePosition.x];
        if (target.TryGet(out ChessPiece chessPieceComponent))
        {
        }
        if (chessPiecesMap.TryGetValue((uint)id, out ChessPiece value))
        {
            chessPieceComponent.SetTilePositionServerRpc(tilePosition);
            RemoveChessPieceToBoardServerRpc(value);
            value.GetComponent<NetworkObject>().Despawn();
            GetBoardState()[tilePosition.y, tilePosition.x] = -1;
        }
    }

    [ServerRpc]
    public void AddChessPieceToBoardServerRpc(NetworkBehaviourReference target)
    {
        AddChessPieceToBoardClientRpc(target);
    }

    [ClientRpc]
    public void AddChessPieceToBoardClientRpc(NetworkBehaviourReference target)
    {
        if (target.TryGet(out ChessPiece chessPieceComponent))
        {
            AddPieceToBoard(chessPieceComponent);
        }
    }

    [ServerRpc]
    public void RemoveChessPieceToBoardServerRpc(NetworkBehaviourReference target)
    {
        RemoveChessPieceToBoardClientRpc(target);
    }

    [ClientRpc]
    public void RemoveChessPieceToBoardClientRpc(NetworkBehaviourReference target)
    {
        if (target.TryGet(out ChessPiece chessPieceComponent))
        {
            RemovePieceToBoard(chessPieceComponent);
        }
    }

    public int [,] GetBoardState()
    {
        var allPositions = GetAllPositions();
        allPositions.MoveNext();
        var currentBoardPosition = allPositions.Current;
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for(int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                allPositions.MoveNext();
                var piece = GetPieceAtPosition(currentBoardPosition);
                if(piece)
                {
                    board[y, x] = (int)piece.NetworkObjectId;
                }
                else
                {
                    board[y, x] = -1;
                }
                currentBoardPosition = allPositions.Current;
            }
        }
        return board;
    }

    public bool CheckPiece(int id, ChessPieceType chessPieceType)
    {
        var piece = GetPieceFromId((uint)id);
        return piece != null && piece.PieceType == chessPieceType;
    }

    public ChessPiece GetPieceAtPosition(Vector3Int position)
    {
        foreach(var piece in chessPiecesList)
        {
            if(piece.IsActive())
            {
                if (piece.TilePosition == position)
                    return piece;
            }
        }
        return null;
    }

    internal ChessPiece GetPieceFromId(uint id)
    {
        chessPiecesMap.TryGetValue(id, out var value);
        return value;
    }

    public void FinishBoardSetup()
    {
        SetStartingEnPassantTarget();
        onFinishedBoardSetup?.Invoke();
    }

    private void SetStartingEnPassantTarget()
    {
        var target = PlacementSystem.StartingSetup.enPassantTargets;
        if (target.Length == 2)
        {
            var x = char.ToUpper(target[0]) - 'A';
            var y = (target[1] - '0') - 1;
            if (y == 3)
            {
                y++;
            }
            else if (y == 6)
            {
                y--;
            }
            var pawn = GetPieceAtPosition(new Vector3Int(x, y, 0));
            if (pawn)
            {
                pawn.SyncDataServerRpc(1, false, true, (uint)pawn.NetworkObjectId);
            }
        }
    }

    internal bool IsInCheck(out ChessPiece checkedKing)
    {
        var allPositions = GetAllPositions();
        allPositions.MoveNext();
        var currentBoardPosition = allPositions.Current;
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                allPositions.MoveNext();
                var piece = GetPieceAtPosition(currentBoardPosition);
                if (piece)
                {
                    if (piece.CheckRuleBehaviour != null)
                    {
                        if (piece.CheckRuleBehaviour.PossibleCheck(this, GetBoardState(), piece, piece.TilePosition, out checkedKing))
                        {
                            Debug.Log($"{checkedKing} is in Check");
                            return true;
                        }
                    }
                }
                currentBoardPosition = allPositions.Current;
            }
        }
        checkedKing = null;
        return false;
    }

    internal bool IsInCheck(int [,] simulatedBoard, out ChessPiece king)
    {
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                var id = simulatedBoard[y, x];
                var piece = GetPieceFromId((uint)id);
                if (piece)
                {
                    if (piece.CheckRuleBehaviour != null)
                    {
                        if (piece.CheckRuleBehaviour.PossibleCheck(this, simulatedBoard, piece, new Vector3Int(x, y, 0), out king))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        king = null;
        return false;
    }

    internal bool IsCheckMate(PlayerColour activeColour)
    {
        var boardState = GetBoardState();
        foreach (var piece in chessPiecesList)
        {
            if (piece.PlayerColour == activeColour)
            {
                var result = CheckPieceCanMove(piece, boardState);
                if (result)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool CheckPieceCanMove(ChessPiece targetPiece, int[,] boardState)
    {
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                var id = boardState[y, x];
                if (id < 0)
                {
                    continue;
                }
                var piece = GetPieceFromId((uint)id);
             
                if (piece.PlayerColour == targetPiece.PlayerColour || id == (int)targetPiece.NetworkObjectId)
                {
                    continue;
                }
                if (targetPiece.ChessRuleBehaviour.PossibleMove(targetPiece.PlayerColour, this, targetPiece, new Vector3Int(x, y, 0), out var _))
                {
                    Debug.Log($"Possible for {targetPiece} to {new Vector3Int(x, y, 0)}");
                    return true;
                }
            }
        }

        return false;
    }

    internal Vector3Int GetIdPosition(uint id, int [,] boardState)
    {
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                if (id == boardState[y, x])
                {
                    return new Vector3Int(x, y, 0);
                }
            }
        }

        return -Vector3Int.one;
    }


    public override string ToString()
    {
        return $"Board has {chessPiecesList.Count} pieces";
    }

    [ServerRpc(RequireOwnership = false)]
    internal void HandlePawnPromotionServerRpc(NetworkBehaviourReference target, ChessPieceType type)
    {
        HandlePawnProtionClientRpc(target, type);

    }

    [ClientRpc]
    internal void HandlePawnProtionClientRpc(NetworkBehaviourReference target, ChessPieceType chessPieceType)
    {
        Debug.Log($"Promote pawn to {chessPieceType}");
        if (target.TryGet(out ChessPiece chessPiece))
        {
            chessPiece.ChangePieceTo(chessPieceType, PlacementSystem);
        }
    }
}