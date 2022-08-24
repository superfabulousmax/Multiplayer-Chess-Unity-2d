using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(InputController))]
public class Player : NetworkBehaviour
{
    [SerializeField]
    NetworkVariable<PlayerColour> playerColour = new(PlayerColour.Unassigned, writePerm: NetworkVariableWritePermission.Server);
    NetworkVariable<bool> isReady = new(false, writePerm: NetworkVariableWritePermission.Server);

    PlayerNetworkColour playerNetworkColour;

    public PlayerColour Colour { get => playerColour.Value;}

    public override void OnNetworkSpawn()
    {
        playerNetworkColour = FindObjectOfType<PlayerNetworkColour>();
        GameConnectionManager.Singleton.OnGameReady += OnGameReady;
#if TestingBlack
        if (IsOwnedByServer)
        {
            AssignColourClientRPC(PlayerColour.PlayerTwo);
        }
        else
        {
            AssignColourClientRPC(PlayerColour.PlayerOne);
        }
#elif TestingWhite
        if (IsOwnedByServer)
        {
            AssignColourClientRPC(PlayerColour.PlayerOne);
        }
        else
        {
            AssignColourClientRPC(PlayerColour.PlayerTwo);
        }
#else
        AssignColourClientRPC(playerNetworkColour.GetColour());
#endif
    }

    public override void OnNetworkDespawn()
    {
        GameConnectionManager.Singleton.OnGameReady -= OnGameReady;
    }

    private void OnGameReady()
    {
        isReady.Value = true;
    }

    [ClientRpc]
    private void AssignColourClientRPC(PlayerColour colour, ClientRpcParams clientRpcParams = default)
    {
        // Run client-side logic here
        playerColour.Value = colour;
        Debug.Log($"Player {OwnerClientId} has colour {playerColour.Value}");
    }
}
