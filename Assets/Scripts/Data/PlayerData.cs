using Unity.Netcode;

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
