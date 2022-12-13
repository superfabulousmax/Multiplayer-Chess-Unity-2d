using System;
using UnityEngine;
using UnityEngine.UIElements;
using static chess.enums.ChessEnums;

public class PawnPromotionButton : Button
{
    internal new class UxmlFactory : UxmlFactory<PawnPromotionButton, UxmlTraits> { }

    internal new class UxmlTraits : Button.UxmlTraits
    {
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
        }
    }

    public event Action<ChessPieceType> onClickButton;
    ChessPieceType chessPieceType;

    public PawnPromotionButton() : base ()
    {
        RegisterCallback<AttachToPanelEvent>(evt =>
        {
            var pieces = Enum.GetValues(typeof(ChessPieceType)) as ChessPieceType[];
            foreach (var piece in pieces)
            {
                if (viewDataKey == piece.ToString())
                {
                    chessPieceType = piece;
                }
            }
            clicked += OnClicked;
        });
        RegisterCallback<DetachFromPanelEvent>(evt =>
        {
            clicked -= OnClicked;
        });
    }

    public void SetSprite(ChessPieces pieces)
    {
        style.backgroundImage = new StyleBackground(pieces.GetSprite(chessPieceType));
    }

    private void OnClicked()
    {
        Debug.Log($"clicked on {chessPieceType}");
        onClickButton?.Invoke(chessPieceType);
    }
}
