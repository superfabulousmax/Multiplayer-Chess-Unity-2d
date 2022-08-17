using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System;

[SelectionBase, RequireComponent(typeof(SpriteRenderer))]
public class ChessPiece : NetworkBehaviour
{
    public enum ChessPieceType { Pawn, King, Queen, Knight, Rook, Bishop };
    public enum ChessPieceStatus { Active, Captured };

    NetworkVariable<PlayerColour> playerColour = new(PlayerColour.Unassigned);
    NetworkVariable<ChessPieceType> pieceType = new(ChessPieceType.Pawn);
    public NetworkVariable<ChessPieceStatus> pieceStatus = new(ChessPieceStatus.Active, writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<Vector3Int> tilePosition = new(Vector3Int.zero, writePerm: NetworkVariableWritePermission.Owner);
    NetworkVariable<Vector3> piecePosition = new(Vector3.zero, writePerm: NetworkVariableWritePermission.Owner);

    NetworkTransform networkTransform;

    SpriteRenderer spriteRenderer;

    [SerializeField]
    IChessRule chessRuleBehaviour;

    public PlayerColour PlayerColour { get => playerColour.Value; private set => playerColour.Value = value; }
    public SpriteRenderer SpriteRenderer { get => spriteRenderer;  }

    public Vector3Int TilePosition { get => tilePosition.Value; }
    public ChessPieceType PieceType { get => pieceType.Value; }
    public IChessRule ChessRuleBehaviour { get => chessRuleBehaviour; set => chessRuleBehaviour = value; }

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
        networkTransform.transform.position = position;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTilePositionServerRpc(Vector3Int newTilePosition)
    {
        if(!IsServer)
        {
            return;
        }

        SetTilePositionClientRpc(newTilePosition);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetCapturedServerRpc()
    {
        SetCaptured();
    }

    [ClientRpc]
    internal void DisablePieceClientRpc()
    {
        gameObject.SetActive(false);
    }

    void SetCaptured()
    {
        pieceStatus.Value = ChessPieceStatus.Captured;
    }

    public override void OnNetworkSpawn()
    {
        networkTransform = GetComponent<NetworkTransform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        switch (pieceType.Value)
        {
            case ChessPieceType.Pawn:
                chessRuleBehaviour = new PawnChessPiece(PlayerColour, tilePosition.Value);
                break;
            case ChessPieceType.King:
                chessRuleBehaviour = new KingChessPiece();
                break;
            case ChessPieceType.Queen:
                chessRuleBehaviour = new QueenChessPiece(new TakePieceRule(ChessPieceType.Queen));
                break;
            case ChessPieceType.Rook:
                chessRuleBehaviour = new RookChessPiece(new TakePieceRule(ChessPieceType.Rook));
                break;
            case ChessPieceType.Knight:
                chessRuleBehaviour = new KnightChessPiece(new TakePieceRule(ChessPieceType.Knight));
                break;
            case ChessPieceType.Bishop:
                chessRuleBehaviour = new BishopChessPiece(new TakePieceRule(ChessPieceType.Bishop));
                break;
        }

    }

    internal void Init(PlayerColour colour, Sprite sprite, ChessPieceType type, Vector3Int tilePosition = default)
    {
        networkTransform = GetComponent<NetworkTransform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        playerColour.Value = colour;
        pieceType.Value = type;
        this.tilePosition.Value = tilePosition;
        
    }

    public override string ToString()
    {
        return $"{playerColour.Value} {pieceType.Value} {pieceStatus.Value} {tilePosition.Value}";
    }

    internal bool IsActive()
    {
        return pieceStatus.Value == ChessPieceStatus.Active;
    }

    [ServerRpc(RequireOwnership =false)]
    internal void SyncDataServerRpc(int moveCount, bool isFirstMove, bool firstMoveTwo, uint lastMovedPawnId)
    {
        SyncDataClientRpc(moveCount, isFirstMove, firstMoveTwo, lastMovedPawnId);
    }

    [ClientRpc]
    internal void SyncDataClientRpc(int moveCount, bool isFirstMove, bool firstMoveTwo, uint lastMovedPawnId)
    {
        if(chessRuleBehaviour is PawnChessPiece pawnChessPiece)
        {
            pawnChessPiece.MoveCount = moveCount;
            pawnChessPiece.IsFirstMove = isFirstMove;
            pawnChessPiece.FirstMoveTwo = firstMoveTwo;
            pawnChessPiece.LastMovedPawnID = lastMovedPawnId;
        }
    }
}
