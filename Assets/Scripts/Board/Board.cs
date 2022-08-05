using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Board : MonoBehaviour
{
    Tilemap board;

    public Tilemap BoardTileMap { get => board; }

    void Awake()
    {
        board = GetComponent<Tilemap>();
    }

    public BoundsInt.PositionEnumerator GetAllPositions()
    {
        return board.cellBounds.allPositionsWithin;
    }
}