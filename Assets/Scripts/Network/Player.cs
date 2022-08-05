using UnityEngine;
using Unity.Netcode;
using System;

public class Player : NetworkBehaviour
{
    NetworkVariable<PlayerColour> playerColour = new(PlayerColour.Unassigned, writePerm: NetworkVariableWritePermission.Server);
    NetworkVariable<bool> isReady = new(false, writePerm: NetworkVariableWritePermission.Server);

    PlayerNetworkColour playerNetworkColour;
    TurnSystem turnSystem;


    public override void OnNetworkSpawn()
    {
        turnSystem = FindObjectOfType<TurnSystem>();
        playerNetworkColour = FindObjectOfType<PlayerNetworkColour>();

        GameConnectionManager.Singleton.OnGameReady += OnGameReady;

        AssignColourClientRPC(playerNetworkColour.GetColour());

    }

    public override void OnNetworkDespawn()
    {
        GameConnectionManager.Singleton.OnGameReady -= OnGameReady;
    }

    private void OnGameReady()
    {
        isReady.Value = true;
    }

    private void Update()
    {
        if (!isReady.Value || !IsOwner || !turnSystem.IsTurn(playerColour.Value))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (IsServer)
            {
                InputClientRpc();
            }
            else
            {
                RequestInputServerRpc();
            }
        }
    }

    [ClientRpc]
    private void AssignColourClientRPC(PlayerColour colour, ClientRpcParams clientRpcParams = default)
    {
        // Run client-side logic here
        playerColour.Value = colour;
        Debug.Log($"Player {NetworkObjectId} has colour {playerColour.Value}");
    }


    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void RequestInputServerRpc()
    {
        if (!IsServer)
        {
            return;
        }
        InputClientRpc();
    }

    [ClientRpc]
    private void InputClientRpc()
    {
        if (ExecuteMove())
        {
            FinishTurn();
        }
    }

    private void FinishTurn()
    {
        turnSystem.ChangeTurnClientRpc();
    }

    private bool ExecuteMove()
    {
        Debug.Log($"Execute move for {NetworkObjectId}");
        return true;
    }
}
