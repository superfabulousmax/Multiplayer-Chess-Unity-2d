using UnityEngine;
using Unity.Netcode;
using static ChessPiece;
using System.Collections.Generic;

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

    public FENChessNotation StartingSetup { get => startingSetup; }
    public ChessPieces PlayerOnePieces { get => playerOnePieces; }
    public ChessPieces PlayerTwoPieces { get => playerTwoPieces; }

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

        chessBoard = FindObjectOfType<Board>();
        AssignBoardComponentClientRpc(chessBoard);
        AssignPlacementComponentClientRpc(this);
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
    private void AssignPlacementComponentClientRpc(NetworkBehaviourReference target)
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
        if(IsServer)
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
        foreach(var piece in chessBoard.ChessPiecesList)
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
                chessPiece.Init(pieces, pieces.playerColour, pieces.rook, ChessPieceType.Rook);
                break;
            }
            case 'N':
            {
                chessPiece.Init(pieces, pieces.playerColour, pieces.knight, ChessPieceType.Knight);
                break;
            }
            case 'B':
            {
                chessPiece.Init(pieces, pieces.playerColour, pieces.bishop, ChessPieceType.Bishop);
                break;
            }
            case 'K':
            {
                chessPiece.Init(pieces, pieces.playerColour, pieces.king, ChessPieceType.King);
                break;
            }
            case 'Q':
            {
                chessPiece.Init(pieces, pieces.playerColour, pieces.queen, ChessPieceType.Queen);
                break;
            }
            case 'P':
            {
                chessPiece.Init(pieces, pieces.playerColour, pieces.pawn, ChessPieceType.Pawn);
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

        for (int i = placements.Length - 1; i >= 0; --i)
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
                    chessPiece.Init(chessPiecesMapping[item].ChessPieces, chessPiecesMapping[item].PlayerColour, chessPiecesMapping[item].SpriteRenderer.sprite, chessPiecesMapping[item].PieceType, currentBoardPosition);
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
