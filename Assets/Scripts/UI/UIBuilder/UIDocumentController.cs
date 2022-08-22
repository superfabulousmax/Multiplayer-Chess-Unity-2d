using System.Linq;
using Unity.Netcode;
using UnityEngine.UIElements;

public class UIDocumentController : NetworkBehaviour
{
    UIDocument document;
    VisualElement root;
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
        foreach (var button in pawnPromotionButtons)
        {
            if(piece.PlayerColour == PlayerColour.PlayerOne)
            {
                button.SetSprite(board.PlacementSystem.PlayerOnePieces);
            }
            else
            {
                button.SetSprite(board.PlacementSystem.PlayerTwoPieces);
            }
        }
        root.style.display = DisplayStyle.Flex;
        cachedPiece = piece;
    }


    private void OnFinishPromotion(ChessPiece.ChessPieceType chessPieceType)
    {
        board.HandlePawnPromotionServerRpc(cachedPiece, chessPieceType);
        root.style.display = DisplayStyle.None;
    }
}

