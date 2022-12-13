using System;
using static chess.enums.ChessEnums;

public class InactivePlayerInput : IPlayerInput
{
    public void ClearHighlights(PlayerColour activeColour, PlayerColour currentColour, bool isOwner)
    {
    }

    public void HandleInput(int id, PlayerColour activeColour,PlayerColour currentColour, bool isOwner)
    {
    }
}
