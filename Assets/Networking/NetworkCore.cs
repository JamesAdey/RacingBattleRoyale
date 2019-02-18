using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;
using System.Net.Sockets;

public class NetworkCore : NetBehaviour
{
    public int updateRate = 30;
    private static float nextUpdateTime;

    internal static NetworkCore singleton;
    private static bool hasInit = false;
    private static bool hasStarted = false;

    private static ConnectionConfig connConfig = null;
    private static HostTopology clientTopology = null;
    private static HostTopology serverTopology = null;

    public static int UnreliableMsg
    {
        get
        {
            return unreliableChannelId;
        }
    }

    public static int ReliableMsg
    {
        get
        {
            return reliableChannelId;
        }
    }

    public static int UnreliableSequencedMsg
    {
        get
        {
            return unreliableSequencedChannelId;
        }

    }

    public static int ReliableSequencedMsg
    {
        get
        {
            return reliableSequencedChannelId;
        }

    }

    public static int AllCostMsg
    {
        get
        {
            return allCostChannelId;
        }

    }

    public static float networkUpdateDelay
    {
        get
        {
            if (singleton == null)
            {
                return 1f;
            }
            return (1f / singleton.updateRate);
        }
    }

    private static int unreliableChannelId;
    private static int reliableChannelId;
    private static int unreliableSequencedChannelId;
    private static int reliableSequencedChannelId;
    private static int allCostChannelId;

    private static int serverHostId;
    private static int clientHostId;
    internal static int clientConnectionId;

    private static byte err = 0;

    public static bool isClient
    {
        get
        {
            return _isClient;
        }
        private set
        {
            _isClient = value;
        }
    }

    private static bool _isClient;

    public static bool isServer
    {
        get
        {
            return _isServer;
        }
        private set
        {
            _isServer = value;
        }
    }

    private static bool _isServer;

    public static bool isConnected
    {
        get
        {
            if (NetworkCore._isClient)
            {
                return clientStatus == ConnectionStatus.Connected;
            }
            if (NetworkCore.isServer)
            {
                return hasStarted;
            }
            return false;
        }
    }

    private static byte[] recBuffer;
    public const int MAX_DATA_SIZE = 1024;

    private static int serverConnections;
    private static ConnectionData[] connections;

    private static Queue<int> disconnections;

    public static ConnectionStatus clientStatus
    {
        get
        {
            return _clientStatus;
        }
        private set
        {
            _clientStatus = value;
        }
    }

    public static string serverAddress
    {
        get
        {
            return _serverAddress;
        }
        private set
        {
            _serverAddress = value;
        }
    }

    private static string _serverAddress = "NONE";
    private static string attemptedAddress;

    private static ConnectionStatus _clientStatus;
    [SerializeField]
    private int debugConnNumber = 0;
    [SerializeField]
    private int debugHostID = 0;

    public override void OnNewNetworkSceneLoaded()
    {
        // do nothing
    }

    public override void ServerStart()
    {
        // do nothing
    }

    public override void ClientStart()
    {
        // do nothing
    }

    public static void Reset()
    {
        hasInit = false;
        hasStarted = false;
        unreliableChannelId = -1;
        reliableChannelId = -1;
        unreliableSequencedChannelId = -1;
        serverHostId = -1;
        clientHostId = -1;
        clientConnectionId = -1;
        connConfig = null;
        clientTopology = null;
        serverTopology = null;
        err = 0;
        isClient = false;
        isServer = false;
        recBuffer = null;
        connections = null;
        serverConnections = -1;
        clientStatus = ConnectionStatus.Disconnected;
        nextUpdateTime = 0;
        serverAddress = "NONE";
        attemptedAddress = "NONE";

        if (disconnections != null)
        {
            disconnections.Clear();
        }
        disconnections = null;

        // shutdown the network if it's still going
        if (NetworkTransport.IsStarted)
        {
            NetworkTransport.Shutdown();
        }
    }

    private static void Init()
    {
        // initialise all our variables, and reset the network
        Reset();
        GlobalConfig globalConfig = new GlobalConfig();
        // init network with default settings
        NetworkTransport.Init(globalConfig);
        // setup channels
        connConfig = new ConnectionConfig();
        unreliableChannelId = connConfig.AddChannel(QosType.Unreliable);
        unreliableSequencedChannelId = connConfig.AddChannel(QosType.UnreliableSequenced);
        reliableChannelId = connConfig.AddChannel(QosType.Reliable);
        reliableSequencedChannelId = connConfig.AddChannel(QosType.ReliableSequenced);
        allCostChannelId = connConfig.AddChannel(QosType.AllCostDelivery);
        hasInit = true;
        recBuffer = new byte[MAX_DATA_SIZE];
        disconnections = new Queue<int>();
    }

    public void OnApplicationQuit()
    {
        if (hasInit)
        {
            if (isServer)
            {
                CloseServer();
            }
            else if (isClient)
            {
                Disconnect();
            }
        }
    }

    internal static bool Shutdown()
    {
        if (!hasInit)
        {
            Debug.LogWarning("Tried to shutdown network without starting it");
            return false;
        }
        // set hasInit to false to prevent more network updates
        hasInit = false;
        // close the transport layer
        NetworkTransport.Shutdown();
        // wipe our variables
        Reset();
        return true;
    }

    public static void StartClient()
    {
        if (!hasInit)
        {
            Init();
        }
        if (hasStarted)
        {
            Debug.LogWarning("Tried to start client, but network already started!");
            return;
        }
        clientTopology = new HostTopology(connConfig, 1);
        clientHostId = NetworkTransport.AddHost(clientTopology);
        Debug.Log("client id:" + clientHostId);
        isClient = true;
        hasStarted = true;
        Debug.Log("Client started.");
        NetworkManager.NotifyClientStarted();
    }

    public static void StartServer(bool hasLocalClient, int maxClients)
    {
        if (!hasInit)
        {
            Init();
        }
        if (hasStarted)
        {
            Debug.LogWarning("Tried to start server, but network already started!");
            return;
        }
        // default to 4, let user change this in the future
        serverConnections = maxClients;
        serverTopology = new HostTopology(connConfig, serverConnections);
        // listen on port 27015
        serverHostId = NetworkTransport.AddHost(serverTopology, 27015);
        Debug.Log("server id:" + serverHostId);
        connections = new ConnectionData[serverConnections];
        Debug.Log("Server started.");
        serverAddress = FindLocalIP();
        // start the client
        if (hasLocalClient)
        {
            StartClient();
        }
        isServer = true;
        hasStarted = true;
        NetworkManager.NotifyServerStarted();
    }

    public static void Connect(string address, int port)
    {
        if (!hasStarted)
        {
            Debug.LogWarning("Connection attempt without starting network!");
            return;
        }
        clientStatus = ConnectionStatus.Connecting;
        clientConnectionId = NetworkTransport.Connect(clientHostId, address, port, 0, out err);
        attemptedAddress = address;
        CheckNetworkError();
    }

    public static bool CloseServer()
    {
        if (!isServer)
        {
            Debug.LogWarning("Tried to close a server, but no server started");
            return false;
        }

        if (isClient)
        {
            NetworkTransport.RemoveHost(clientHostId);
        }
        NetworkTransport.RemoveHost(serverHostId);
        return Shutdown();
    }

    public static void Disconnect()
    {
        if (!hasStarted)
        {
            Debug.LogWarning("Tried to disconnect without starting network!");
            return;
        }

        if (isServer)
        {
            Debug.LogWarning("Can't disconnect a server. use CloseServer() instead.");
        }
        bool success = NetworkTransport.Disconnect(clientHostId, clientConnectionId, out err);
        CheckNetworkError();
        if (success)
        {
            Debug.Log("Disconnected successfully.");
        }
        else
        {
            Debug.Log("Disconnect failed! see above error msg");
        }
        if (isClient)
        {
            NetworkTransport.RemoveHost(clientHostId);
        }
        Shutdown();
    }

    private static bool CheckNetworkError()
    {
        NetworkError e = (NetworkError)err;
        if (e != NetworkError.Ok)
        {
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }

    public void UpdateNetwork()
    {
        int r_hostId;
        int r_connectionId;
        int r_channelId;
        int receivedSize;

        NetworkEventType eventType;
        eventType = NetworkTransport.Receive(out r_hostId, out r_connectionId, out r_channelId, recBuffer, MAX_DATA_SIZE, out receivedSize, out err);

        CheckNetworkError();
        while (eventType != NetworkEventType.Nothing)
        {
            switch (eventType)
            {
                case NetworkEventType.DataEvent:
                    ProcessDataEvent(recBuffer, receivedSize, r_connectionId);
                    break;
                case NetworkEventType.ConnectEvent:
                    if (r_hostId == serverHostId)
                    {
                        sv_ProcessConnectEvent(r_connectionId);
                    }
                    else if (r_hostId == clientHostId)
                    {
                        cl_ProcessConnectEvent(r_connectionId);
                    }
                    else
                    {
                        Debug.LogError("Connection event on unknown host ID! " + r_hostId);
                    }
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("hst:" + r_hostId + "\ncon:" + r_connectionId);
                    ProcessDisconnectEvent(r_connectionId);
                    break;
                case NetworkEventType.BroadcastEvent:
                    ProcessBroadcastEvent();
                    break;
            }
            eventType = NetworkTransport.Receive(out r_hostId, out r_connectionId, out r_channelId, recBuffer, MAX_DATA_SIZE, out receivedSize, out err);
        }
    }

    private void ProcessDataEvent(byte[] buffer, int length, int connectionId)
    {
        NetworkManager.singleton.ProcessData(buffer, length, connectionId);
    }

    private void sv_ProcessConnectEvent(int receivedId)
    {
        connections[receivedId] = new ConnectionData(receivedId);
        if (clientConnectionId == receivedId)
        {
            Debug.Log("local client connected. id: " + receivedId);
            clientStatus = ConnectionStatus.Connected;
        }
        else
        {
            Debug.Log("remote client connected, sending initial update for :" + receivedId);
        }
        NetworkManager.singleton.BeginInitialNetworkUpdate(connections[receivedId]);
    }

    private void cl_ProcessConnectEvent(int receivedId)
    {
        if (clientConnectionId == receivedId)
        {
            Debug.Log("Connected to server. id: " + receivedId);
            clientStatus = ConnectionStatus.Connected;
            // store our attempted address
            if (!isServer)
            {
                serverAddress = attemptedAddress;
                attemptedAddress = "NONE";
            }
        }
        else
        {
            Debug.Log("oh crap");
        }
    }

    private void ProcessDisconnectEvent(int receivedId)
    {
        // TODO uncomment this
        //CheckNetworkError();
        if (clientConnectionId == receivedId)
        {
            Debug.Log("Disconnected from server. id: " + receivedId);
            clientStatus = ConnectionStatus.Disconnected;
            Disconnect();
        }
        else
        {
            connections[receivedId].active = false;
            Debug.Log("Queueing disconnect for client: " + receivedId);
            disconnections.Enqueue(receivedId);
        }

    }

    private void ProcessDisconnects()
    {
        while(disconnections.Count > 0)
        {
            int connectionToRemove = disconnections.Dequeue();
            Debug.Log("Disconnecting connection:" + connectionToRemove);
            connections[connectionToRemove] = null;
            NetworkManager.singleton.NotifyClientDisconnected(connectionToRemove);
        }
    }

    private void ProcessBroadcastEvent()
    {
        // might receive a broadcast, but this game doesn't use them
    }

    internal void SendRawToServer(int channelType, byte[] buffer, int bufferSize)
    {
        if (!isClient)
        {
            Debug.LogWarning("Tried to send to server, but no client socket exists!");
            return;
        }
        NetworkTransport.Send(clientHostId, clientConnectionId, channelType, buffer, bufferSize, out err);
        CheckNetworkError();
    }

    internal void SendRawToClient(int targetConnectionId, int channelType, byte[] buffer, int bufferSize)
    {
        if (!isServer)
        {
            Debug.LogWarning("Tried to send to a client, but no server socket exists!");
            return;
        }
        NetworkTransport.Send(serverHostId, targetConnectionId, channelType, buffer, bufferSize, out err);
        CheckNetworkError();
    }

    internal void SendRawToAllClients(int channelType, byte[] buffer, int bufferSize)
    {
        if (!isServer)
        {
            Debug.LogWarning("Tried to send to a client, but no server socket exists!");
            return;
        }
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i] == null)
            {
                continue;       // skip null connections
            }
            if (!connections[i].active)
            {
                continue;       // skip inactive connections
            }
            NetworkTransport.Send(serverHostId, connections[i].connectionId, channelType, buffer, bufferSize, out err);
        }
        CheckNetworkError();
    }

    internal void SendRawToOtherClients(int channelType, byte[] buffer, int bufferSize)
    {
        if (!isServer)
        {
            Debug.LogWarning("Tried to send to a client, but no server socket exists!");
            return;
        }
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i] == null)
            {
                continue;       // skip null clients
            }
            if (connections[i].connectionId == clientConnectionId)
            {
                continue;       // skip local client
            }
            if (!connections[i].active)
            {
                continue;       // skip inactive connections
            }
            NetworkTransport.Send(serverHostId, connections[i].connectionId, channelType, buffer, bufferSize, out err);
        }
        CheckNetworkError();
    }

    internal void SendRawToFilteredClients(int channelType, byte[] buffer, int bufferSize, NetworkFilter filter)
    {
        if (!isServer)
        {
            Debug.LogWarning("Tried to send to a client, but no server socket exists!");
            return;
        }
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i] == null)
            {
                continue;       // skip null clients
            }
            if (!connections[i].active)
            {
                continue;       // skip inactive connections
            }
            if (!filter.CheckConnection(connections[i].connectionId))
            {
                continue;       // filter out bad connections
            }

            NetworkTransport.Send(serverHostId, connections[i].connectionId, channelType, buffer, bufferSize, out err);
        }
        CheckNetworkError();
    }


    void Awake()
    {
        singleton = this;
    }

    void Update()
    {
        if (!hasInit)
        {
            return;
        }
        if (Time.time > nextUpdateTime)
        {
            // do all receiving
            UpdateNetwork();
            // now update everyone who is listening
            NetworkManager.UpdateBehaviours();
            // process any disconnects
            if (isServer)
            {
                ProcessDisconnects();
            }
            // force send all messages NOW!
            FlushNetwork();
            
            
            nextUpdateTime = Time.time + (1 / updateRate);
        }
    }

    void OnGUI()
    {
        if (connections == null)
        {
            return;
        }

        int i = debugConnNumber;
        if (connections[i] == null)
        {
            return;
        }
        if (connections[i].active)
        {
            GUIForConnection(debugHostID, connections[i].connectionId);
            return;
        }

    }

    static Rect IncRect(Rect r, int h)
    {
        return new Rect(r.x, r.y + h, r.width, r.height);
    }

    void GUIForConnection(int hostID, int connectionID)
    {
        int HEIGHT = 20;

        Rect rect = new Rect(10, 20, 200, HEIGHT); ;
        byte err;

        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "HOST:" + hostID + "     CONN:" + connectionID);

        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "Ack Buffer:" + NetworkTransport.GetAckBufferCount(hostID, connectionID, out err));

        rect = IncRect(rect, HEIGHT);
        
        GUI.Label(rect, "Incoming Packets:" + NetworkTransport.GetIncomingPacketCount(hostID, connectionID, out err));

        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "Incoming Packets Lost:" + NetworkTransport.GetIncomingPacketLossCount(hostID, connectionID, out err));

        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "Max Bandwidth:" + NetworkTransport.GetMaxAllowedBandwidth(hostID, connectionID, out err));


        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "OUTGOING");

        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "Full Bytes:" + NetworkTransport.GetOutgoingFullBytesCountForConnection(hostID, connectionID, out err));

        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "User Bytes:" + NetworkTransport.GetOutgoingUserBytesCountForConnection(hostID, connectionID, out err));

        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "Network PKT Loss %:" + NetworkTransport.GetOutgoingPacketNetworkLossPercent(hostID, connectionID, out err));

        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "Overflow PKT Loss %:" + NetworkTransport.GetOutgoingPacketOverflowLossPercent(hostID, connectionID, out err));

        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "Message Count:" + NetworkTransport.GetOutgoingMessageCountForConnection(hostID, connectionID, out err));

        rect = IncRect(rect, HEIGHT);
        GUI.Label(rect, "Packet Count:" + NetworkTransport.GetOutgoingPacketCountForConnection(hostID, connectionID, out err));

    }

    private void FlushNetwork()
    {
        if (isServer)
        {
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i] == null)
                {
                    continue;
                }
                //Debug.Log("sending queue:"+i);
                //Debug.Log(connections[i].active);
                //Debug.Log(connections[i].connectionId);
                NetworkTransport.SendQueuedMessages(serverHostId, connections[i].connectionId, out err);
                CheckNetworkError();
            }
        }

        if (isClient && clientStatus == ConnectionStatus.Connected)
        {
            NetworkTransport.SendQueuedMessages(clientHostId, clientConnectionId, out err);
            CheckNetworkError();
        }
    }

    static internal void DoneInitialNetworkUpdate(int connectionID)
    {
        Debug.Log("Done initial network update for client " + connectionID);
        connections[connectionID].active = true;
        NetworkManager.NewClientConnected(connectionID);
    }

    static string FindLocalIP()
    {
        // won't always work if a VM is running
        // hopefully the user should know how to get their ip address though if this happens
        string localIP = "NONE";
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress[] addresses = host.AddressList;
        for (int i = 0; i < addresses.Length; i++)
        {
            if (addresses[i].AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = addresses[i].ToString();
                break;
            }
        }
        return localIP;
    }

}