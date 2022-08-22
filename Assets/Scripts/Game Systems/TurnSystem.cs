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

    [ServerRpc(RequireOwnership = false)]
    public void ChangeTurnServerRpc()
    {
        var next = context.RequestNext();
        context.CurrentIndex = next;
        ChangeTurnClientRpc(next);
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

    public PlayerColour GetActiveColour()
    {
        return context.CurrentState.PlayerColourState;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeContextConfigurationServerRpc()
    {
        ChangeContextConfigurationClientRpc();
    }

    [ClientRpc]
    private void ChangeContextConfigurationClientRpc()
    {
        context = new Context(blackTurn, whiteTurn);
    }

    internal void SetTurn(char turn)
    {
        if (turn == 'w')
        {
            return;
        }
        else if (turn == 'b')
        {
            ChangeContextConfigurationServerRpc();
        }
        else
        {
            Debug.LogError($"Unknown character {turn}, cannot set turn");
        }
    }
}
