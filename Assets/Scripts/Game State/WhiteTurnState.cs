using UnityEngine;

public class WhiteTurnState : State
{
    public override PlayerColour PlayerColourState => PlayerColour.PlayerOne;

    public override bool Handle()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("White turn state");
            return true;
        }
        return false;
    }
}
