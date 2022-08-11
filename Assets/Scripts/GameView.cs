using Unity.Netcode;
using UnityEngine;

public class GameView : NetworkBehaviour
{
    Player player;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        player = GetComponent<Player>();
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
        if (!IsOwner)
        {
            return;
        }

        if (player.Colour == PlayerColour.PlayerTwo)
        {
            Camera.main.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
    }

    [ClientRpc]
    void ChangePieceOrientationClientRpc()
    {
        if (!IsOwner)
        {
            return;
        }

        if (player.Colour == PlayerColour.PlayerTwo)
        {
            foreach (var piece in FindObjectsOfType<ChessPiece>())
            {
                piece.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }
    }
}
