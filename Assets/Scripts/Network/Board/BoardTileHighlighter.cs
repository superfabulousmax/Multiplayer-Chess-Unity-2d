using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using System.Collections;

[RequireComponent(typeof(Tilemap))]
public class BoardTileHighlighter : NetworkBehaviour
{
    [SerializeField]
    Sprite whiteSquare;

    [SerializeField]
    private Color highlightColour = new Color(204 / 255.0f, 204 / 255.0f, 255 / 255.0f, 200 / 255.0f); // periwinkle
    [SerializeField]
    private Color clearColour = Color.clear;
    [SerializeField]
    private Color checkedColour = Color.red;
    [SerializeField]
    private Color possibleMoveColour = new Color(152 / 255.0f, 251 / 255.0f, 152 / 255.0f, 200 / 255.0f); // mint

    Tilemap tilemap;

    public Color HighlightColour { get => highlightColour; }
    public Color ClearColour { get => clearColour; }
    public Color CheckedColour { get => checkedColour; }
    public Color PossibleMoveColour { get => possibleMoveColour; }

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            tilemap.InsertCells(Vector3Int.zero, GameConstants.BoardLengthDimension, GameConstants.BoardLengthDimension, 1);
        }

        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                var position = new Vector3Int(x, y, 0);
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = whiteSquare;
                tilemap.SetTile(position, tile);
                tilemap.SetTileFlags(position, TileFlags.None);
                tilemap.SetColor(position, Color.clear);
            }
        }
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
            Debug.Log($"Set tile {position} {colour} on client {OwnerClientId}");
            tilemap.SetTileFlags(position, TileFlags.None);
            tilemap.SetColor(position, colour);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTileColourServerRpc(Vector3Int position, Color colour)
    {
        SetTileColourClientRpc(position, colour);
    }

    [ClientRpc]
    private void SetTileColourClientRpc(Vector3Int position, Color colour)
    {
        SetTileColour(position, colour);
    }
}
