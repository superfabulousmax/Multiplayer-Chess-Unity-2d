using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Unity.Netcode;

[RequireComponent(typeof(Tilemap))]
public class Board : NetworkBehaviour
{
    Tilemap tilemap;

    [SerializeField]
    List<ChessPiece> chessPiecesList;

    private const int MaxPieces = 32;

    public Tilemap BoardTileMap { get => tilemap; }
    public IReadOnlyDictionary<uint, ChessPiece> ChessPieces { get => chessPiecesMap; }
    public IReadOnlyList<ChessPiece> ChessPiecesList { get => chessPiecesList; }

    Dictionary<uint, ChessPiece> chessPiecesMap;
    public event System.Action onFinishedBoardSetup;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        chessPiecesMap = new Dictionary<uint, ChessPiece>();
        chessPiecesList = new List<ChessPiece>(MaxPieces);
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
        var tilePosition = tilemap.WorldToCell(point);

        return tilePosition;
    }

    internal void AddPieceToBoard(ChessPiece chessPiece)
    {
        chessPiecesMap.Add((uint)chessPiece.NetworkObjectId, chessPiece);
        chessPiecesList.Add(chessPiece);
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

    internal ChessPiece GetPieceFromId(uint id)
    {
        return chessPiecesMap[id];
    }

    public void FinishBoardSetup()
    {
        onFinishedBoardSetup?.Invoke();
    }

    public override string ToString()
    {
        return $"Board has {chessPiecesList.Count} pieces";
    }
}