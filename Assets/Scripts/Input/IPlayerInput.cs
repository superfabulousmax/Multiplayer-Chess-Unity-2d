using System;
using static chess.enums.ChessEnums;

public interface IPlayerInput
{
    public void HandleInput(int id, PlayerColour activeColour, PlayerColour currentColour, bool isOwner);
    public void ClearHighlights(PlayerColour activeColour, PlayerColour currentColour, bool isOwner);
}
