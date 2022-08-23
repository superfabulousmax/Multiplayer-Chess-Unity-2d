using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class BoardTileHighlighter : MonoBehaviour
{
    Tilemap tilemap;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void StartWaitThenSetColour(Vector3Int position, Color colour, float seconds = 1f)
    {
        StartCoroutine(WaitThenSetColour(position, colour, seconds));
    }

    private IEnumerator WaitThenSetColour(Vector3Int position, Color colour, float seconds = 1f)
    {
        yield return new WaitForSeconds(seconds);
        SetTileColour(position, colour);
    }

    public void SetTileColour(Vector3Int position, Color colour)
    {
        var tile = tilemap.GetTile(position);
        if (tile)
        {
            tilemap.SetTileFlags(position, TileFlags.None);
            tilemap.SetColor(position, colour);
        }
    }
}
