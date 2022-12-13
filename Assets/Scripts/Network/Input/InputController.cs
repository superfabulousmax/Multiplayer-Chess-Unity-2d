using System;
using Unity.Netcode;
using UnityEngine;
using static chess.enums.ChessEnums;

public class InputController : NetworkBehaviour
{
    private Player player;
    private Board board;
    private TurnSystem turnSystem;
    private IPlayerInput playerInput;
    private bool isWaiting;

    public event Action<PlayerColour> onFinishInput;

    public PlayerColour Colour { get => player.Colour; }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        player = GetComponent<Player>();
        board = FindObjectOfType<Board>();
        turnSystem = FindObjectOfType<TurnSystem>();
        playerInput = new ActivePlayerInput(board, OnInputFinished);
        turnSystem.onNextTurn += OnNextPlayerTurn;
        board.onFinishedBoardSetup += OnFinishedBoardSetup;
        isWaiting = false;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner || turnSystem == null || board == null)
        {
            return;
        }
        Debug.Log($"IsOwner {IsOwner}");
        turnSystem.onNextTurn -= OnNextPlayerTurn;
        board.onFinishedBoardSetup -= OnFinishedBoardSetup;
    }

    private void OnNextPlayerTurn(PlayerColour currentColour)
    {

    }

    private void OnFinishedBoardSetup()
    {
        var turn = ChessPiecesContainer.Singleton.StartingSetup.activeColour;
        turnSystem.SetTurn(turn);
    }

    private void Update()
    {
        if (!IsOwner || !IsSpawned || isWaiting)
        {
            return;
        }
        HandleInput();
    }

    private void HandleInput()
    {
        playerInput.HandleInput((int)NetworkObjectId, turnSystem.GetActiveColour(), Colour, IsOwner);
    }

    private void OnInputFinished()
    {
        playerInput.ClearHighlights(turnSystem.GetActiveColour(), Colour, IsOwner);
        board.DetectCheckServerRpc();
        turnSystem.ChangeTurnServerRpc();
    }
}
