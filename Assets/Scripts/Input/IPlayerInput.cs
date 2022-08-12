using System;

public interface IPlayerInput
{
    public void HandleInput(int id, PlayerColour activeColour, PlayerColour currentColour, bool isOwner, Action onFinish);
}
