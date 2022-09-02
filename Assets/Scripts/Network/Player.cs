using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer.Samples.BossRoom;

public struct PlayerData : ISessionPlayerData, INetworkSerializable
{
    private bool isConnected;
    private ulong clientID;
    public bool IsConnected { get => isConnected; set => IsConnected = value; }
    public ulong ClientID { get => clientID; set => clientID = value; }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out isConnected);
            reader.ReadValueSafe(out clientID);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(isConnected);
            writer.WriteValueSafe(clientID);
        }
    }

    public void Reinitialize()
    {
    }

}

[RequireComponent(typeof(InputController))]
public class Player : NetworkBehaviour
{
    [SerializeField]
    NetworkVariable<PlayerColour> playerColour = new(PlayerColour.Unassigned, writePerm: NetworkVariableWritePermission.Server);

    public PlayerColour Colour { get => playerColour.Value; }

    public override void OnNetworkSpawn()
    {
        var playerNetworkColour = FindObjectOfType<PlayerNetworkColour>();
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
        OnServerEnded();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        OnServerEnded();
    }

    private void OnServerEnded()
    {
        if (IsOwnedByServer)
        {
            SessionManager<PlayerData>.Instance.OnSessionEnded();
            SessionManager<PlayerData>.Instance.OnServerEnded();
        }
    }

    [ClientRpc]
    private void AssignColourClientRPC(PlayerColour colour, ClientRpcParams clientRpcParams = default)
    {
        playerColour.Value = colour;
        Debug.Log($"Player {OwnerClientId} has colour {playerColour.Value}");
    }
}
