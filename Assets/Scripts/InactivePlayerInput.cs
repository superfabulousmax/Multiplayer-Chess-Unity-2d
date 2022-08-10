using System;

public class InactivePlayerInput : IPlayerInput
{
    public InactivePlayerInput()
    {
    }

    public void HandleInput(int id, PlayerColour activeColour,PlayerColour currentColour, bool isOwner, Action onFinish)
    {
    }
}
