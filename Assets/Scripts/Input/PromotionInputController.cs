using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PromotionInputController : MonoBehaviour
{
    [SerializeField]
    ChessPieces playerOnePieces;

    [SerializeField]
    ChessPieces playerTwoPieces;

    [SerializeField]
    Button[] buttons;

    internal void SetButtons(PlayerColour colour)
    {
        Assert.IsTrue(buttons.Length == 4);

        if (colour == PlayerColour.PlayerOne)
        {
            SetSprites(playerOnePieces);
        }
        else if (colour == PlayerColour.PlayerTwo)
        {
            SetSprites(playerTwoPieces);
        }

        SetButtonCallbacks();
    }

    private void SetButtonCallbacks()
    {
        buttons[0].onClick.AddListener(OnClickBishop);
        buttons[1].onClick.AddListener(OnClickBishop);
        buttons[2].onClick.AddListener(OnClickBishop);
        buttons[3].onClick.AddListener(OnClickBishop);
    }

    private void OnClickBishop()
    {

    }

    void SetSprites(ChessPieces chessPieces)
    {
        buttons[0].transform.GetComponent<Image>().sprite = chessPieces.bishop;
        buttons[1].transform.GetComponent<Image>().sprite = chessPieces.queen;
        buttons[2].transform.GetComponent<Image>().sprite = chessPieces.knight;
        buttons[3].transform.GetComponent<Image>().sprite = chessPieces.rook;
    }
}
