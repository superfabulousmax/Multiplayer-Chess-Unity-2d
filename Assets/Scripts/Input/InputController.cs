using System;
using Unity.Netcode;
using UnityEngine;

public class InputController : NetworkBehaviour
{
    private IPlayerInput playerInput;
    private IPlayerInput activeInput;

    private Player player;
    private Board board;
    private TurnSystem turnSystem;

    public IPlayerInput PlayerInput { get => playerInput; private set => playerInput = value; }
    public PlayerColour Colour { get => player.Colour; }

    public event Action<PlayerColour> onFinishInput;

    public override void OnNetworkSpawn()
    {
        player = GetComponent<Player>();
        board = FindObjectOfType<Board>();
        turnSystem = FindObjectOfType<TurnSystem>();
        activeInput = new ActivePlayerInput(board, OnInputFinished);
        playerInput = activeInput;
        turnSystem.onNextTurn += OnNextPlayerTurn;
    }

    private void OnNextPlayerTurn(PlayerColour currentColour)
    {
        Debug.Log($"{currentColour} turn");
    }

    private void Update()
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

    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
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
        playerInput.HandleInput((int)NetworkObjectId, turnSystem.GetActiveColour(), Colour, IsOwner);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        turnSystem.onNextTurn -= OnNextPlayerTurn;
    }
    private void OnInputFinished()
    {
        Debug.Log($"{turnSystem.GetActiveColour()}" +
            $" Turn Finished");
        turnSystem.ChangeTurnServerRpc();
    }
}
