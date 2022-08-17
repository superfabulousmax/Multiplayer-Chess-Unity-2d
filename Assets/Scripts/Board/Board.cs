using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Unity.Netcode;
using System;
using static ChessPiece;

[RequireComponent(typeof(Tilemap))]
public class Board : NetworkBehaviour
{
    Tilemap tilemap;

    [SerializeField]
    List<ChessPiece> chessPiecesList;

    PiecePlacementSystem piecePlacementSystem;

    public Tilemap BoardTileMap { get => tilemap; }
    public IReadOnlyDictionary<uint, ChessPiece> ChessPieces { get => chessPiecesMap; }
    public IReadOnlyList<ChessPiece> ChessPiecesList { get => chessPiecesList; }
    public PiecePlacementSystem PlacementSystem { get => piecePlacementSystem; set => piecePlacementSystem = value; }

    Dictionary<uint, ChessPiece> chessPiecesMap;

    public event Action onFinishedBoardSetup;
    public event Func<ChessPiece, bool, bool, Vector3Int, bool> onValidateMove;

    private const int MaxPieces = 32;

    int [,] board;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        chessPiecesMap = new Dictionary<uint, ChessPiece>();
        chessPiecesList = new List<ChessPiece>(MaxPieces);
        board = new int[8, 8];
    }

    public BoundsInt.PositionEnumerator GetAllPositions()
    {
        return tilemap.cellBounds.allPositionsWithin;
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
        for (int y = 0; y < 8; y++)
        {
            for(int x = 0; x < 8; x++)
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

    public bool CheckPiece(int id, ChessPiece.ChessPieceType type)
    {
        var piece = GetPieceFromId((uint)id);
        return piece != null && piece.PieceType == type;
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
        onFinishedBoardSetup?.Invoke();
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