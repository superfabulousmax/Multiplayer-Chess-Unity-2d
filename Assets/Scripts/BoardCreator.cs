using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardCreator : MonoBehaviour
{
    [SerializeField]
    ChessTiles tiles;

    Tilemap tilemap;

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
        var prevSprite = tiles.second;
        var currentSprite = tiles.first;

        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x +=2)
            {
                var position = new Vector3Int(x, y, 0);
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = currentSprite;
                tilemap.SetTile(position, tile);
            }
            for (int x = 1; x < GameConstants.BoardLengthDimension; x += 2)
            {
                var position = new Vector3Int(x, y, 0);
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = prevSprite;
                tilemap.SetTile(position, tile);
            }
            var temp = currentSprite;
            currentSprite = prevSprite;
            prevSprite = temp;
        }
    }
}
