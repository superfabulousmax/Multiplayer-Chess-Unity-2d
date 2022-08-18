using System;
using Unity.Netcode;
using UnityEngine;

public class InputController : NetworkBehaviour
{
    private PromotionInputController promotionInputController;

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
        //promotionInputController = FindObjectOfType<PromotionInputController>();
        //promotionInputController.gameObject.SetActive(false);
        activeInput = new ActivePlayerInput(board, OnInputFinished, OnPromotion);
        playerInput = activeInput;
        turnSystem.onNextTurn += OnNextPlayerTurn;
        isWaiting = false;
    }

    private void OnNextPlayerTurn(PlayerColour currentColour)
    {
        Debug.Log($"{currentColour} turn");
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

    private void OnPromotion(PlayerColour promotedColour)
    {
        isWaiting = true;
        promotionInputController.gameObject.SetActive(true);
        promotionInputController.SetButtons(promotedColour);
    }


    private void OnInputFinished()
    {
        Debug.Log($"{turnSystem.GetActiveColour()}" +
            $" Turn Finished");
        turnSystem.ChangeTurnServerRpc();
    }
}
