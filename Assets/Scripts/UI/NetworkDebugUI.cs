using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkDebugUI : MonoBehaviour
{
    // Start is called before the first frame update
    Button startHost;
    Button startServer;
    Button startClient;

    void Start()
    {
        var buttons = GetComponentsInChildren<Button>();

        Debug.Assert(buttons.Length == 3);

        startHost = buttons[0];
        startServer = buttons[1];
        startClient = buttons[2];

        startHost.onClick.AddListener(() => 
        { 
            if (NetworkManager.Singleton.StartHost())
            {
            }
            else
            {

            }
        });

        startServer.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {

            }
            else
            {

            }
        });

        startClient.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {

            }
            else
            {

            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
