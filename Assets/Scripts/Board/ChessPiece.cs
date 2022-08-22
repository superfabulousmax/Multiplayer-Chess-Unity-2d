using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

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

    ChessPieces chessPieces;

    [SerializeField]
    IChessRule chessRuleBehaviour;

    ICheckRule checkRuleBehaviour;

    public PlayerColour PlayerColour { get => playerColour.Value; private set => playerColour.Value = value; }
    public SpriteRenderer SpriteRenderer { get => spriteRenderer;  }

    public Vector3Int TilePosition { get => tilePosition.Value; }
    public ChessPieceType PieceType { get => pieceType.Value; }
    public IChessRule ChessRuleBehaviour { get => chessRuleBehaviour; set => chessRuleBehaviour = value; }
    public ICheckRule CheckRuleBehaviour { get => checkRuleBehaviour; set => checkRuleBehaviour = value; }
    public ChessPieces ChessPieces { get => chessPieces; set => chessPieces = value; }

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

    public override void OnNetworkSpawn()
    {
        networkTransform = GetComponent<NetworkTransform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        AssignChessRules(pieceType.Value);
    }

    public override string ToString()
    {
        return $"{playerColour.Value} {pieceType.Value} {pieceStatus.Value} {tilePosition.Value}";
    }

    internal void AssignChessRules(ChessPieceType chessPieceType)
    {
        switch (chessPieceType)
        {
            case ChessPieceType.Pawn:
                chessRuleBehaviour = new PawnChessPiece(new PawnPromotionRule(), new MoveToStopCheck(), PlayerColour, tilePosition.Value);
                checkRuleBehaviour = new PawnCheckRule();
                break;
            case ChessPieceType.King:
                chessRuleBehaviour = new KingChessPiece(new MoveToStopCheck(), new CastleRule());
                checkRuleBehaviour = new KingCheckRule();
                break;
            case ChessPieceType.Queen:
                chessRuleBehaviour = new QueenChessPiece(new TakePieceRule(ChessPieceType.Queen));
                checkRuleBehaviour = new QueenCheckRule(new RookCheckRule(), new BishopCheckRule());
                break;
            case ChessPieceType.Rook:
                chessRuleBehaviour = new RookChessPiece(new TakePieceRule(ChessPieceType.Rook), new MoveToStopCheck());
                checkRuleBehaviour = new RookCheckRule();
                break;
            case ChessPieceType.Knight:
                chessRuleBehaviour = new KnightChessPiece(new TakePieceRule(ChessPieceType.Knight), new MoveToStopCheck());
                checkRuleBehaviour = new KnightCheckRule();
                break;
            case ChessPieceType.Bishop:
                chessRuleBehaviour = new BishopChessPiece(new TakePieceRule(ChessPieceType.Bishop), new MoveToStopCheck());
                checkRuleBehaviour = new BishopCheckRule();
                break;
        }
    }

    internal void Init(ChessPieces chessPieces, PlayerColour colour, Sprite sprite, ChessPieceType type, Vector3Int tilePosition = default)
    {
        networkTransform = GetComponent<NetworkTransform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        this.chessPieces = chessPieces;
        playerColour.Value = colour;
        spriteRenderer.sprite = sprite;
        pieceType.Value = type;
        this.tilePosition.Value = tilePosition;
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
    private void SyncDataClientRpc(int moveCount, bool isFirstMove, bool firstMoveTwo, uint lastMovedPawnId)
    {
        if(chessRuleBehaviour is PawnChessPiece pawnChessPiece)
        {
            pawnChessPiece.MoveCount = moveCount;
            pawnChessPiece.IsFirstMove = isFirstMove;
            pawnChessPiece.FirstMoveTwo = firstMoveTwo;
            pawnChessPiece.LastMovedPawnID = lastMovedPawnId;
        }
        else if(chessRuleBehaviour is RookChessPiece rookChessPiece)
        {
            rookChessPiece.MoveCount = moveCount;
        }
        else if(chessRuleBehaviour is KingChessPiece kingChessPiece)
        {
            kingChessPiece.MoveCount = moveCount;
        }
    }

    internal void ChangePieceTo(ChessPieceType chessPieceType, PiecePlacementSystem placementSystem)
    {
        AssignChessRules(chessPieceType);
        if (chessPieces != null)
        {
            spriteRenderer.sprite = chessPieces.GetSprite(chessPieceType);
        }
        else
        {
            spriteRenderer.sprite =  placementSystem.GetSpriteForPiece(PlayerColour, chessPieceType);
        }
    }
}
