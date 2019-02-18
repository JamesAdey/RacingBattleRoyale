using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetObjectID))]
public abstract class NetBehaviour : MonoBehaviour
{
    public NetworkID thisNetworkID
    {
        get
        {
            return GetComponent<NetObjectID>().netID;
        }
    }

    protected bool addedNetFunctions = false;

    /// <summary>
    /// Raises the new network scene loaded event. This should be used to subscribe function listeners.
    /// You should unsubscribe function listeners before destroying the object.
    /// </summary>
    public abstract void OnNewNetworkSceneLoaded();

    /// <summary>
    /// This function automatically dispatches ServerStart() and ClientStart() functions to separate behaviour
    /// </summary>
    public virtual void Start()
    {
        if (NetworkCore.isServer)
        {
            ServerStart();
        }
        if (NetworkCore.isClient)
        {
            ClientStart();
        }
    }

    /// <summary>
    /// This function replaces the default "Start" function on the server.
    /// </summary>
    public abstract void ServerStart();
    /// <summary>
    /// This function replaces the default "Start" function on the client.
    /// </summary>
    public abstract void ClientStart();
}
