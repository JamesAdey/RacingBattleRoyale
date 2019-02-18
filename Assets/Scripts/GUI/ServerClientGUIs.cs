using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerClientGUIs : MonoBehaviour
{


    [SerializeField]
    private GameObject[] serverGUIs;
    [SerializeField]
    private GameObject[] clientGUIs;

    void FixedUpdate()
    {
        bool server = NetworkCore.isServer;
        bool client = NetworkCore.isClient;
        for (int i = 0; i < serverGUIs.Length; i++)
        {
            serverGUIs[i].SetActive(server);
        }
        for (int i = 0; i < clientGUIs.Length; i++)
        {
            clientGUIs[i].SetActive(client);
        }
    }
}
