using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class BoardCreator : MonoBehaviour
{
    [SerializeField]
    ChessTiles tiles;

    Tilemap tilemap;

    public static float tileWidth;
    public static float tileHeight;

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
        InitDimensions();
        CreateBoard();
    }

    public void InitDimensions()
    {
        Assert.AreEqual(tiles.first.rect.width, tiles.second.rect.width);
        Assert.AreEqual(tiles.first.rect.height, tiles.second.rect.height);

        tileWidth = tilemap.cellSize.x;
        tileHeight = tilemap.cellSize.y;
    }

    public void CreateBoard()
    {
        var prevSprite = tiles.second;
        var currentSprite = tiles.first;
        var skip = 2;

        for (var y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (var x = 0; x < GameConstants.BoardLengthDimension; x += skip)
            {
                var position = new Vector3Int(x, y);
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = currentSprite;
                tilemap.SetTile(position, tile);
            }
            for (var x = 1; x < GameConstants.BoardLengthDimension; x += skip)
            {
                var position = new Vector3Int(x, y);
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = prevSprite;
                tilemap.SetTile(position, tile);
            }
            var temp = currentSprite;
            currentSprite = prevSprite;
            prevSprite = temp;
        }

        tilemap.RefreshAllTiles();
    }
}
