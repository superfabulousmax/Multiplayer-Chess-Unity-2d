using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackTurnState : State
{
    public override PlayerColour PlayerColourState => PlayerColour.PlayerTwo;

    public override bool Handle()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Black turn state");
            return true;
        }
        return false;
    }
}
