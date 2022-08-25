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

    private bool isWaiting;

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
        Debug.Log($"{currentColour} turn");
    }

    private void OnFinishedBoardSetup()
    {
        var turn = board.PlacementSystem.StartingSetup.activeColour;
        Debug.Log($"Setting player turn to {turn}");
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
        playerInput.HandleInput((int)NetworkObjectId, turnSystem.GetActiveColour(), Colour, IsOwner);
    }

    private void OnPromotion(PlayerColour promotedColour)
    {
        isWaiting = true;
    }


    private void OnInputFinished()
    {
        Debug.Log($"{turnSystem.GetActiveColour()}" +
            $" Input Finished");
        board.DetectCheckServerRpc();
        turnSystem.ChangeTurnServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HighlightKingServerRpc(int x, int y, int z = 0)
    {
        HighlightKingClientRpc(x, y, z);
    }

    [ClientRpc]
    private void HighlightKingClientRpc(int x, int y, int z = 0)
    {
        board.TileHighlighter.SetTileColour(new Vector3Int(x, y, z), Color.red);
    }
}
