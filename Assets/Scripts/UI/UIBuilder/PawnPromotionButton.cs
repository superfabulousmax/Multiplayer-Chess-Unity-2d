using System;
using UnityEngine;
using UnityEngine.UIElements;
using static ChessPiece;

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

    public PawnPromotionButton() : base ()
    {
        RegisterCallback<AttachToPanelEvent>(evt =>
        {
            clicked += OnClicked;
        });
        RegisterCallback<DetachFromPanelEvent>(evt =>
        {
            clicked -= OnClicked;
        });
    }

    private void OnClicked()
    {
        var pieces = Enum.GetValues(typeof(ChessPieceType)) as ChessPieceType [];
        foreach (var piece in pieces)
        {
            if (viewDataKey == piece.ToString())
            {
                Debug.Log($"clicked on {piece}");
                onClickButton?.Invoke(piece);
            }
        }
    }
}
