using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using System.Text;
using static chess.enums.ChessEnums;
using Unity.Multiplayer.Samples.BossRoom;

public class PiecePlacementSystem : NetworkBehaviour
{
    [SerializeField]
    GameObject chessPiecePrefab;

    BoardNetworked chessBoard;

    ChessPiecesContainer chessPieces;

    Dictionary<char, ChessPieceNetworked> chessPiecesMapping;

    private ClientRpcParams clientRpcParams;

    private const string AllChessPieces = "rnbkqp";

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

    public void Start()
    {
        chessPieces = ChessPiecesContainer.Singleton;
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
            var isServer = clientNetworkObject.GetComponent<NetworkObject>()?.IsOwnedByServer;
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

    public Sprite GetSpriteForPiece(PlayerColour playerColour, ChessPieceType chessPieceType)
    {
        if (playerColour == PlayerColour.PlayerOne)
        {
            return chessPieces.PlayerOnePieces.GetSprite(chessPieceType);
        }
        return chessPieces.PlayerTwoPieces.GetSprite(chessPieceType);
    }
  
    [ServerRpc(RequireOwnership = false)]
    public void SetupPiecesServerRpc()
    {
        if(!IsOwner)
        {
            return;
        }

        chessBoard = FindObjectOfType<BoardNetworked>();
        AssignBoardComponentClientRpc(chessBoard);
        chessPiecesMapping = new Dictionary<char, ChessPieceNetworked>();
        // Create black chess pieces with lower case
        CreateChessPieces(AllChessPieces);
        // Create white chess pieces with upper case
        CreateChessPieces(AllChessPieces.ToUpper());
        PlacePiecesOnBoardServerRpc();
        GetSpritesClientRpc();
        chessBoard.FinishBoardSetup();
        chessBoard.onResetBoard += OnResetBoard;
    }

    // todo setup pieces for single player
    // for testing purposes
    public void SetupTestBoard()
    {
        chessBoard = FindObjectOfType<BoardNetworked>();
        chessPiecesMapping = new Dictionary<char, ChessPieceNetworked>();
        // Create black chess pieces with lower case
        CreateChessPieces(AllChessPieces);
        // Create white chess pieces with upper case
        CreateChessPieces(AllChessPieces.ToUpper());
        PlacePiecesOnBoard();
        chessBoard.onResetBoard += OnResetBoard;
        chessBoard.FinishBoardSetup();
    }

    private void OnResetBoard()
    {
        ResetGame();
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
        if(!chessBoard)
        {
            chessBoard = FindObjectOfType<BoardNetworked>();
        }
        chessBoard.ResetBoard();
        foreach (var piece in FindObjectsOfType<ChessPieceNetworked>())
        {
            Debug.Log($"Reconnecting... adding: {piece}");
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

        chessBoard = FindObjectOfType<BoardNetworked>();
        AssignBoardComponentClientRpc(chessBoard);
        chessPiecesMapping = new Dictionary<char, ChessPieceNetworked>();
        // Create black chess pieces with lower case
        CreateChessPieces(AllChessPieces);
        // Create white chess pieces with upper case
        CreateChessPieces(AllChessPieces.ToUpper());
        PlacePiecesOnBoardServerRpc();
        GetSpritesClientRpc();
        chessBoard.FinishBoardSetup();
    }

    [ClientRpc]
    public void AssignBoardComponentClientRpc(NetworkBehaviourReference target)
    {
        if (IsServer)
        {
            return;
        }

        if (target.TryGet(out BoardNetworked boardComponent))
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
            (piece as ChessPieceNetworked).SpriteRenderer.sprite = GetSpriteForPiece(piece.PlayerColour, piece.PieceType);
        }
    }

    private void CreateChessPieces(string pieces)
    {
        foreach (var piece in pieces)
        {
            var chessPiece = Instantiate(chessPiecePrefab, transform).GetComponent<ChessPieceNetworked>();

            if (char.IsUpper(piece))
            {
                InitChessPiece(piece, chessPiece, chessPieces.PlayerOnePieces);
            }
            else
            {
                InitChessPiece(piece, chessPiece, chessPieces.PlayerTwoPieces);
            }

            chessPiece.gameObject.SetActive(false);
            chessPiecesMapping.Add(piece, chessPiece);
        }
    }

    private void InitChessPiece(char piece, ChessPieceNetworked chessPiece, ChessPieces pieces)
    {
        switch (char.ToUpper(piece))
        {
            case 'R':
            {
                chessPiece.Init(piece, pieces.playerColour, pieces.rook, ChessPieceType.Rook);
                break;
            }
            case 'N':
            {
                chessPiece.Init(piece, pieces.playerColour, pieces.knight, ChessPieceType.Knight);
                break;
            }
            case 'B':
            {
                chessPiece.Init(piece, pieces.playerColour, pieces.bishop, ChessPieceType.Bishop);
                break;
            }
            case 'K':
            {
                chessPiece.Init(piece, pieces.playerColour, pieces.king, ChessPieceType.King);
                break;
            }
            case 'Q':
            {
                chessPiece.Init(piece, pieces.playerColour, pieces.queen, ChessPieceType.Queen);
                break;
            }
            case 'P':
            {
                chessPiece.Init(piece, pieces.playerColour, pieces.pawn, ChessPieceType.Pawn);
                break;
            }
            default:
            {
                Debug.LogError($"Invalid piece initial: {piece}");
                break;
            }
        }

    }

    public string SaveFenToSO()
    {
        if (chessBoard == null)
            return string.Empty;
        var sb = new StringBuilder();
        for (var y = GameConstants.BoardLengthDimension - 1; y >= 0; y--)
        {
            var emptySpaces = 0;
            for (var x = 0; x <= GameConstants.BoardLengthDimension - 1; x++)
            {
                var id = chessBoard.BoardState[y, x];
                var piece = chessBoard.GetPieceFromId((uint)id);
                if (piece == null)
                {
                    emptySpaces++;
                }
                else
                {
                    if (emptySpaces > 0)
                    {
                        sb.Append(emptySpaces);
                    }
                    sb.Append(piece.Symbol);
                    emptySpaces = 0;

                } 
            }
            if (emptySpaces > 0)
            {
                sb.Append(emptySpaces);
            }
            sb.Append(FENChessNotation.delimeter);
        }

        Debug.Log($"{sb} FEN STRING TEST");
        return sb.ToString();
    }

    [ServerRpc]
    private void PlacePiecesOnBoardServerRpc()
    {
        if (!IsServer)
        {
            return;
        }
        PlacePiecesOnBoard();
    }

    private void PlacePiecesOnBoard()
    {
        var allPositions = chessBoard.GetAllPositions();
        allPositions.MoveNext();
        var placements = chessPieces.StartingSetup.piecePlacement.Split(FENChessNotation.delimeter);
        var currentBoardPosition = allPositions.Current;
        for (var i = placements.Length - 1; i >= 0; --i)
        {
            var row = placements[i];
            foreach (var item in row)
            {
                if (chessPiecesMapping.ContainsKey(item))
                {
                    var position = new Vector3(
                        currentBoardPosition.x + BoardCreator.tileWidth * 0.5f,
                        currentBoardPosition.y + BoardCreator.tileHeight * 0.5f,
                        0);
                    var chessPiece = Instantiate(chessPiecePrefab, position, Quaternion.identity).GetComponent<ChessPieceNetworked>();
                    var sprite = chessPiecesMapping[item].SpriteRenderer.sprite;
                    chessPiece.SetTilePositionServerRpc(currentBoardPosition);
                    chessPiece.Init(chessPiecesMapping[item].Symbol, chessPiecesMapping[item].PlayerColour, chessPiecesMapping[item].SpriteRenderer.sprite, chessPiecesMapping[item].PieceType, currentBoardPosition);
                    chessPiece.gameObject.GetComponent<NetworkObject>().Spawn();
                    chessBoard.AddChessPieceToBoardServerRpc(chessPiece);
                    allPositions.MoveNext();
                    currentBoardPosition = allPositions.Current;
                }
                else
                {
                    // handle number
                    var emptySpaces = item - '0';
                    for (var j = 0; j < emptySpaces; ++j)
                    {
                        allPositions.MoveNext();
                        currentBoardPosition = allPositions.Current;
                        Debug.Log($"{currentBoardPosition} current board position");
                    }
                }

                Debug.Log($"{currentBoardPosition} current board position");
            }
        }
    }
}
