using UnityEngine;
using Unity.Netcode;
using static ChessPiece;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.BossRoom;
using System;
using Unity.Collections;

public class PiecePlacementSystem : NetworkBehaviour
{
    [SerializeField]
    // starting fen -> rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
    string fenString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    [SerializeField]
    ChessPieces playerOnePieces;

    [SerializeField]
    ChessPieces playerTwoPieces;

    [SerializeField]
    GameObject chessPiecePrefab;

    Board chessBoard;

    FENChessNotation startingSetup;

    Dictionary<char, ChessPiece> chessPiecesMapping;

    private const string AllChessPieces = "rnbkqp";
    private const string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public FENChessNotation StartingSetup { get => startingSetup; }
    public ChessPieces PlayerOnePieces { get => playerOnePieces; }
    public ChessPieces PlayerTwoPieces { get => playerTwoPieces; }

    private ClientRpcParams clientRpcParams;

    public Sprite GetSpriteForPiece(PlayerColour playerColour, ChessPieceType chessPieceType)
    {
        if(playerColour == PlayerColour.PlayerOne)
        {
            return playerOnePieces.GetSprite(chessPieceType);
        }
        return playerTwoPieces.GetSprite(chessPieceType);
    }

    void OnEnable()
    {
        if (GameConnectionManager.Singleton != null)
        {
            GameConnectionManager.Singleton.OnGameReady += OnGameReady;
        }
    }

    private void OnDisable()
    {
        if (GameConnectionManager.Singleton != null)
        {
            GameConnectionManager.Singleton.OnGameReady -= OnGameReady;
        }
    }

    private void OnGameReady()
    {
        // Only the server spawns, clients will disable this component on their side
        enabled = IsServer;

        if (!enabled || chessPiecePrefab == null)
        {
            return;
        }

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var clientNetworkObject = NetworkManager.Singleton.ConnectedClients[client.ClientId].PlayerObject;
            var isServer = clientNetworkObject.GetComponent<Player>()?.IsOwnedByServer;
            var checkIsServer = isServer.HasValue && isServer.Value == true;

            if (!checkIsServer)
            {
                clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { client.ClientId }
                    }
                };
                GetClientDataClientRpc(client.ClientId, clientRpcParams);
            }
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void SetupPiecesServerRpc()
    {
        if(!IsOwner)
        {
            return;
        }

        chessBoard = FindObjectOfType<Board>();
        AssignBoardComponentClientRpc(chessBoard);
        AssignPlacementComponentClientRpc(this, fenString);
        chessPiecesMapping = new Dictionary<char, ChessPiece>();
        // Create black chess pieces with lower case
        CreateChessPieces(AllChessPieces);
        // Create white chess pieces with upper case
        CreateChessPieces(AllChessPieces.ToUpper());
        PlacePiecesOnBoardServerRpc();
        GetSpritesClientRpc();
        chessBoard.FinishBoardSetup();
    }

    [ClientRpc]
    public void GetClientDataClientRpc(ulong id, ClientRpcParams clientRpcParams)
    {
        const string IdKey = "PlayerGUID";
        var savedID = PlayerPrefs.GetString(IdKey, default);
        FindKeyServerRpc(id, savedID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void FindKeyServerRpc(ulong id, string clientId)
    {
        var playerData = SessionManager<PlayerData>.Instance.GetPlayerData(clientId);
        if (playerData == null)
        {
            var guid = Guid.NewGuid().ToString();
            SaveGuidClientRpc(guid, clientRpcParams);
            SessionManager<PlayerData>.Instance.SetupConnectingPlayerSessionData(id, guid, new PlayerData());
            SetupPiecesServerRpc();
        }
        else
        {
            ResumePlayClientRpc(clientRpcParams);
        }
    }

    [ClientRpc]
    private void SaveGuidClientRpc(string guid, ClientRpcParams clientRpcParams)
    {
        const string IdKey = "PlayerGUID";
        PlayerPrefs.SetString(IdKey, guid);
        PlayerPrefs.Save();
    }

    [ClientRpc]
    private void ResumePlayClientRpc(ClientRpcParams clientRpcParams)
    {
        ResetBoard();
        GetSprites();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetupConnectingPlayerServerRpc(ulong id, string guid, PlayerData data)
    {
        SessionManager<PlayerData>.Instance.SetupConnectingPlayerSessionData(id, guid, data);
    }

    internal void ResetBoard()
    {
        chessBoard.ResetBoard();
        foreach (var piece in FindObjectsOfType<ChessPiece>())
        {
            Debug.Log($"Reconnecting: adding: {piece}");
            chessBoard.AddPieceToBoard(piece);
        }
    }

    public void ResetGame()
    {
        enabled = IsServer;

        if (!enabled || chessPiecePrefab == null)
        {
            return;
        }

        chessBoard = FindObjectOfType<Board>();
        AssignBoardComponentClientRpc(chessBoard);
        AssignPlacementComponentClientRpc(this, StartingFen);
        chessPiecesMapping = new Dictionary<char, ChessPiece>();
        // Create black chess pieces with lower case
        CreateChessPieces(AllChessPieces);
        // Create white chess pieces with upper case
        CreateChessPieces(AllChessPieces.ToUpper());
        PlacePiecesOnBoardServerRpc();
        GetSpritesClientRpc();
        chessBoard.FinishBoardSetup();
    }

    [ClientRpc]
    private void AssignPlacementComponentClientRpc(NetworkBehaviourReference target, string fenString)
    {
        startingSetup = FENReader.ReadFENInput(fenString);
        if (target.TryGet(out PiecePlacementSystem piecePlacementSystem))
        {
            chessBoard.PlacementSystem = piecePlacementSystem;
        }
    }

    [ClientRpc]
    public void AssignBoardComponentClientRpc(NetworkBehaviourReference target)
    {
        if (IsServer)
        {
            return;
        }

        if (target.TryGet(out Board boardComponent))
        {
            chessBoard = boardComponent;
        }
    }

    [ClientRpc]
    public void GetSpritesClientRpc()
    {
        GetSprites();
    }

    private void GetSprites()
    {
        foreach (var piece in chessBoard.ChessPiecesList)
        {
            piece.SpriteRenderer.sprite = GetSpriteForPiece(piece.PlayerColour, piece.PieceType);
        }
    }

    private void CreateChessPieces(string pieces)
    {
        foreach (var piece in pieces)
        {
            var chessPiece = Instantiate(chessPiecePrefab, transform).GetComponent<ChessPiece>();

            if (char.IsUpper(piece))
            {
                InitChessPiece(piece, chessPiece, playerOnePieces);
            }
            else
            {
                InitChessPiece(piece, chessPiece, playerTwoPieces);
            }

            chessPiece.gameObject.SetActive(false);
            chessPiecesMapping.Add(piece, chessPiece);
        }
    }

    private void InitChessPiece(char piece, ChessPiece chessPiece, ChessPieces pieces)
    {
        switch (char.ToUpper(piece))
        {
            case 'R':
            {
                chessPiece.Init(pieces.playerColour, pieces.rook, ChessPieceType.Rook);
                break;
            }
            case 'N':
            {
                chessPiece.Init(pieces.playerColour, pieces.knight, ChessPieceType.Knight);
                break;
            }
            case 'B':
            {
                chessPiece.Init(pieces.playerColour, pieces.bishop, ChessPieceType.Bishop);
                break;
            }
            case 'K':
            {
                chessPiece.Init(pieces.playerColour, pieces.king, ChessPieceType.King);
                break;
            }
            case 'Q':
            {
                chessPiece.Init(pieces.playerColour, pieces.queen, ChessPieceType.Queen);
                break;
            }
            case 'P':
            {
                chessPiece.Init(pieces.playerColour, pieces.pawn, ChessPieceType.Pawn);
                break;
            }
            default:
            {
                Debug.LogError($"Invalid piece initial: {piece}");
                break;
            }
        }

    }

    [ServerRpc]
    private void PlacePiecesOnBoardServerRpc()
    {
        if (!IsServer)
        {
            return;
        }

        var allPositions = chessBoard.GetAllPositions();
        allPositions.MoveNext();
        var placements = startingSetup.piecePlacement.Split(FENChessNotation.delimeter);
        var currentBoardPosition = allPositions.Current;

        for (var i = placements.Length - 1; i >= 0; --i)
        {
            var row = placements[i];
            foreach (var item in row)
            {
                if (chessPiecesMapping.ContainsKey(item))
                {
                    var position = new Vector3(currentBoardPosition.x + chessBoard.BoardTileMap.cellSize.x * 0.5f, currentBoardPosition.y + chessBoard.BoardTileMap.cellSize.y * 0.5f, 0);
                    var chessPiece = Instantiate(chessPiecePrefab, position, Quaternion.identity).GetComponent<ChessPiece>();
                    var sprite = chessPiecesMapping[item].SpriteRenderer.sprite;
                    chessPiece.SetTilePositionServerRpc(currentBoardPosition);
                    chessPiece.Init(chessPiecesMapping[item].PlayerColour, chessPiecesMapping[item].SpriteRenderer.sprite, chessPiecesMapping[item].PieceType, currentBoardPosition);
                    chessPiece.gameObject.GetComponent<NetworkObject>().Spawn();
                    chessBoard.AddChessPieceToBoardServerRpc(chessPiece);
                    allPositions.MoveNext();
                    currentBoardPosition = allPositions.Current;
                }
                else
                {
                    // handle number
                    var emptySpaces = item - '0';
                    for (int j = 0; j < emptySpaces; ++j)
                    {
                        allPositions.MoveNext();
                        currentBoardPosition = allPositions.Current;
                    }
                }
            }
        }
    }
}
