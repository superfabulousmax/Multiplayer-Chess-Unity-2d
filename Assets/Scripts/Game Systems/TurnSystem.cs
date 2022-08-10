using System;
using Unity.Netcode;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

public class TurnSystem : NetworkBehaviour
{
    State whiteTurn;
    State blackTurn;
    Context context;

    public event Action<PlayerColour> onNextTurn;

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
        if(!IsOwner)
        {
            return;
        }
        context.RequestNext();
        onNextTurn?.Invoke(context.CurrentState.PlayerColourState);
        Debug.Log($"Change turn {GetActiveColour()}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeTurnServerRpc()
    {
        if (!IsServer)
        {
            return;
        }
        ChangeTurnClientRpc();
    }

    public PlayerColour GetActiveColour()
    {
        return context.CurrentState.PlayerColourState;
    }
}
