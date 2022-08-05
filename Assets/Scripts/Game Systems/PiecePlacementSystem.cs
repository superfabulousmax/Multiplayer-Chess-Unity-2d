using System.Collections.Generic;
using UnityEngine;

public class PiecePlacementSystem : MonoBehaviour
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

    ChessNotation startingSetup;

    private const string AllChessPieces = "rnbkqp";

    Dictionary<char, ChessPiece> chessPiecesMapping;

    void Start()
    {
        chessBoard = FindObjectOfType<Board>();
        startingSetup = FENReader.ReadFENInput(fenString);
        chessPiecesMapping = new Dictionary<char, ChessPiece>();
        // Create black chess pieces with lower case
        CreateChessPieces(AllChessPieces);
        // Create white chess pieces with upper case
        CreateChessPieces(AllChessPieces.ToUpper());
        PlacePiecesOnBoard();
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
                chessPiece.Init(pieces, pieces.rook, ChessPiece.ChessPieceType.Rook);
                break;
            }
            case 'N':
            {
                chessPiece.Init(pieces, pieces.knight, ChessPiece.ChessPieceType.Knight);
                break;
            }
            case 'B':
            {
                chessPiece.Init(pieces, pieces.bishop, ChessPiece.ChessPieceType.Bishop);
                break;
            }
            case 'K':
            {
                chessPiece.Init(pieces, pieces.king, ChessPiece.ChessPieceType.King);
                break;
            }
            case 'Q':
            {
                chessPiece.Init(pieces, pieces.queen, ChessPiece.ChessPieceType.Queen);
                break;
            }
            case 'P':
            {
                chessPiece.Init(pieces, pieces.pawn, ChessPiece.ChessPieceType.Pawn);
                break;
            }
            default:
            {
                Debug.LogError($"Invalid piece initial: {piece}");
                break;
            }
        }

    }

    private void PlacePiecesOnBoard()
    {
        var allPositions = chessBoard.GetAllPositions();
        var placements = startingSetup.piecePlacement.Split(ChessNotation.delimeter);
        allPositions.MoveNext();
        var currentBoardPosition = allPositions.Current;
        for (int i = placements.Length - 1; i >= 0; --i)
        {
            var row = placements[i];
            foreach (var item in row)
            {
                if (chessPiecesMapping.ContainsKey(item))
                {
                    var chessPiece = Instantiate(chessPiecesMapping[item], new Vector3(currentBoardPosition.x + chessBoard.BoardTileMap.cellSize.x  * 0.5f , currentBoardPosition.y + chessBoard.BoardTileMap.cellSize.y * 0.5f, 0), Quaternion.identity, transform);
                    chessPiece.gameObject.SetActive(true);
                    chessPiece.CopyBlueprint(chessPiecesMapping[item]);
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
