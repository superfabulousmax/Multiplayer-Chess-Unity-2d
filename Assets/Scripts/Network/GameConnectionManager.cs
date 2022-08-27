using System;
using Unity.Netcode;
using UnityEngine;

public class GameConnectionManager : MonoBehaviour
{
    private const int NumberAllowedConnections = 2;
    private int connectedClients = 0;
    public enum ConnectionStatus
    {
        Connected,
        Disconnected
    }

    public static GameConnectionManager Singleton { get; internal set; }

    public event Action<ulong, ConnectionStatus> OnClientConnectionNotification;

    public event Action OnGameReady;
    public event Action OnResetGame;

    private void Awake()
    {
        if (Singleton != null)
        {
            // As long as you aren't creating multiple NetworkManager instances, throw an exception.
            // (***the current position of the callstack will stop here***)
            throw new Exception($"Detected more than one instance of {nameof(GameConnectionManager)}! " +
                $"Do you have more than one component attached to a {nameof(GameObject)}");
        }
        Singleton = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnDestroy()
    {
        // Since the NetworkManager could potentially be destroyed before this component, only 
        // remove the subscriptions if the singleton still exists.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Client id {clientId} is connected");
        OnClientConnectionNotification?.Invoke(clientId, ConnectionStatus.Connected);
        connectedClients++;
        if(IsGameReady())
        {
            Debug.Log($"{NumberAllowedConnections} clients are now connected, ready to start.");
            OnGameReady?.Invoke();
        }
    }

    public void ResetGame()
    {
        if(IsGameReady())
        {
            OnResetGame?.Invoke();
        }
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Client id {clientId} is disconnected");
        OnClientConnectionNotification?.Invoke(clientId, ConnectionStatus.Disconnected);
        connectedClients--;
    }

    public bool IsGameReady()
    {
        return connectedClients == NumberAllowedConnections;
    }
}