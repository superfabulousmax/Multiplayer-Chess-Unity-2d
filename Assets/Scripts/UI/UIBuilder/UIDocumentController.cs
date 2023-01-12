using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine.UIElements;
using UnityEngine;
using static chess.enums.ChessEnums;

public class UIDocumentController : NetworkBehaviour
{
    BoardNetworked board;
    ChessPieceNetworked cachedPiece;

    UIDocument document;
    VisualElement root;

    // pawn promotion elements
    VisualElement pawnPromotionContainer;
    PawnPromotionButton[] pawnPromotionButtons;

    // Check mate elements
    VisualElement checkMateContainer;
    Label winnerLabel;
    VisualElement winnerImage;
    Button retryButton;

    private void OnEnable()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;
        pawnPromotionContainer = root.Q("PawnPromotionContainer");
        pawnPromotionButtons = pawnPromotionContainer.Children().Where(child => child is PawnPromotionButton).Select(child => child as PawnPromotionButton).ToArray();
        checkMateContainer = root.Q("CheckMateContainer");
        winnerLabel = checkMateContainer.Q<Label>("WinnerLabel");
        winnerImage = checkMateContainer.Q("WinnerImage");
        retryButton = checkMateContainer.Q<Button>("RetryButton");

        retryButton.clicked += OnRetryClicked;

        foreach(var button in pawnPromotionButtons)
        {
            button.onClickButton += OnFinishPromotion;
        }

        checkMateContainer.style.display = DisplayStyle.None;
        pawnPromotionContainer.style.display = DisplayStyle.None;

        board = FindObjectOfType<BoardNetworked>();
        board.onPawnPromoted += OnPawnPromoted;
        board.onCheckMate += OnCheckMate;
    }

    private void OnRetryClicked()
    {
        HideCheckMateServerRpc();
        board.Reset();
    }

    private void OnDisable()
    {
        retryButton.clicked -= OnRetryClicked;
        board.onPawnPromoted -= OnPawnPromoted;
        foreach (var button in pawnPromotionButtons)
        {
            button.onClickButton -= OnFinishPromotion;
        }
        board.onCheckMate -= OnCheckMate;
    }

    private void OnPawnPromoted(ChessPieceNetworked piece)
    {
        Debug.Log("OnPawnPromoted");
        foreach (var button in pawnPromotionButtons)
        {
            if(piece.PlayerColour == PlayerColour.PlayerOne)
            {
                button.SetSprite(ChessPiecesContainer.Singleton.PlayerOnePieces);
            }
            else
            {
                button.SetSprite(ChessPiecesContainer.Singleton.PlayerTwoPieces);
            }
        }

        pawnPromotionContainer.style.display = DisplayStyle.Flex;
        cachedPiece = piece;
    }


    private void OnFinishPromotion(ChessPieceType chessPieceType)
    {
        board.HandlePawnPromotionServerRpc(cachedPiece, chessPieceType);
        pawnPromotionContainer.style.display = DisplayStyle.None;
    }

    private void OnCheckMate(ChessPieceNetworked piece)
    {
        ShowCheckMateServerRpc(piece);

    }

    [ServerRpc]
    private void ShowCheckMateServerRpc(NetworkBehaviourReference target)
    {
        ShowCheckMateClientRpc(target);
    }

    [ClientRpc]
    private void ShowCheckMateClientRpc(NetworkBehaviourReference target)
    {
        if(target.TryGet<ChessPieceNetworked>(out var piece))
        {
            pawnPromotionContainer.style.display = DisplayStyle.None;
            checkMateContainer.style.display = DisplayStyle.Flex;
            winnerLabel.text = $"{piece.PlayerColour} wins!";
            winnerImage.style.backgroundImage = new StyleBackground(piece.SpriteRenderer.sprite);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void HideCheckMateServerRpc()
    {
        HideCheckMateClientRpc();
    }

    [ClientRpc]
    private void HideCheckMateClientRpc()
    {
        checkMateContainer.style.display = DisplayStyle.None;
    }
}


