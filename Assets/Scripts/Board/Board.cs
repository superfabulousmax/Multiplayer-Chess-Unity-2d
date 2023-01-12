using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static chess.enums.ChessEnums;

/// <summary>
/// For all the non networked aspects of the chess board
/// </summary>
public class Board : IBoard
{   
    Tilemap tilemap;
    ChessPiecesContainer chessPiecesContainer;

    Dictionary<uint, IChessPiece> chessPiecesMap;

    int[,] boardState;

    private Vector3Int checkedPos;

    public Tilemap BoardTileMap { get => tilemap; }
    public IReadOnlyDictionary<uint, IChessPiece> ChessPiecesMap { get => chessPiecesMap; }
    public IReadOnlyList<IChessPiece> ChessPiecesList { get => chessPiecesMap.Values.ToList(); }
    public FENChessNotation StartingSetup { get => chessPiecesContainer.StartingSetup; }
    public int[,] BoardState { get => boardState; }
    public Vector3Int CheckedPos { get => checkedPos; }

    public event Action onFinishedBoardSetup;
    public event Action<IChessPiece> onPawnPromoted;
    public event Action<IChessPiece> onCheckMate;
    public event Action onResetBoard;

    public Board(Tilemap tilemap, ChessPiecesContainer chessPiecesContainer)
    {
        this.tilemap = tilemap;
        this.chessPiecesContainer = chessPiecesContainer;
        boardState = new int[GameConstants.BoardLengthDimension, GameConstants.BoardLengthDimension];
        chessPiecesMap = new Dictionary<uint, IChessPiece>();
    }

    public override string ToString()
    {
        return $"Board has {ChessPiecesList.Count} pieces";
    }

    public void AddPieceToBoard(IChessPiece piece)
    {
        chessPiecesMap.Add((uint)piece.PieceId, piece);
    }

    public void RemovePieceFromBoard(IChessPiece piece)
    {
        chessPiecesMap.Remove((uint)piece.PieceId);
    }

    public bool CheckPiece(int id, ChessPieceType chessPieceType)
    {
        var piece = GetPieceFromId((uint)id);
        return piece != null && piece.PieceType == chessPieceType;
    }

    public void FinishBoardSetup()
    {
        SetEnPassantTarget();
        onFinishedBoardSetup?.Invoke();
        DetectCheck();
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

    public void DetectCheck()
    {
        if (IsInCheck(out var king))
        {
            checkedPos = king.Position;
            if (IsCheckMate(king.PlayerColour))
            {
                onCheckMate?.Invoke(GetPiecesWith(GetOppositeColour(king.PlayerColour), ChessPieceType.King).First());
            }
            else
            {
                // TODO
                //TileHighlighter?.SetTileColourServerRpc(king.Position, Color.red);
            }
        }
        else
        {
            // TODO
            //tileHighlighter?.SetTileColourServerRpc(checkedPos, Color.clear);
            checkedPos = -Vector3Int.one;
        }
    }


    public int[,] GetBoardState()
    {
        var allPositions = GetAllPositions();
        allPositions.MoveNext();
        var currentBoardPosition = allPositions.Current;
        for (var y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (var x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                allPositions.MoveNext();
                var piece = GetPieceAtPosition(currentBoardPosition);
                if (piece != null)
                {
                    boardState[y, x] = piece.PieceId;
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

    public BoundsInt.PositionEnumerator GetAllPositions()
    {
        return tilemap.cellBounds.allPositionsWithin;
    }

    public Vector3Int GetIdPosition(uint id)
    {
        return chessPiecesMap[id].Position;
    }

    public IChessPiece GetPieceAtPosition(Vector3Int position)
    {
        return ChessPiecesList
            .Where(piece => piece != null && piece.Position == position)
            .FirstOrDefault();
    }

    public IChessPiece GetPieceFromId(uint id)
    {
        chessPiecesMap.TryGetValue(id, out var value);
        return value;
    }

    public void HandlePawnPromotion(IChessPiece piece, ChessPieceType chessPieceType)
    {
        piece.ChangePieceTo(chessPieceType);
        DetectCheck();
    }

    public bool IsCheckMate(PlayerColour activeColour)
    {
        for (var i = 0; i < ChessPiecesList.Count; i++)
        {
            var piece = ChessPiecesList[i];
            if (piece != null && piece.PlayerColour == activeColour)
            {
                if (CheckPieceCanMove(piece))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool CheckPieceCanMove(IChessPiece targetPiece)
    {
        var moveList = targetPiece.MoveList;
        if (moveList != null)
        {
            var possibleMoves = moveList.GetPossibleMoves(targetPiece.PlayerColour, this, targetPiece);
            return possibleMoves.Count > 0;
        }
        return false;
    }


    public bool IsInCheck(out IChessPiece checkedKing)
    {
        var boardState = GetBoardState();
        for (var y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (var x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                var id = boardState[y, x];
                if (id < 0)
                {
                    continue;
                }
                var piece = GetPieceFromId((uint)id);
                if (piece.CheckRuleBehaviour != null)
                {
                    if (piece.CheckRuleBehaviour.PossibleCheck(this, boardState, piece, piece.Position, out checkedKing))
                    {
                        return true;
                    }
                }
            }
        }

        checkedKing = null;
        return false;
    }

    public bool IsInCheck(int[,] simulatedBoard, out List<IChessPiece> kings)
    {
        kings = new List<IChessPiece>();
        // Note: need to use the simulatedBoard to get pieces 
        // as ChessPieceList does not have same piece set as the simulated board array
        for (var y = 0; y < GameConstants.BoardLengthDimension; y++)
        {
            for (var x = 0; x < GameConstants.BoardLengthDimension; x++)
            {
                var id = simulatedBoard[y, x];
                var piece = GetPieceFromId((uint)id);
                if (piece != null && piece.CheckRuleBehaviour != null)
                {
                    if (piece.CheckRuleBehaviour.PossibleCheck(this, simulatedBoard, piece, new Vector3Int(x, y, 0), out var king))
                    {
                        kings.Add(king);
                    }
                }
            }
        }

        return kings.Count > 0;
    }

    public bool IsValidPosition(Vector3Int position)
    {
        return tilemap.GetTile(position) != null;
    }

    public void ResetBoard()
    {
        chessPiecesMap.Clear();
    }

    public IReadOnlyList<IChessPiece> GetPiecesWith(PlayerColour playerColour, ChessPieceType chessPieceType)
    {
        var result = new List<IChessPiece>();

        for (var i = 0; i < ChessPiecesList.Count; i++)
        {
            var piece = ChessPiecesList[i];
            if (piece != null && piece.PlayerColour == playerColour && piece.PieceType == chessPieceType)
            {
                result.Add(piece);
            }
        }

        return result;
    }

    public bool CheckSpaceAttacked(int id, PlayerColour activeColour, Vector3Int position)
    {
        for (var i = 0; i < ChessPiecesList.Count; i++)
        {
            var piece = ChessPiecesList[i];
            if (piece == null || piece.PieceId == id || piece.PlayerColour == activeColour)
            {
                continue;
            }

            var possibleMoves = piece.MoveList.GetPossibleMoves(piece.PlayerColour, this, piece);
            if (possibleMoves.Contains(position))
            {
                return true;
            }
        }
        return false;
    }

    public void TakePiece(IChessPiece piece, Vector3Int position)
    {
        var takenID = GetBoardState()[position.y, position.x];
        if (chessPiecesMap.TryGetValue((uint)takenID, out IChessPiece takenPiece))
        {
            piece.SetPosition(position);
            RemovePieceFromBoard(takenPiece);
        }
    }

    // TODO
    public void OnPawnPromoted(IChessPiece piece)
    {
        onPawnPromoted?.Invoke(piece);
    }

    public bool ValidateMove(PlayerColour activePlayer, IChessPiece selectedChessPiece, Vector3Int tilePosition, out bool takenPiece)
    {
        return selectedChessPiece.PieceRuleBehaviour.PossibleMove(activePlayer, this, selectedChessPiece, tilePosition, out takenPiece);
    }
}
