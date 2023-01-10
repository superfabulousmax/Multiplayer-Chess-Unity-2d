using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using static chess.enums.ChessEnums;
using System.Text;
using System.Linq;
using Unity.Multiplayer.Samples.BossRoom;

[RequireComponent(typeof(Tilemap))]
public class BoardNetworked : NetworkBehaviour, IBoard
{
    [SerializeField]
    BoardTileHighlighter tileHighlighter;

    Tilemap tilemap;
    ChessPiecesContainer chessPiecesContainer;

    IBoard board;

    public NetworkVariable<Vector3Int> checkedPos = new(-Vector3Int.one, writePerm: NetworkVariableWritePermission.Server);

    public Tilemap BoardTileMap { get => tilemap; }
    public FENChessNotation StartingSetup { get => chessPiecesContainer.StartingSetup; }
    public BoardTileHighlighter TileHighlighter { get => tileHighlighter; }
    public int [,] BoardState { get => board.GetBoardState(); }
    public Vector3Int CheckedPos { get => checkedPos.Value; }

    public IReadOnlyDictionary<uint, IChessPiece> ChessPiecesMap => board.ChessPiecesMap;

    public IReadOnlyList<IChessPiece> ChessPiecesList => board.ChessPiecesList;

    public event Action onFinishedBoardSetup;
    public event Action<ChessPieceNetworked> onPawnPromoted;
    public event Action<ChessPieceNetworked> onCheckMate;
    public event Action onResetBoard;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        chessPiecesContainer = ChessPiecesContainer.Singleton;
        board = new Board(tilemap, chessPiecesContainer);
    }

    public void Reset()
    {
        ResetBoardServerRpc();
    }

    public void ResetBoard()
    {
        board.ResetBoard();
    }

    public BoundsInt.PositionEnumerator GetAllPositions()
    {
        return tilemap.cellBounds.allPositionsWithin;
    }

    internal IReadOnlyList<IChessPiece> GetPiecesWith(PlayerColour playerColour, ChessPieceType chessPieceType)
    {
        return board.GetPiecesWith(playerColour, chessPieceType);
    }

    public bool CheckSpaceAttacked(int id, PlayerColour activeColour, Vector3Int position)
    {
        return board.CheckSpaceAttacked(id, activeColour, position);
    }

    public bool ValidateMove(PlayerColour activePlayer, IChessPiece selectedChessPiece, Vector3Int tilePosition, out bool takenPiece)
    {
        return board.ValidateMove(activePlayer, selectedChessPiece, tilePosition, out takenPiece);
    }

    internal bool IsValidPosition(Vector3Int position)
    {
        var tile = tilemap.GetTile(position);
        return tile != null;
    }

    public int [,] GetBoardState()
    {
        return board.GetBoardState();
    }

    public bool CheckPiece(int id, ChessPieceType chessPieceType)
    {
        return board.CheckPiece(id, chessPieceType);
    }

    public void FinishBoardSetup()
    {
        SetEnPassantTarget();
        onFinishedBoardSetup?.Invoke();
        DetectCheckServerRpc();
    }

    private void SetEnPassantTarget()
    {
        var target = chessPiecesContainer.StartingSetup.enPassantTargets;
        if (target.Length == 2)
        {
            var x = char.ToUpper(target[0]) - 'A';
            var y = (target[1] - '0') - 1;
            if (y == 3)
            {
                y++;
            }
            else if (y == 5)
            {
                y--;
            }
            var pawn = GetPieceAtPosition(new Vector3Int(x, y, 0));
            if (pawn != null)
            {
                pawn.SyncData(1, false, true, (uint)pawn.PieceId);
            }
        }
    }

    public bool IsInCheck(out IChessPiece checkedKing)
    {
        return board.IsInCheck(out checkedKing);
    }

    public bool IsInCheck(int [,] simulatedBoard, out List<IChessPiece> kings)
    {
        return board.IsInCheck(simulatedBoard, out kings);
    }

    public bool IsCheckMate(PlayerColour activeColour)
    {
        return board.IsCheckMate(activeColour);
    }

    public void AddPieceToBoard(IChessPiece piece)
    {
        board.AddPieceToBoard(piece);
    }

    public void RemovePieceFromBoard(IChessPiece piece)
    {
        board.RemovePieceFromBoard(piece);
    }

    public IChessPiece GetPieceAtPosition(Vector3Int position)
    {
        return board.GetPieceAtPosition(position);
    }

    public Vector3Int GetIdPosition(uint id)
    {
        return board.GetIdPosition(id);
    }

    public IChessPiece GetPieceFromId(uint id)
    {
        return board.GetPieceFromId(id);
    }

    IReadOnlyList<IChessPiece> IBoard.GetPiecesWith(PlayerColour playerColour, ChessPieceType chessPieceType)
    {
        return board.GetPiecesWith(playerColour, chessPieceType);
    }

    bool IBoard.IsValidPosition(Vector3Int position)
    {
        return board.IsValidPosition(position);
    }

    public void HandlePawnPromotion(IChessPiece piece, ChessPieceType chessPieceType)
    {
        HandlePawnPromotionServerRpc(piece as ChessPieceNetworked, chessPieceType);
    }

    public void OnPawnPromoted(IChessPiece piece)
    {
        onPawnPromoted?.Invoke(piece as ChessPieceNetworked);
    }

    public void TakePiece(IChessPiece piece, Vector3Int position)
    {
        TakePieceServerRpc(piece as ChessPieceNetworked, position);
    }

    public string GetBoardStateString()
    {
        var sbResult = new StringBuilder();
        var padding = "     ";
        for (var y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (var x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                board.ChessPiecesMap.TryGetValue((uint)board.GetBoardState()[y, x], out var piece);
                if (piece != null)
                {
                    var result = string.Format("{0,5}", DebugUtils.GetEncoding(piece.PlayerColour, piece.PieceType));
                    sbResult.Append($"{result}{padding}");
                }
                else
                {
                    var result = string.Format("{0,5}", "--");
                    sbResult.Append($"{result}{padding}");
                }
            }
            sbResult.Append($"\n");
        }
        return sbResult.ToString();
    }

    // RPCS
    [ServerRpc(RequireOwnership = false)]
    private void ResetBoardServerRpc()
    {
        for (var i = 0; i < board.ChessPiecesList.Count; i++)
        {
            var piece = board.ChessPiecesList[i] as ChessPieceNetworked;
            if (piece == null)
            {
                continue;
            }
            piece.GetComponent<NetworkObject>().Despawn();
        }

        checkedPos.Value = -Vector3Int.one;
        ResetBoardClientRpc();
        onResetBoard?.Invoke();
    }

    [ClientRpc]
    private void ResetBoardClientRpc()
    {
        ResetBoard();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakePieceServerRpc(NetworkBehaviourReference target, Vector3Int tilePosition)
    {
        if (!target.TryGet(out ChessPieceNetworked chessPieceComponent))
        {
            return;
        }
        var id = GetBoardState()[tilePosition.y, tilePosition.x];
        if (board.ChessPiecesMap.TryGetValue((uint)id, out var takenPiece))
        {
            var takenNetworkedPiece = takenPiece as ChessPieceNetworked;
            chessPieceComponent.SetTilePositionServerRpc(tilePosition);
            RemoveChessPieceToBoardServerRpc(takenNetworkedPiece);
            takenNetworkedPiece.GetComponent<NetworkObject>().Despawn();
        }
    }

    [ServerRpc]
    public void AddChessPieceToBoardServerRpc(NetworkBehaviourReference target)
    {
        AddChessPieceToBoardClientRpc(target);
    }

    [ClientRpc]
    public void AddChessPieceToBoardClientRpc(NetworkBehaviourReference target)
    {
        if (target.TryGet(out ChessPieceNetworked chessPieceComponent))
        {
            AddPieceToBoard(chessPieceComponent);
        }
    }

    [ServerRpc]
    public void RemoveChessPieceToBoardServerRpc(NetworkBehaviourReference target)
    {
        RemoveChessPieceToBoardClientRpc(target);
    }

    [ClientRpc]
    public void RemoveChessPieceToBoardClientRpc(NetworkBehaviourReference target)
    {
        if (target.TryGet(out ChessPieceNetworked chessPieceComponent))
        {
            RemovePieceFromBoard(chessPieceComponent);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DetectCheckServerRpc()
    {
        if (IsInCheck(out var king))
        {
            checkedPos.Value = king.Position;
            if (IsCheckMate(king.PlayerColour))
            {
                onCheckMate?.Invoke(GetPiecesWith(GetOppositeColour(king.PlayerColour), ChessPieceType.King).First() as ChessPieceNetworked);
                SessionManager<PlayerData>.Instance.OnSessionEnded();
            }
            else
            {
                TileHighlighter?.SetTileColourServerRpc(king.Position, Color.red);
            }
        }
        else
        {
            tileHighlighter?.SetTileColourServerRpc(checkedPos.Value, Color.clear);
            checkedPos.Value = -Vector3Int.one;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AskPawnPromotionServerRpc(NetworkBehaviourReference target, ServerRpcParams serverRpcParams = default)
    {
        // TODO cache this to avoid unnecessary memory alloc
        // NOTE! In case you know a list of ClientId's ahead of time, that does not need change,
        // Then please consider caching this (as a member variable), to avoid Allocating Memory every time you run this function
        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
            }
        };
        AskPawnProtionClientRpc(target, clientRpcParams);
    }

    [ClientRpc]
    public void AskPawnProtionClientRpc(NetworkBehaviourReference target, ClientRpcParams clientRpcParams)
    {
        if (target.TryGet(out ChessPieceNetworked chessPiece))
        {
            onPawnPromoted?.Invoke(chessPiece);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandlePawnPromotionServerRpc(NetworkBehaviourReference target, ChessPieceType chessPieceType)
    {
        HandlePawnProtionClientRpc(target, chessPieceType);
    }

    [ClientRpc]
    internal void HandlePawnProtionClientRpc(NetworkBehaviourReference target, ChessPieceType chessPieceType)
    {
        Debug.Log($"Promote pawn to {chessPieceType}");
        if (target.TryGet(out ChessPieceNetworked chessPiece))
        {
            (chessPiece as IChessPiece).ChangePieceTo(chessPieceType);
            DetectCheckServerRpc();
        }
    }
}