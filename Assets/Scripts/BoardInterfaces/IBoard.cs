using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using static chess.enums.ChessEnums;

public interface IBoard
{
    public Vector3Int CheckedPos { get; }
    public Tilemap BoardTileMap { get; }
    public FENChessNotation StartingSetup { get; }
    public IReadOnlyDictionary<uint, IChessPiece> ChessPiecesMap { get; }
    public IReadOnlyList<IChessPiece> ChessPiecesList { get; }
    public void AddPieceToBoard(IChessPiece piece);
    public void RemovePieceFromBoard(IChessPiece piece);
    public void FinishBoardSetup();
    public void ResetBoard();

    public int[,] GetBoardState();
    public Vector3Int GetIdPosition(uint id);
    public IChessPiece GetPieceAtPosition(Vector3Int position);
    public IChessPiece GetPieceFromId(uint id);
    public IReadOnlyList<IChessPiece> GetPiecesWith(PlayerColour playerColour, ChessPieceType chessPieceType);

    public bool IsValidPosition(Vector3Int position);
    // TODO have only one IsInCheck for interface?
    public bool IsInCheck(out IChessPiece checkedKing);
    public bool IsInCheck(int[,] simulatedBoard, out List<IChessPiece> kings);
    public bool IsCheckMate(PlayerColour activeColour);

    public bool CheckPiece(int id, ChessPieceType chessPieceType);
    public bool CheckSpaceAttacked(int id, PlayerColour activeColour, Vector3Int position);

    public void HandlePawnPromotion(IChessPiece piece, ChessPieceType chessPieceType);
    public void OnPawnPromoted(IChessPiece piece);
    public void TakePiece(IChessPiece piece, Vector3Int position);
    public bool ValidateMove(PlayerColour activePlayer, IChessPiece selectedChessPiece, Vector3Int tilePosition, out bool takenPiece);

}
