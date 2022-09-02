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
    private bool isWaiting;

    public event Action<PlayerColour> onFinishInput;

    public IPlayerInput PlayerInput { get => playerInput; private set => playerInput = value; }
    public PlayerColour Colour { get => player.Colour; }

    public override void OnNetworkSpawn()
    {
        player = GetComponent<Player>();
        board = FindObjectOfType<Board>();
        turnSystem = FindObjectOfType<TurnSystem>();
        activeInput = new ActivePlayerInput(board, OnInputFinished);
        playerInput = activeInput;
        turnSystem.onNextTurn += OnNextPlayerTurn;
        board.onFinishedBoardSetup += OnFinishedBoardSetup;
        isWaiting = false;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        turnSystem.onNextTurn -= OnNextPlayerTurn;
        board.onFinishedBoardSetup -= OnFinishedBoardSetup;
    }

    private void OnNextPlayerTurn(PlayerColour currentColour)
    {

    }

    private void OnFinishedBoardSetup()
    {
        var turn = board.PlacementSystem.StartingSetup.activeColour;
        turnSystem.SetTurn(turn);
    }

    private void Update()
    {
        if (isWaiting)
        {
            return;
        }
        if (IsServer)
        {
            InputClientRpc();
        }
        else
        {
            RequestInputServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
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
        if(!IsOwner)
        {
            return;
        }
        playerInput.HandleInput((int)NetworkObjectId, turnSystem.GetActiveColour(), Colour, IsOwner);
    }

    private void OnInputFinished()
    {
        Debug.Log($"{turnSystem.GetActiveColour()}" +
            $" Input Finished");
        board.DetectCheckServerRpc();
        turnSystem.ChangeTurnServerRpc();
    }
}
