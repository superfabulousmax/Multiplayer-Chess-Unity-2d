using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using static ChessPiece;
using System.Text;

[RequireComponent(typeof(Tilemap))]
public class Board : NetworkBehaviour
{
    [SerializeField]
    BoardTileHighlighter tileHighlighter;

    Tilemap tilemap;

    [SerializeField]
    List<ChessPiece> chessPiecesList;

    PiecePlacementSystem piecePlacementSystem;

    ChessPiece playerOneKing;
    ChessPiece playerTwoKing;

    Dictionary<uint, ChessPiece> chessPiecesMap;

    int [,] boardState;

    public NetworkVariable<Vector3Int> checkedPos = new(-Vector3Int.one, writePerm: NetworkVariableWritePermission.Server);

    public Tilemap BoardTileMap { get => tilemap; }
    public IReadOnlyDictionary<uint, ChessPiece> ChessPieces { get => chessPiecesMap; }
    public IReadOnlyList<ChessPiece> ChessPiecesList { get => chessPiecesList; }
    public PiecePlacementSystem PlacementSystem { get => piecePlacementSystem; set => piecePlacementSystem = value; }
    public ChessPiece PlayerOneKing { get => playerOneKing; }
    public ChessPiece PlayerTwoKing { get => playerTwoKing; }
    public BoardTileHighlighter TileHighlighter { get => tileHighlighter; }
    public int [,] BoardState { get => boardState; }
    public Vector3Int CheckedPos { get => checkedPos.Value; }

    public event Action onFinishedBoardSetup;
    public event Action<ChessPiece> onPawnPromoted;
    public event Action<ChessPiece> onCheckMate;

    internal ChessPiece GetOppositeKing(PlayerColour activeColour)
    {
        if (activeColour == PlayerColour.PlayerOne)
        {
            return GetKingForColour(PlayerColour.PlayerTwo);
        }
        return GetKingForColour(PlayerColour.PlayerOne);
    }

    internal ChessPiece GetKingForColour(PlayerColour colour)
    {
        if (colour == PlayerColour.PlayerOne)
        {
            return PlayerOneKing;
        }
        return playerTwoKing;
    }


    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        chessPiecesMap = new Dictionary<uint, ChessPiece>();
        chessPiecesList = new List<ChessPiece>(GameConstants.MaxPieces);
        boardState = new int[GameConstants.BoardLengthDimension, GameConstants.BoardLengthDimension];
    }

    public void Reset()
    {
        ResetBoardServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetBoardServerRpc()
    {
        for(int i = 0; i < chessPiecesList.Count; i++)
        {
            var piece = chessPiecesList[i];
            if (piece == null)
            {
                continue;
            }
            piece.GetComponent<NetworkObject>().Despawn();
        }
        ResetBoardClientRpc();
        checkedPos.Value = -Vector3Int.one;
        PlacementSystem.ResetGame();
    }

    [ClientRpc]
    private void ResetBoardClientRpc()
    {
        chessPiecesList.Clear();
        chessPiecesMap.Clear();
        playerOneKing = null;
        playerTwoKing = null;
    }

    public BoundsInt.PositionEnumerator GetAllPositions()
    {
        return tilemap.cellBounds.allPositionsWithin;
    }

    internal IReadOnlyList<ChessPiece> GetPieceWith(PlayerColour playerColour, ChessPieceType chessPieceType)
    {
        var result = new List<ChessPiece>();

        for (int i = 0; i < chessPiecesList.Count; i++)
        {
            var piece = chessPiecesList[i];
            if (piece != null && piece.PlayerColour == playerColour && piece.PieceType == chessPieceType)
            {
                result.Add(piece);
            }
        }

        return result;
    }

    internal bool CheckSpaceAttacked(PlayerColour activeColour, Vector3Int position)
    {
        for (int i = 0; i < chessPiecesList.Count; i++)
        {
            var piece = chessPiecesList[i];
            if (piece != null && piece.PlayerColour != activeColour)
            {
                if (piece.ChessRuleBehaviour.PossibleMove(piece.PlayerColour, this, piece, position, out var _, true))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Vector3Int GetTileAtMousePosition(Vector3 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        var plane = new Plane(Vector3.back, Vector3.zero);
        plane.Raycast(ray, out var hitDist);
        var point = ray.GetPoint(hitDist);
        return tilemap.WorldToCell(point);
    }

    internal bool ValidateMove(PlayerColour activePlayer, ChessPiece selectedChessPiece, Vector3Int tilePosition, out bool takenPiece)
    {
        return selectedChessPiece.ChessRuleBehaviour.PossibleMove(activePlayer, this, selectedChessPiece, tilePosition, out takenPiece);
    }

    internal void AddPieceToBoard(ChessPiece chessPiece)
    {
        if (chessPiece.PieceType == ChessPieceType.King && chessPiece.PlayerColour == PlayerColour.PlayerOne)
        {
            playerOneKing = chessPiece;
        }
        else if (chessPiece.PieceType == ChessPieceType.King && chessPiece.PlayerColour == PlayerColour.PlayerTwo)
        {
            playerTwoKing = chessPiece;
        }

        chessPiecesMap.Add((uint)chessPiece.NetworkObjectId, chessPiece);
        chessPiecesList.Add(chessPiece);
    }

    internal void RemovePieceToBoard(ChessPiece chessPiece)
    {
        chessPiecesMap.Remove((uint)chessPiece.NetworkObjectId);
        chessPiecesList.Remove(chessPiece);
    }

    internal bool IsValidPosition(Vector3Int position)
    {
        var tile = tilemap.GetTile(position);
        return tile != null;
    }

    [ServerRpc(RequireOwnership =false)]
    internal void TakePieceServerRpc(NetworkBehaviourReference target, Vector3Int tilePosition)
    {
        if (!target.TryGet(out ChessPiece chessPieceComponent))
        {
            return;
        }
        var id = GetBoardState()[tilePosition.y, tilePosition.x];
        if (chessPiecesMap.TryGetValue((uint)id, out ChessPiece takenPiece))
        {
            chessPieceComponent.SetTilePositionServerRpc(tilePosition);
            RemoveChessPieceToBoardServerRpc(takenPiece);
            takenPiece.GetComponent<NetworkObject>().Despawn();
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
        if (target.TryGet(out ChessPiece chessPieceComponent))
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
        if (target.TryGet(out ChessPiece chessPieceComponent))
        {
            RemovePieceToBoard(chessPieceComponent);
        }
    }

    public int [,] GetBoardState()
    {
        var allPositions = GetAllPositions();
        allPositions.MoveNext();
        var currentBoardPosition = allPositions.Current;
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for(int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                allPositions.MoveNext();
                var piece = GetPieceAtPosition(currentBoardPosition);
                if(piece)
                {
                    boardState[y, x] = (int)piece.NetworkObjectId;
                }
                else
                {
                    boardState[y, x] = -1;
                }
                currentBoardPosition = allPositions.Current;
            }
        }
        return boardState;
    }

    public bool CheckPiece(int id, ChessPieceType chessPieceType)
    {
        var piece = GetPieceFromId((uint)id);
        return piece != null && piece.PieceType == chessPieceType;
    }

    public ChessPiece GetPieceAtPosition(Vector3Int position)
    {
        for (int i = 0; i < chessPiecesList.Count; i++)
        {
            var piece = chessPiecesList[i];
            if (piece)
            {
                if (piece.TilePosition == position)
                    return piece;
            }
        }
        return null;
    }

    internal ChessPiece GetPieceFromId(uint id)
    {
        chessPiecesMap.TryGetValue(id, out var value);
        return value;
    }

    public void FinishBoardSetup()
    {
        SetStartingEnPassantTarget();
        onFinishedBoardSetup?.Invoke();
    }

    private void SetStartingEnPassantTarget()
    {
        var target = PlacementSystem.StartingSetup.enPassantTargets;
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
            if (pawn)
            {
                pawn.SyncDataServerRpc(1, false, true, (uint)pawn.NetworkObjectId);
            }
        }
    }

    internal bool IsInCheck(out ChessPiece checkedKing)
    {
        var boardState = GetBoardState();
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                var id = boardState[y, x];
                if (id < 0)
                {
                    continue;
                }
                var piece = GetPieceFromId((uint)id);
                if (piece.CheckRuleBehaviour != null)
                {
                    if (piece.CheckRuleBehaviour.PossibleCheck(this, boardState, piece, piece.TilePosition, out checkedKing))
                    {
                        Debug.Log($"{checkedKing} is in Check");
                        return true;
                    }
                }
            }
        }

        checkedKing = null;
        return false;
    }

    internal bool IsInCheck(int [,] simulatedBoard, out ChessPiece king)
    {
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                var id = simulatedBoard[y, x];
                var piece = GetPieceFromId((uint)id);
                if (piece && piece.CheckRuleBehaviour != null)
                {
                    if (piece.CheckRuleBehaviour.PossibleCheck(this, simulatedBoard, piece, new Vector3Int(x, y, 0), out king))
                    {
                        return true;
                    }
                }
            }
        }

        king = null;
        return false;
    }

    internal bool IsCheckMate(PlayerColour activeColour)
    {
        var boardState = GetBoardState();
        for (int i = 0; i < chessPiecesList.Count; i++)
        {
            var piece = chessPiecesList[i];
            if (piece != null && piece.PlayerColour == activeColour)
            {
                var result = CheckPieceCanMove(piece, boardState);
                if (result)
                {
                    return false;
                }
            }
        }

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DetectCheckServerRpc()
    {
        if (IsInCheck(out var king))
        {
            checkedPos.Value = king.TilePosition;
            if (IsCheckMate(king.PlayerColour))
            {
                onCheckMate?.Invoke(GetOppositeKing(king.PlayerColour));
            }
            else
            {
                TileHighlighter.SetTileColourServerRpc(king.TilePosition, Color.red);
            }
        }
        else
        {
            tileHighlighter.SetTileColourServerRpc(checkedPos.Value, Color.clear);
            checkedPos.Value = -Vector3Int.one;
        }
    }


    private bool CheckPieceCanMove(ChessPiece targetPiece, int[,] boardState)
    {
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                var id = boardState[y, x];
                if (id >= 0)
                {
                    var piece = GetPieceFromId((uint)id);

                    if (piece.PlayerColour == targetPiece.PlayerColour || id == (int)targetPiece.NetworkObjectId)
                    {
                        continue;
                    }
                }

                // todo use possible moves instead to avoid checking every tile for every piece
                if (targetPiece.ChessRuleBehaviour.PossibleMove(targetPiece.PlayerColour, this, targetPiece, new Vector3Int(x, y, 0), out var _, true))
                {
                    Debug.Log($"Possible for {targetPiece} to {new Vector3Int(x, y, 0)}");
                    return true;
                }
            }
        }

        return false;
    }

    internal Vector3Int GetIdPosition(uint id, int [,] boardState)
    {
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                if (id == boardState[y, x])
                {
                    return new Vector3Int(x, y, 0);
                }
            }
        }

        return -Vector3Int.one;
    }


    public override string ToString()
    {
        return $"Board has {chessPiecesList.Count} pieces";
    }

    [ServerRpc(RequireOwnership = false)]
    internal void AskPawnPromotionServerRpc(NetworkBehaviourReference target, ServerRpcParams serverRpcParams = default)
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
    internal void AskPawnProtionClientRpc(NetworkBehaviourReference target, ClientRpcParams clientRpcParams)
    {
        if (target.TryGet(out ChessPiece chessPiece))
        {
            onPawnPromoted?.Invoke(chessPiece);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    internal void HandlePawnPromotionServerRpc(NetworkBehaviourReference target, ChessPieceType chessPieceType)
    {
        HandlePawnProtionClientRpc(target, chessPieceType);
    }

    [ClientRpc]
    internal void HandlePawnProtionClientRpc(NetworkBehaviourReference target, ChessPieceType chessPieceType)
    {
        Debug.Log($"Promote pawn to {chessPieceType}");
        if (target.TryGet(out ChessPiece chessPiece))
        {
            chessPiece.ChangePieceTo(chessPieceType, PlacementSystem);
            DetectCheckServerRpc();
        }
    }

    public string PrintOutBoardState(int[,] boardState)
    {
        var sbResult = new StringBuilder();
        var padding = "     ";
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                if(boardState[y, x] == -1)
                {
                    var result = string.Format("{0,5:00}", 0);
                    sbResult.Append($"{result}{padding}");
                }
                else
                {
                    var result = string.Format("{0,5:00}", boardState[y, x]);
                    sbResult.Append($"{result}{padding}");
                }
            }
            sbResult.Append($"\n");
        }
        Debug.Log(sbResult.ToString());
        return sbResult.ToString();
    }

    public string GetBoardStateString()
    {
        var sbResult = new StringBuilder();
        var padding = "     ";
        for (int y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (int x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                var piece = GetPieceFromId((uint)boardState[y, x]);
                if (piece)
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

}