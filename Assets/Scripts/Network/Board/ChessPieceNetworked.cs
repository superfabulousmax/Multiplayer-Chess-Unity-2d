using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using static chess.enums.ChessEnums;

[SelectionBase, RequireComponent(typeof(SpriteRenderer))]
public class ChessPieceNetworked : NetworkBehaviour, IChessPiece
{
    public PlayerColour PlayerColour { get => playerColour.Value; private set => playerColour.Value = value; }
    public ChessPieceType PieceType { get => pieceType.Value; }
    public Vector3Int Position { get => tilePosition.Value; set => tilePosition.Value = value; }
    public char Symbol { get => symbol.Value; private set => symbol.Value = value; }
    public SpriteRenderer SpriteRenderer { get => GetComponent<SpriteRenderer>(); }

    public IChessRule PieceRuleBehaviour { get => chessRuleBehaviour; set => chessRuleBehaviour = value; }
    public ICheckRule CheckRuleBehaviour { get => checkRuleBehaviour; set => checkRuleBehaviour = value; }
    public IMoveList MoveList { get => moveList; set => moveList = value; }

    public int PieceId 
    { 
        get 
        { 
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                return (int)NetworkObjectId;
            }
            return pieceId; 
        } 
        set => pieceId = value; 
    }

    public Transform PieceTransform => transform;

    // Network variables
    NetworkVariable<Vector3Int> tilePosition = new(Vector3Int.zero, writePerm: NetworkVariableWritePermission.Owner);
    NetworkVariable<PlayerColour> playerColour = new(PlayerColour.Unassigned);
    NetworkVariable<ChessPieceType> pieceType = new(ChessPieceType.Pawn);
    NetworkVariable<char> symbol = new('-');
    NetworkTransform networkTransform;

    // Non-network variables
    SpriteRenderer spriteRenderer;

    IChessRule chessRuleBehaviour;
    ICheckRule checkRuleBehaviour;
    IMoveList moveList;

    int pieceId;

    public override void OnNetworkSpawn()
    {
        InitComponents();
        AssignChessRules(pieceType.Value);
    }

    public override string ToString()
    {
        return $"{playerColour.Value} {pieceType.Value} {tilePosition.Value}";
    }

    private void AssignChessRules(ChessPieceType chessPieceType)
    {
        var moveToStopCheck = new MoveToStopCheck();
        switch (chessPieceType)
        {
            case ChessPieceType.Pawn:
                chessRuleBehaviour = new PawnChessPiece(new PawnPromotionRule(), moveToStopCheck, 
                                                        new TakePieceRule(chessPieceType), PlayerColour, tilePosition.Value);
                moveList = chessRuleBehaviour as IMoveList;
                checkRuleBehaviour = new PawnCheckRule();
                break;
            case ChessPieceType.King:
                chessRuleBehaviour = new KingChessPiece(moveToStopCheck, new CastleMoves());
                moveList = chessRuleBehaviour as IMoveList;
                checkRuleBehaviour = new KingCheckRule();
                break;
            case ChessPieceType.Queen:
                chessRuleBehaviour = new QueenChessPiece(new TakePieceRule(chessPieceType));
                moveList = chessRuleBehaviour as IMoveList;
                checkRuleBehaviour = new QueenCheckRule(new RookCheckRule(), new BishopCheckRule());
                break;
            case ChessPieceType.Rook:
                chessRuleBehaviour = new RookChessPiece(new TakePieceRule(chessPieceType), moveToStopCheck);
                moveList = chessRuleBehaviour as IMoveList;
                checkRuleBehaviour = new RookCheckRule();
                break;
            case ChessPieceType.Knight:
                chessRuleBehaviour = new KnightChessPiece(new TakePieceRule(chessPieceType), moveToStopCheck);
                moveList = chessRuleBehaviour as IMoveList;
                checkRuleBehaviour = new KnightCheckRule();
                break;
            case ChessPieceType.Bishop:
                chessRuleBehaviour = new BishopChessPiece(new TakePieceRule(chessPieceType), moveToStopCheck);
                moveList = chessRuleBehaviour as IMoveList;
                checkRuleBehaviour = new BishopCheckRule();
                break;
        }
    }

    public void Init(char piece, PlayerColour colour, Sprite sprite, ChessPieceType type, Vector3Int tilePosition = default)
    {
        InitComponents();
        symbol.Value = piece;
        playerColour.Value = colour;
        spriteRenderer.sprite = sprite;
        pieceType.Value = type;
        this.tilePosition.Value = tilePosition;
    }

    private void InitComponents()
    {
        networkTransform = GetComponent<NetworkTransform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SyncData(int moveCount, bool isFirstMove, bool firstMoveTwo, uint lastMovedPawnId)
    {
        SyncDataServerRpc(moveCount, isFirstMove, firstMoveTwo, lastMovedPawnId);
    }

    void IChessPiece.ChangePieceTo(ChessPieceType chessPieceType)
    {
        AssignChessRules(chessPieceType);

        spriteRenderer.sprite = ChessPiecesContainer.Singleton.GetSpriteForPiece(PlayerColour, chessPieceType);
    }

    public void SetPosition(Vector3Int position)
    {
        SetTilePositionServerRpc(position);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTilePositionServerRpc(Vector3Int newTilePosition)
    {
        if (!IsServer)
        {
            return;
        }

        SetTilePositionClientRpc(newTilePosition);
    }

    [ClientRpc]
    private void SetTilePositionClientRpc(Vector3Int newTilePosition)
    {
        if (!IsOwner)
        {
            return;
        }

        tilePosition.Value = newTilePosition;
        // todo
        var position = new Vector3(tilePosition.Value.x + 0.5f, tilePosition.Value.y + 0.5f, 0);
        networkTransform.transform.position = position;
    }

    [ServerRpc(RequireOwnership =false)]
    // TODO make this better but how? eeee
    internal void SyncDataServerRpc(int moveCount, bool isFirstMove, bool firstMoveTwo, uint lastMovedPawnId)
    {
        SyncDataClientRpc(moveCount, isFirstMove, firstMoveTwo, lastMovedPawnId);
    }

    [ClientRpc]
    private void SyncDataClientRpc(int moveCount, bool isFirstMove, bool firstMoveTwo, uint lastMovedPawnId)
    {
        if (chessRuleBehaviour is PawnChessPiece pawnChessPiece)
        {
            pawnChessPiece.MoveCount = moveCount;
            pawnChessPiece.IsFirstMove = isFirstMove;
            pawnChessPiece.FirstMoveTwo = firstMoveTwo;
            pawnChessPiece.LastMovedPawnID = lastMovedPawnId;
        }
        else if (chessRuleBehaviour is RookChessPiece rookChessPiece)
        {
            rookChessPiece.MoveCount = moveCount;
        }
        else if (chessRuleBehaviour is KingChessPiece kingChessPiece)
        {
            kingChessPiece.MoveCount = moveCount;
        }
    }
}
