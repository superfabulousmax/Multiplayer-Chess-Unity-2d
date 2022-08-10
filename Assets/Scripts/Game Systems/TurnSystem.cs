using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

public class TurnSystem : NetworkBehaviour
{
    [SerializeField]
    Context context;

    State whiteTurn;
    State blackTurn;

    public event Action<PlayerColour> onNextTurn;

    void Start()
    {
        whiteTurn = new WhiteTurnState();
        blackTurn = new BlackTurnState();

        Assert.IsTrue(whiteTurn != blackTurn);

        context = new Context(whiteTurn, blackTurn);
    }

    [ClientRpc]
    public void ChangeTurnClientRpc(int currentIndex)
    {  
        if (IsOwner)
        {
            return;
        }

        context.CurrentIndex = currentIndex;
        onNextTurn?.Invoke(GetActiveColour());
        Debug.Log($"Change turn {GetActiveColour()}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeTurnServerRpc()
    {
        var next = context.RequestNext();
        context.CurrentIndex = next;
        ChangeTurnClientRpc(next);
    }

    public PlayerColour GetActiveColour()
    {
        return context.CurrentState.PlayerColourState;
    }
}
