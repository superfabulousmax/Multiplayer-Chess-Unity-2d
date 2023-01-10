using Unity.Netcode;
using UnityEngine;
using static chess.enums.ChessEnums;

public class GameView : NetworkBehaviour
{
    [SerializeField]
    BoardNetworked board;
    Player player;
    Quaternion playerTwoRotation;

    public override void OnNetworkSpawn()
    {
        player = GetComponent<Player>();
        board = FindObjectOfType<BoardNetworked>();
        if (board != null)
        {
            board.onFinishedBoardSetup += OnFinishedBoardSetup;
        }
        playerTwoRotation = Quaternion.Euler(0, 0, 180);
    }

    public override void OnNetworkDespawn()
    {
        if (board != null)
        {
            board.onFinishedBoardSetup -= OnFinishedBoardSetup;
        }
    }

    private void OnFinishedBoardSetup()
    {
        ChangeCameraViewClientRpc();
        ChangePieceOrientationClientRpc();
    }

    [ClientRpc]
    void ChangeCameraViewClientRpc()
    {
        if (!IsLocalPlayer)
        {
            return;
        }
        if (player.Colour == PlayerColour.PlayerTwo)
        {
            Camera.main.transform.rotation = playerTwoRotation;
        }
    }

    [ClientRpc]
    void ChangePieceOrientationClientRpc()
    {
        if (!IsLocalPlayer)
        {
            return;
        }
        if (player.Colour == PlayerColour.PlayerTwo)
        {
            foreach (var piece in board.ChessPiecesList)
            {
                piece.PieceTransform.rotation = playerTwoRotation;
            }
        }
    }
}
