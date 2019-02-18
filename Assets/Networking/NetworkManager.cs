using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public delegate void NetkFunction(NetReader reader);

[System.Flags]
internal enum NetFuncType
{
    none = 0,
    // the function can only be called on the client
    calledOnClient = 1,
    // the function can only be called on the server
    calledOnServer = 2
}

/// <summary>
/// Holds a bunch of parameters for network functions
/// </summary>
[System.Serializable]
internal struct NetEditorFunction
{
    public string name;
    public bool calledOnClient;
    public bool calledOnServer;

    public NetFuncType FuncType
    {
        get
        {
            NetFuncType t = NetFuncType.none;
            if (calledOnClient)
            {
                t = t | NetFuncType.calledOnClient;
            }
            if (calledOnServer)
            {
                t = t | NetFuncType.calledOnServer;
            }
            return t;
        }
    }
}

/// <summary>
/// Holds a bunch of parameters for network functions
/// </summary>
[System.Serializable]
public struct NetEditorPrefab
{
    public string name;
    public GameObject clientPrefab;
    public GameObject serverPrefab;
}

public class NetworkPrefab
{
    public readonly string name;
    public readonly int id;
    public readonly GameObject clientPrefab;
    public readonly GameObject serverPrefab;

    internal NetworkPrefab(int id, NetEditorPrefab netEditorPrefab)
    {
        this.id = id;
        this.name = netEditorPrefab.name;
        this.clientPrefab = netEditorPrefab.clientPrefab;
        this.serverPrefab = netEditorPrefab.serverPrefab;
    }
}

class NetworkFunction
{
    public string name;
    public NetFuncType funcType;
    public int id;
    Dictionary<NetworkID, NetkFunction> listeners;

    public NetworkFunction(string _name, int _id, NetFuncType typ)
    {
        name = _name;
        id = _id;
        funcType = typ;
        listeners = new Dictionary<NetworkID, NetkFunction>();
    }

    internal bool CallFunction(NetworkID targetId, NetReader reader)
    {
        NetkFunction fun;
        if (listeners.TryGetValue(targetId, out fun))
        {
            fun(reader);
            return true;
        }
        Debug.LogError("No mapping for " + targetId + " to function " + name);
        return false;
    }

    internal void AddListener(NetworkID id, NetkFunction fun)
    {
        listeners.Add(id, fun);
    }

    internal bool RemoveListener(NetworkID id)
    {
        return listeners.Remove(id);
    }

    internal void PrintListeners()
    {
        foreach (var listener in listeners)
        {
            Debug.Log(name + "|" + listener.Key);
        }
    }
}

/// <summary>
/// An event that takes no parameters
/// </summary>
public delegate void VoidEvent();

/// <summary>
/// An event that takes 1 integer as parameter
/// </summary>
public delegate void IntEvent(int i);

public abstract class NetworkManager : NetBehaviour
{

    public static NetworkManager singleton
    {
        get
        {
            return _singleton;
        }
    }

    private static NetworkManager _singleton;
    [SerializeField]
    private NetEditorFunction[] editorFunctions;
    [SerializeField]
    private NetEditorPrefab[] editorPrefabs;

    private static List<NetworkPrefab> networkPrefabs = new List<NetworkPrefab>();

    // mapping of names to functions
    private static List<NetworkFunction> networkFunctions = new List<NetworkFunction>(256);

    // a list of buffered messages
    static private List<BufferedMessage> bufferedMessages = new List<BufferedMessage>(64);

    // whether we have received all the starting data
    public static bool HasJoinedGame { get; private set; }

    /// <summary>
    /// Occurs every network update.
    /// </summary>
    public static event VoidEvent NetworkUpdateEvent;

    public static event VoidEvent ServerStartEvent;

    public static event VoidEvent ClientStartEvent;

    /// <summary>
    /// Fired when a new client has connected to the server
    /// </summary>
    public static event IntEvent ClientConnectEvent;

    public static event IntEvent ClientDisconnectEvent;

    public static int FindIdForName(string name)
    {
        for (int i = 0; i < networkFunctions.Count; i++)
        {
            if (networkFunctions[i].name == name)
            {
                return i;
            }
        }
        Debug.LogError("Unknown network function: " + name);
        return -1;
    }

    public string FindNameForId(int id)
    {
        if (id >= 0 && id < networkFunctions.Count)
        {
            return networkFunctions[id].name;
        }
        Debug.LogError("Couldn't find function id: " + id);
        return null;
    }

    private void RegisterAllFunctions()
    {
        RegisterNewNetworkFunction("cl_NetworkInstantiate", NetFuncType.calledOnClient);
        RegisterNewNetworkFunction("cl_NetworkDestroy", NetFuncType.calledOnClient);
        RegisterNewNetworkFunction("cl_NotifyConnected", NetFuncType.calledOnClient);
        for (int i = 0; i < editorFunctions.Length; i++)
        {
            RegisterNewNetworkFunction(editorFunctions[i].name, editorFunctions[i].FuncType);
        }
    }

    internal void RegisterNewNetworkFunction(string name, NetFuncType type)
    {
        NetworkFunction fun = new NetworkFunction(name, networkFunctions.Count, type);
        networkFunctions.Add(fun);
    }

    public static bool AddNetFunctionListener(string functionName, NetworkID netId, NetkFunction listener)
    {
        //Debug.Log("adding listener for " + functionName + "|" + netId);
        int id = FindIdForName(functionName);
        if (id != -1)
        {
            networkFunctions[id].AddListener(netId, listener);
            return true;
        }
        Debug.LogError("Failed to add listener for " + functionName + " as no such function exists!");
        return false;

    }

    public static bool RemoveNetFunctionListener(string functionName, NetworkID netId)
    {
        int id = FindIdForName(functionName);
        if (id != -1)
        {
            return networkFunctions[id].RemoveListener(netId);
        }
        Debug.LogError("Couldn't unsubscribe to function " + functionName + "as it doesn't exist");
        return false;
    }

    public static void PrintMappings()
    {
        foreach (var fun in networkFunctions)
        {
            fun.PrintListeners();
        }
    }

    void Awake()
    {
        if (singleton)
        {
            Destroy(gameObject);
        }
        else
        {
            _singleton = this;
            ServerStartEvent += _onServerStarted;
            ClientStartEvent += _onClientStarted;
            Init();
        }
    }

    void OnDestroy()
    {
        if (_singleton == this)
        {
            _singleton = null;
            ServerStartEvent -= _onServerStarted;
            ClientStartEvent -= _onClientStarted;
        }
    }

    private static NetReader reader = null;

    public void ProcessData(byte[] data, int length, int connectionId)
    {

        // structure of a network message
        // -- header (4 byte)--
        // id number 		1 byte
        // id type			1 byte
        // function id		2 bytes
        // -- data --
        // rest is data		...

        // setup the reader
        reader.Reset(data, length);
        // we know which channel this came in on, this is who sent the message
        reader.connectionId = connectionId;

        // read header
        NetworkID targetId = NetworkID.GetDataFromBytes(reader);
        int functionId = reader.ReadUShort();
        // now try and call the function
        if (functionId < networkFunctions.Count)
        {
            networkFunctions[functionId].CallFunction(targetId, reader);
        }
        else
        {
            // uh... this id doesnt exist
            // TODO implement security here
            // a bad function id may indicate a dodgy connection... packet modification... or even someone trying to maliciously affect the game
            Debug.LogError("Unknown function id: " + functionId);
        }
    }

    private static NetWriter writer = null;
    private static byte[] writeData = null;

    public static NetWriter StartNetworkMessage(string functionName, NetworkID senderID)
    {
        if (writer.isWriting)
        {
            Debug.LogWarning("Tried to start writing another message when one is already being written. Please finish and send the first message before starting another one.");
            return null;
        }
        writer.Reset(writeData, NetworkCore.MAX_DATA_SIZE);
        writer.StartWriting();
        // write the sender id
        senderID.WriteBytes(writer);
        // write the destination function id
        ushort functionID = (ushort)FindIdForName(functionName);
        writer.WriteUShort(functionID);
        return writer;
    }

    public static void SendMessageToServer(NetWriter _writer, int channelType)
    {

        writer.StopWriting();
        if (NetworkCore.clientStatus != ConnectionStatus.Connected)
        {
            return;
        }
        int writtenSize = _writer.GetWrittenBytes();
        byte[] rawData = _writer.GetRawData();
        NetworkCore.singleton.SendRawToServer(channelType, rawData, writtenSize);
    }

    /// <summary>
    /// Sends the message to all clients, including our local one.
    /// Messages sent in this way can be buffered into a queue, so that they are executed on all new clients in order.
    /// </summary>
    /// <param name="_writer"></param>
    /// <param name="channelType"></param>
    /// <param name="addToBuffer"> add this message to the buffer</param>
    /// <returns>The Buffered Message if one was added.</returns>
    public static BufferedMessage SendMessageToAllClients(NetWriter _writer, int channelType, bool addToBuffer)
    {
        writer.StopWriting();
        if (!NetworkCore.isServer)
        {
            Debug.LogError("Tried to send message to all clients, but we are not a server!");
            return null;
        }
        int writtenSize = _writer.GetWrittenBytes();
        byte[] rawData = _writer.GetRawData();
        NetworkCore.singleton.SendRawToAllClients(channelType, rawData, writtenSize);
        if (addToBuffer)
        {
            BufferedMessage msg = new BufferedMessage(channelType, rawData, writtenSize, true);
            bufferedMessages.Add(msg);
            return msg;
        }
        return null;
    }

    /// <summary>
    /// Sends the message to all clients except our local one.
    /// /// Messages sent in this way can be buffered into a queue, so that they are executed on all new clients in order.
    /// </summary>
    /// <param name="_writer">Writer.</param>
    /// <param name="channelType">Channel type.</param>
    /// <param name="addToBuffer"> add this message to the buffer</param>
    /// <returns>The Buffered Message if one was added.</returns>
    public static BufferedMessage SendMessageToOtherClients(NetWriter _writer, int channelType, bool addToBuffer)
    {
        writer.StopWriting();
        if (!NetworkCore.isServer)
        {
            Debug.LogError("Tried to send message to other clients, but we are not a server!");
            return null;
        }
        int writtenSize = _writer.GetWrittenBytes();
        byte[] rawData = _writer.GetRawData();
        NetworkCore.singleton.SendRawToOtherClients(channelType, rawData, writtenSize);
        if (addToBuffer)
        {
            BufferedMessage msg = new BufferedMessage(channelType, (byte[])rawData.Clone(), writtenSize, false);
            bufferedMessages.Add(msg);
            return msg;
        }
        return null;
    }

    /// <summary>
    /// Sends the message to all clients that meet the filters criteria.
    /// Messages sent in this way cannot be buffered into a queue
    /// </summary>
    /// <param name="_writer"></param>
    /// <param name="channelType"></param>
    /// <param name="addToBuffer"> add this message to the buffer</param>
    /// <returns>The Buffered Message if one was added.</returns>
    public static void SendMessageToFilteredClients(NetWriter _writer, int channelType, NetworkFilter filter)
    {
        writer.StopWriting();
        if (!NetworkCore.isServer)
        {
            Debug.LogError("Tried to send message to filtered clients, but we are not a server!");
            return;
        }
        int writtenSize = _writer.GetWrittenBytes();
        byte[] rawData = _writer.GetRawData();
        NetworkCore.singleton.SendRawToFilteredClients(channelType, rawData, writtenSize, filter);

    }

    public static void SendMessageToClient(NetWriter _writer, int channelType, int clientId)
    {
        writer.StopWriting();
        int writtenSize = _writer.GetWrittenBytes();
        byte[] rawData = _writer.GetRawData();
        NetworkCore.singleton.SendRawToClient(clientId, channelType, rawData, writtenSize);
    }

    public static bool RemoveBufferedMessage(BufferedMessage message)
    {
        return bufferedMessages.Remove(message);
    }

    private static bool hasInit = false;

    void Init()
    {
        if (hasInit)
        {
            return;
        }
        writer = new NetWriter();
        reader = new NetReader();
        writeData = new byte[NetworkCore.MAX_DATA_SIZE];
        sceneIDs = new Pair<NetObjectID, bool>[256];
        gameIDs = new Pair<NetObjectID, bool>[256];
        for (int i = 0; i < 256; i++)
        {
            sceneIDs[i] = new Pair<NetObjectID, bool>();
            gameIDs[i] = new Pair<NetObjectID, bool>();
        }
        HasJoinedGame = false;
        // load up all the network functions
        RegisterAllFunctions();
        // load up the prefabs
        RegisterAllPrefabs();
        // link into the scene loading
        SceneManager.sceneLoaded += OnSceneLoaded;
        // notify all objects in this scene that the network scene has loaded
        //OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        hasInit = true;
    }

    private void RegisterAllPrefabs()
    {
        networkPrefabs = new List<NetworkPrefab>();
        for (int i = 0; i < editorPrefabs.Length; i++)
        {
            NetworkPrefab prefab = new NetworkPrefab(i, editorPrefabs[i]);
            networkPrefabs.Add(prefab);
        }
    }

    public static NetworkPrefab FindNetworkPrefab(string name)
    {
        foreach (NetworkPrefab n in networkPrefabs)
        {
            if (n.name == name)
            {
                return n;
            }
        }
        return null;
    }

    /// <summary>
    /// Called from network core, to update all listeners.
    /// </summary>
    static internal void UpdateBehaviours()
    {
        if (NetworkUpdateEvent == null)
        {
            return;
        }
        NetworkUpdateEvent();
    }

    #region Network ID management

    public Pair<NetObjectID, bool>[] sceneIDs;
    public Pair<NetObjectID, bool>[] gameIDs;

    // only include this function in the editor
#if UNITY_EDITOR
    public static void RecalculateSceneIDs()
    {
        // find all network objects in the scene
        NetObjectID[] netObjects = FindObjectsOfType<NetObjectID>();
        // reassign ids
        Debug.Log("recalculating IDs for " + netObjects.Length + " objects in scene");
        Debug.Assert(netObjects.Length < 255, "Over 255 scene network IDs, some will have duplicate values!");
        int idNum = 0;
        for (int i = 0; i < netObjects.Length; i++)
        {
            if (netObjects[i].isTransient)
            {
                Debug.Log(netObjects[i].gameObject.name + " is marked as transient, not setting scene ID");
                continue;
            }
            Undo.RecordObject(netObjects[i], "Update Network ID");
            netObjects[i].SetNetID(new NetworkID((byte)idNum, NetworkIDType.scene));

            idNum++;
        }
    }
#endif

    public override void OnNewNetworkSceneLoaded()
    {
        // we are the one who raises this function event.
        AddNetFunctionListener("cl_NetworkInstantiate", thisNetworkID, CL_NetworkInstantiate);
        AddNetFunctionListener("cl_NetworkDestroy", thisNetworkID, CL_NetworkDestroy);
        AddNetFunctionListener("cl_NotifyConnected", thisNetworkID, CL_NotifyConnected);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Loaded " + scene.name);
        // detect all scene network IDs
        if (mode == LoadSceneMode.Single)
        {
            LinkSceneIDs();
        }
        RemoveNetFunctionListener("cl_NetworkInstantiate", thisNetworkID);
        RemoveNetFunctionListener("cl_NetworkDestroy", thisNetworkID);
        RemoveNetFunctionListener("cl_NotifyConnected", thisNetworkID);
        // notify all behaviours of a new scene load
        // this should be used to register function listeners
        NetBehaviour[] netScripts = FindObjectsOfType<NetBehaviour>();
        for (int i = 0; i < netScripts.Length; i++)
        {
            netScripts[i].OnNewNetworkSceneLoaded();
        }
        //Debug.Log("Mapping after load");
        //PrintMappings();
    }

    internal void LinkSceneIDs()
    {
        // reset the array
        for (int i = 0; i < sceneIDs.Length; i++)
        {

            sceneIDs[i].left = null;
            sceneIDs[i].right = false;
        }

        // repopulate the array
        NetObjectID[] netObjects = FindObjectsOfType<NetObjectID>();
        for (int i = 0; i < netObjects.Length; i++)
        {
            if (netObjects[i].netID.type == NetworkIDType.scene)
            {
                if (netObjects[i].isTransient)
                {
                    // ah... we have a scene ID marked as transient
                    NetworkID newID = MakeTransientID(netObjects[i], true);
                    netObjects[i].SetNetID(newID);
                }
                else
                {
                    sceneIDs[i].left = netObjects[i];
                    sceneIDs[i].right = true;
                }
            }
        }
    }

    /// <summary>
    /// returns the next available ID, and records which object it's attached to
    /// </summary>
    /// <returns>next available id</returns>
    /// <param name="id">Identifier.</param>
    internal NetworkID GetNextID(NetObjectID id)
    {
        // find and assign appropriate object id
        if (Application.isPlaying)
        {
            for (int i = 0; i < gameIDs.Length; i++)
            {
                if (gameIDs[i].right == false)
                {
                    gameIDs[i].left = id;
                    gameIDs[i].right = true;
                    return new NetworkID((byte)i, NetworkIDType.game);

                }
            }
        }
        else
        {
            for (int i = 0; i < sceneIDs.Length; i++)
            {
                if (sceneIDs[i].right == false)
                {
                    sceneIDs[i].left = id;
                    sceneIDs[i].right = true;
                    return new NetworkID((byte)i, NetworkIDType.scene);
                }
            }
        }
        throw new System.InvalidOperationException("No more IDs available");
    }

    public NetworkID MakeTransientID(NetObjectID objectID, bool unlinkSceneIDs)
    {
        if (objectID.netID.type != NetworkIDType.scene)
        {
            Debug.LogError("Tried to make non-scene id " + objectID.netID + " transient.");
            return objectID.netID;
        }
        NetworkID oldID = objectID.netID;
        // allocate a new game id
        NetworkID newID = GetNextID(objectID);

        // unlink the scene ID if required
        if (unlinkSceneIDs)
        {
            sceneIDs[oldID.idNumber].left = null;
            sceneIDs[oldID.idNumber].right = false;
        }

        return newID;
    }

    #endregion

    /// <summary>
    /// Instantiates a prefab and assigns a network ID to it.
    /// This is replicated across all peers, and is buffered for any new players.
    /// To destroy these objects, you should use the "NetworkDestroy" function.
    /// </summary>
    /// <param name="pos">Position.</param>
    /// <param name="rot">Rot.</param>
    public GameObject NetworkInstantiate(NetworkPrefab prefab, Vector3 pos, Quaternion rot, bool sendToLocalClient)
    {
        if (NetworkCore.isServer)
        {
            if (prefab == null)
            {
                Debug.LogError("NetworkInstantiate called with no prefab. Object not created");
                return null;
            }
            GameObject newObj = Instantiate(prefab.serverPrefab, pos, rot);
            NetObjectID idComp = newObj.GetComponent<NetObjectID>();
            NetworkID newID = GetNextID(idComp);
            idComp.SetNetID(newID);
            NetWriter writer = StartNetworkMessage("cl_NetworkInstantiate", thisNetworkID);
            writer.WriteInt(prefab.id);
            writer.WriteVector3(pos);
            writer.WriteVector3(rot.eulerAngles);
            newID.WriteBytes(writer);
            BufferedMessage msg = null;
            if (sendToLocalClient)
            {
                msg = SendMessageToAllClients(writer, NetworkCore.AllCostMsg, true);

            }
            else
            {
                msg = SendMessageToOtherClients(writer, NetworkCore.AllCostMsg, true);
            }

            if (msg == null)
            {
                Debug.Log("Failed to successfully send Instantiation message. Unregistering Network ID: " + newID.ToString());
                FreeNetworkID(idComp);
                return newObj;
            }
            idComp.AddBufferedMessage(msg);
            newObj.BroadcastMessage("OnNewNetworkSceneLoaded");
            return newObj;
        }
        return null;
    }

    public void CL_NetworkInstantiate(NetReader reader)
    {
        int prefabID = reader.ReadInt();
        Vector3 pos = reader.ReadVector3();
        Vector3 euler = reader.ReadVector3();
        Quaternion rot = Quaternion.Euler(euler);
        GameObject newObj = (GameObject)Instantiate(networkPrefabs[prefabID].clientPrefab, pos, rot);
        NetObjectID idComp = newObj.GetComponent<NetObjectID>();
        NetworkID newID = NetworkID.GetDataFromBytes(reader);
        // assign the 
        RegisterObjectAndID(newID, idComp);
        
        newObj.BroadcastMessage("OnNewNetworkSceneLoaded");
    }

    private void RegisterObjectAndID(NetworkID newID, NetObjectID idComp)
    {
        int idNum = newID.idNumber;

        if(newID.type == NetworkIDType.game)
        {
            Debug.Assert(gameIDs[idNum].right == false && gameIDs[idNum].left == null, "Instantiated Game ID already exists! overwriting...");
            gameIDs[idNum].left = idComp;
            gameIDs[idNum].right = true;
            idComp.SetNetID(newID);
        }
        else
        {
            Debug.LogError("Instantiate with invalid ID provided!!!");
        }

    }

    /// <summary>
    /// Destroys a network object. To be used in conjunction with NetworkInstantiate
    /// </summary>
    /// <param name="obj"></param>
    public void NetworkDestroy(NetObjectID obj)
    {
        NetworkID id = obj.netID;
        if (id.type != NetworkIDType.game)
        {
            Debug.LogError("Tried to destroy an ID that was not a game ID");
            return;
        }

        // remove all buffered messages assigned to this object
        foreach (BufferedMessage msg in obj.bufferedMessages)
        {
            bufferedMessages.Remove(msg);
        }
        // create a destroy event for all clients
        NetWriter writer = StartNetworkMessage("cl_NetworkDestroy", thisNetworkID);
        id.WriteBytes(writer);
        // notify all clients to destroy this object
        SendMessageToOtherClients(writer, NetworkCore.AllCostMsg, false);
        // finally destroy this object
        Destroy(obj.gameObject);
        // unregister this ID
        gameIDs[id.idNumber].left = null;
        gameIDs[id.idNumber].right = false;
    }

    public void CL_NetworkDestroy(NetReader reader)
    {
        NetworkID idToDestroy = NetworkID.GetDataFromBytes(reader);
        if (idToDestroy.type == NetworkIDType.game)
        {
            NetObjectID objId = gameIDs[idToDestroy.idNumber].left;
            Destroy(objId.gameObject);
            gameIDs[idToDestroy.idNumber].left = null;
            gameIDs[idToDestroy.idNumber].right = false;
        }
        else
        {
            Debug.LogError("Tried to destroy a network ID that wasn't a game allocated one. " + idToDestroy);
        }
    }

    /// <summary>
    /// Frees the network object ID, and unlinks it from the system.
    /// THIS METHOD DOES NOT DESTROY THE OBJECT.
    /// </summary>
    /// <param name="newID"></param>
    /// <returns>true if the id was removed, false otherwise</returns>
    public bool FreeNetworkID(NetObjectID newID)
    {
        if (newID.netID.type != NetworkIDType.game)
        {
            Debug.LogError("Tried to free a network ID that wasn't a game allocated one. Aborting.");
            return false;
        }
        int num = newID.netID.idNumber;
        gameIDs[num].right = false;
        gameIDs[num].left = null;
        return true;
    }

    internal void BeginInitialNetworkUpdate(ConnectionData conn)
    {
        foreach (BufferedMessage msg in bufferedMessages)
        {
            if (!msg.sendToLocal && conn.connectionId == NetworkCore.clientConnectionId)
            {
                continue;
            }
            NetworkCore.singleton.SendRawToClient(conn.connectionId, msg.channelType, msg.rawData, msg.writtenSize);
        }
        StartCoroutine(SV_InitialNetworkUpdate(conn));
    }

    /// <summary>
    /// This is called AFTER all buffered messages have been sent (not necessarily received...)
    /// Called on the server when a client connects, to send initial Game Specific network data.
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    protected abstract IEnumerator SV_InitialNetworkUpdate(ConnectionData conn);

    /// <summary>
    /// Notifies any listeners of a new client successfully connected
    /// </summary>
    /// <param name="connectionId">Connection identifier.</param>
    internal static void NewClientConnected(int connectionId)
    {
        // notify the client that it has connected
        NetWriter writer = NetworkManager.StartNetworkMessage("cl_NotifyConnected", singleton.thisNetworkID);
        SendMessageToClient(writer, NetworkCore.AllCostMsg, connectionId);

        singleton.OnNewClientConnected(connectionId);
        if (ClientConnectEvent == null)
        {
            return;
        }
        ClientConnectEvent(connectionId);
        
    }

    internal abstract void OnNewClientConnected(int connectionId);

    internal void NotifyClientDisconnected(int connectionId)
    {
        OnClientDisconnected(connectionId);
        if (ClientDisconnectEvent == null)
        {
            return;
        }
        ClientDisconnectEvent(connectionId);
    }

    internal abstract void OnClientDisconnected(int connectionId);

    internal static void NotifyServerStarted()
    {
        Debug.Assert(ServerStartEvent.Target != null, "oh server crappers");
        ServerStartEvent();
    }

    internal static void NotifyClientStarted()
    {
        Debug.Assert(ClientStartEvent.Target != null, "oh client crappers");
        ClientStartEvent();
    }

    void _onServerStarted()
    {
        Debug.Log("Server Started.");
        NetBehaviour[] allScripts = FindObjectsOfType<NetBehaviour>();
        for (int i = 0; i < allScripts.Length; i++)
        {
            allScripts[i].ServerStart();
        }
    }

    void _onClientStarted()
    {
        Debug.Log("Client Started.");
        NetBehaviour[] allScripts = FindObjectsOfType<NetBehaviour>();
        for (int i = 0; i < allScripts.Length; i++)
        {
            allScripts[i].ClientStart();
        }
    }

    void CL_NotifyConnected(NetReader reader)
    {
        HasJoinedGame = true;
        OnMyClientJoinedGame();
    }

    public abstract void OnMyClientJoinedGame();

    public static NetObjectID GetObjectForID(NetworkID id)
    {
        if(singleton == null)
        {
            return null;
        }
        if(id.type == NetworkIDType.game)
        {
            return singleton.gameIDs[id.idNumber].left;
        }
        if(id.type == NetworkIDType.scene)
        {
            return singleton.sceneIDs[id.idNumber].left;
        }
        return null;
    }
}
