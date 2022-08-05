using Unity.Netcode;
using UnityEngine;

public enum PlayerColour { PlayerOne = 0, PlayerTwo = 1, Unassigned  = 3, Assigned = 4}

public class PlayerNetworkColour: NetworkBehaviour
{
    public NetworkVariable<int> white = new((int)PlayerColour.Unassigned, writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> black = new((int)PlayerColour.Unassigned, writePerm: NetworkVariableWritePermission.Server);

    public PlayerColour GetColour()
    {
        if (white.Value == (int)PlayerColour.Assigned && black.Value == (int)PlayerColour.Assigned)
        {
            return PlayerColour.Assigned;
        }

        var colour = Random.Range(0, 2);

        if(colour == (int)PlayerColour.PlayerOne)
        {
            if(white.Value == (int)PlayerColour.Unassigned)
            {
                white.Value = (int)PlayerColour.Assigned;
                return PlayerColour.PlayerOne;
            }
            else
            {
                black.Value = (int)PlayerColour.Assigned;
                return PlayerColour.PlayerTwo;
            }
        }
        else
        {
            if (black.Value == (int)PlayerColour.Unassigned)
            {
                black.Value = (int)PlayerColour.Assigned;
                return PlayerColour.PlayerTwo;
            }
            else
            {
                white.Value = (int)PlayerColour.Assigned;
                return PlayerColour.PlayerOne;
            }
        }
    }
}
