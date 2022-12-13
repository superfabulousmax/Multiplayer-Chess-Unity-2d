using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using static chess.enums.ChessEnums;
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
    private void ChangeContextToBlackConfigurationServerRpc()
    {
        ChangeContextToBlackConfigurationClientRpc();
    }

    [ClientRpc]
    private void ChangeContextToBlackConfigurationClientRpc()
    {
        context = new Context(blackTurn, whiteTurn);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeContextToWhiteConfigurationServerRpc()
    {
        context.CurrentIndex = 0;
        ChangeContextToWhiteConfigurationClientRpc();
    }

    [ClientRpc]
    private void ChangeContextToWhiteConfigurationClientRpc()
    {
        context = new Context(whiteTurn, blackTurn);
    }

    public void SetTurn(char turn)
    {
        if (turn == 'w')
        {
            ChangeContextToWhiteConfigurationServerRpc();
        }
        else if (turn == 'b')
        {
            ChangeContextToBlackConfigurationServerRpc();
        }
        else
        {
            Debug.LogError($"Unknown character {turn}, cannot set turn");
        }
    }
}
