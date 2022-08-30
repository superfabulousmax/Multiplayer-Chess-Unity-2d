using Unity.Netcode;
using UnityEngine;

public class GameView : NetworkBehaviour
{
    Player player;
    Quaternion playerTwoRotation;

    public override void OnNetworkSpawn()
    {
        player = GetComponent<Player>();
        playerTwoRotation = Quaternion.Euler(0, 0, 180);
        GameConnectionManager.Singleton.OnGameReady += OnGameReady;
    }

    private void OnGameReady()
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
            foreach (var piece in FindObjectsOfType<ChessPiece>())
            {
                piece.transform.rotation = playerTwoRotation;
            }
        }

    }
}
