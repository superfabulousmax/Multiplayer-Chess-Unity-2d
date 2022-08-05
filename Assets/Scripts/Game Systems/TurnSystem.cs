using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class TurnSystem : NetworkBehaviour
{
    State whiteTurn;
    State blackTurn;
    Context context;

    void Start()
    {
        whiteTurn = new WhiteTurnState();
        blackTurn = new BlackTurnState();
        Assert.IsTrue(whiteTurn != blackTurn);

        context = new Context(whiteTurn, blackTurn);
    }

    [ClientRpc]
    public void ChangeTurnClientRpc()
    {
        context.RequestNext();
    }

    internal bool IsTurn(PlayerColour playerColour)
    {
        return playerColour == context.CurrentState.PlayerColourState;
    }
}
