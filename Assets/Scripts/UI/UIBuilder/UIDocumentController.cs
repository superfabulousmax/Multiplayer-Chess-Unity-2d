using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class UIDocumentController : NetworkBehaviour
{
    UIDocument document;
    VisualElement root;
    TurnSystem turnSystem;
    Board board;

    PawnPromotionButton[] pawnPromotionButtons;

    ChessPiece cachedPiece;

    private void OnEnable()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;
        pawnPromotionButtons = root.Q("Container").Children().Where(child => child is PawnPromotionButton).Select(child => child as PawnPromotionButton).ToArray();
        foreach(var button in pawnPromotionButtons)
        {
            button.onClickButton += OnFinishPromotion;
        }
        root.style.display = DisplayStyle.None;
        turnSystem = FindObjectOfType<TurnSystem>();
        board = FindObjectOfType<Board>();
        board.onPawnPromoted += OnPawnPromoted;
    }

    private void OnDisable()
    {
        board.onPawnPromoted -= OnPawnPromoted;
        foreach (var button in pawnPromotionButtons)
        {
            button.onClickButton -= OnFinishPromotion;
        }
    }

    private void OnPawnPromoted(ChessPiece piece)
    {
        //NetworkManager.ConnectedClientsList.First().
        root.style.display = DisplayStyle.Flex;
        cachedPiece = piece;
    }

    private void OnFinishPromotion(ChessPiece.ChessPieceType chessPieceType)
    {
        board.HandlePawnPromotionServerRpc(cachedPiece, chessPieceType);
        root.style.display = DisplayStyle.None;
    }
}

