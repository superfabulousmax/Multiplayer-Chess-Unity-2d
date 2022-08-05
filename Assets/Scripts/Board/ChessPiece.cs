using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[SelectionBase, RequireComponent(typeof(SpriteRenderer))]
public class ChessPiece : NetworkBehaviour
{
    public enum ChessPieceType { Pawn, King, Queen, Knight, Rook, Bishop };

    SpriteRenderer spriteRenderer;

    PlayerColour playerColour;
    ChessPieceType chessPieceType;
    public PlayerColour PlayerColour { get => playerColour; private set => playerColour = value; }

    public void Init(ChessPieces chessPieces, Sprite image, ChessPieceType type)
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        playerColour = chessPieces.playerColour;
        spriteRenderer.sprite = image;
        chessPieceType = type;
    }

    private void OnMouseDown()
    {
        Debug.Log($"selected {PlayerColour} {chessPieceType}");
    }
    
    public void CopyBlueprint(ChessPiece blueprint)
    {
        playerColour = blueprint.playerColour;
        chessPieceType = blueprint.chessPieceType;
    }
}
