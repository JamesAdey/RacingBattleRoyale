using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NetObjectID : MonoBehaviour
{
    public bool isTransient = false;
    [SerializeField]
    public NetworkID netID;

    public List<BufferedMessage> bufferedMessages = new List<BufferedMessage>();

    public void SetNetID(NetworkID id)
    {
        Debug.Log("setting id " + id);
        netID = id;
    }

    void Start()
    {
        if (isTransient)
        {
            if (netID.type == NetworkIDType.scene)
            {
                netID = NetworkManager.singleton.MakeTransientID(this,true);
                Debug.Assert(netID.type == NetworkIDType.game, "Required a game ID, but didn't get one!");
            }
        }
    }

    internal void AddBufferedMessage(BufferedMessage msg)
    {
        bufferedMessages.Add(msg);
    }
}
