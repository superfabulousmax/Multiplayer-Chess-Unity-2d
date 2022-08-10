using System;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;

[SelectionBase, RequireComponent(typeof(SpriteRenderer))]
public class ChessPiece : NetworkBehaviour
{
    public enum ChessPieceType { Pawn, King, Queen, Knight, Rook, Bishop };
    public enum ChessPieceStatus { Active, Captured };

    NetworkVariable<PlayerColour> playerColour = new(PlayerColour.Unassigned);
    NetworkVariable<ChessPieceType> pieceType = new(ChessPieceType.Pawn);
    NetworkVariable<ChessPieceStatus> pieceStatus = new(ChessPieceStatus.Active);
    NetworkVariable<Vector3Int> tilePosition = new(Vector3Int.zero, writePerm: NetworkVariableWritePermission.Owner);
    NetworkVariable<Vector3> piecePosition = new(Vector3.zero, writePerm: NetworkVariableWritePermission.Owner);
    NetworkVariable<uint> id = new(0, writePerm: NetworkVariableWritePermission.Owner);

    NetworkTransform networkTransform;

    SpriteRenderer spriteRenderer;

    public PlayerColour PlayerColour { get => playerColour.Value; private set => playerColour.Value = value; }
    public SpriteRenderer SpriteRenderer { get => spriteRenderer; set => spriteRenderer = value; }

    public Vector3Int TilePosition { get => tilePosition.Value; }
    public ChessPieceType PieceType { get => pieceType.Value; }
    public uint Id { get => id.Value; }

    [ClientRpc]
    public void SetTilePositionClientRpc(Vector3Int newTilePosition)
    {
        if (!IsOwner)
        {
            return;
        }
        tilePosition.Value = newTilePosition;
        var position = new Vector3(tilePosition.Value.x + 0.5f, tilePosition.Value.y + 0.5f, 0);
        piecePosition.Value = position;
        transform.position = piecePosition.Value;
        GetComponent<NetworkTransform>().transform.position = position;
    }

    [ServerRpc(RequireOwnership =false)]
    public void SetTilePositionServerRpc(Vector3Int newTilePosition)
    {
        if(!IsServer)
        {
            return;
        }
        SetTilePositionClientRpc(newTilePosition);
    }

    public override void OnNetworkSpawn()
    {
        networkTransform = GetComponent<NetworkTransform>();
    }

    internal void Init(PlayerColour colour, Sprite sprite, ChessPieceType type, uint id = default, Vector3Int tilePosition = default)
    {
        networkTransform = GetComponent<NetworkTransform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        playerColour.Value = colour;
        pieceType.Value = type;
        this.id .Value = id;
        this.tilePosition.Value = tilePosition;
    }

    public override string ToString()
    {
        return $"{playerColour.Value} {pieceType.Value} {pieceStatus.Value} {tilePosition.Value}";
    }
}
