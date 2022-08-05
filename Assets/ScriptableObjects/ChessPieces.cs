using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChessPieces", menuName = "ScriptableObjects/Create Chess Piece Set", order = 1)]
public class ChessPieces : ScriptableObject
{
    public PlayerColour playerColour;
    public Sprite king;
    public Sprite queen;
    public Sprite rook;
    public Sprite knight;
    public Sprite bishop;
    public Sprite pawn;
}
