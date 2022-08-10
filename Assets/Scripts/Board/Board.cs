using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[RequireComponent(typeof(Tilemap))]
public class Board : MonoBehaviour
{
    Tilemap tilemap;

    public Tilemap BoardTileMap { get => tilemap; }
    public IReadOnlyDictionary<uint, ChessPiece> ChessPieces { get => chessPieces; }

    Dictionary<uint, ChessPiece> chessPieces;
    public event System.Action onFinishedBoardSetup;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        chessPieces = new Dictionary<uint, ChessPiece>();
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

    internal void AddPieceToBoard(uint id, ChessPiece chessPiece)
    {
        chessPieces.Add(id, chessPiece);
    }

    internal ChessPiece GetPieceFromId(uint id)
    {
        return chessPieces[id];
    }

    public void FinishBoardSetup()
    {
        onFinishedBoardSetup?.Invoke();
    }
}