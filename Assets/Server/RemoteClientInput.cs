using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteClientInput : NetBehaviour
{
    // a mapping of connection IDs to input records
    private Dictionary<int, ClientInputMapping> clientInputs = new Dictionary<int, ClientInputMapping>();

    public override void ServerStart()
    {
        // do nothing
    }

    public override void ClientStart()
    {
        // do nothing
    }

    public override void OnNewNetworkSceneLoaded()
    {
        NetworkManager.AddNetFunctionListener("recv_client_inputs", thisNetworkID, RecvClientInputs);
        NetworkManager.AddNetFunctionListener("req_local_player", thisNetworkID, REQ_LocalPlayer);
        NetworkManager.ClientDisconnectEvent += OnClientDisconnected;
    }

    public void OnDestroy()
    {
        NetworkManager.RemoveNetFunctionListener("recv_client_inputs", thisNetworkID);
        NetworkManager.RemoveNetFunctionListener("req_local_player", thisNetworkID);
        NetworkManager.ClientDisconnectEvent -= OnClientDisconnected;
    }

    void RecvClientInputs(NetReader reader)
    {
        // find the correct client for this input
        ClientInputMapping record = clientInputs[reader.connectionId];
        if(record == null)
        {
            Debug.LogWarning("no client input record exists for client " + reader.connectionId);
            return;
        }

        

        // read the data
        byte clientTick = reader.ReadByte();
        byte numPlayers = reader.ReadByte();

        for (int i = 0; i < numPlayers; i++)
        {
            PlayerInput input = new PlayerInput();
            input.tick = clientTick;
            input.driveInput = reader.ReadFloat();
            input.steerInput = reader.ReadFloat();
            input.useInput = reader.ReadFloat();
            ClientInputMapping mapping = clientInputs[reader.connectionId];
            mapping.pairs[i].pPlayer.AddInput(input);
        }
    }

    private void REQ_LocalPlayer(NetReader reader)
    {
        if (!clientInputs.ContainsKey(reader.connectionId))
        {
            clientInputs.Add(reader.connectionId, new ClientInputMapping());
        }

        ClientInputMapping clientMapping = clientInputs[reader.connectionId];

        // check bounds
        if(clientMapping.pairs.Count >= Globals.MAX_PLAYERS_PER_CLIENT)
        {
            Debug.LogWarning("can't add any more players from this client: " + reader.connectionId);
            return;
        }

        // find the next available car
        GamePlayer newPlayer = PlayerInputManager.FindNewPlayer();

        if(newPlayer == null)
        {
            Debug.LogWarning("Tried to get a new player for client " + reader.connectionId + " but all players are full!");
            return;
        }

        // convert this car to be a player
        newPlayer.clientConnection = reader.connectionId;
        newPlayer.playerType = PlayerType.REMOTE;
        newPlayer.AddInput(PlayerInput.None);

        // add a pairing between this client and player
        int clientNum = clientMapping.pairs.Count;
        ClientPlayerPair pair = new ClientPlayerPair(clientNum, newPlayer);    // links a client (player) number to an actual player class
        clientMapping.pairs.Add(pair);

        // notify the player as to which car it is controlling.
        NetWriter writer = NetworkManager.StartNetworkMessage("new_local_player", thisNetworkID);
        writer.WriteInt(clientNum);             // notify the client 
        newPlayer.car.thisNetworkID.WriteBytes(writer);     // write the ID of the car it's controlling
        NetworkManager.SendMessageToClient(writer, NetworkCore.AllCostMsg, reader.connectionId);
    }

    void REQ_DropPlayer(NetReader reader)
    {
        if (!clientInputs.ContainsKey(reader.connectionId))
        {
            Debug.LogWarning("Can't drop player, as no mapping exists for this client");
            return;
        }

        ClientInputMapping clientMapping = clientInputs[reader.connectionId];

        int playerNumToRemove = reader.ReadInt();

        DropPlayer(clientMapping, playerNumToRemove);
    }

    private bool DropPlayer(ClientInputMapping mapping, int playerNumToRemove)
    {
        // find the car to remove
        GamePlayer player = mapping.pairs[playerNumToRemove].pPlayer;

        if (player == null)
        {
            Debug.LogError("No player set for client mapping. MAJOR ISSUE.");
            return false;
        }

        // unlink the player from this game player
        player.clientConnection = -1;
        player.playerType = PlayerType.BOT;
        player.AddInput(PlayerInput.None);

        return true;
    }

    public void OnClientDisconnected(int connectionId)
    {
        ClientInputMapping clientMapping = clientInputs[connectionId];

        foreach(var pair in clientMapping.pairs)
        {
            pair.pPlayer.clientConnection = -1;
            pair.pPlayer.playerType = PlayerType.BOT;
            pair.pPlayer.AddInput(PlayerInput.None);
        }
        clientMapping.pairs.Clear();
        clientInputs.Remove(connectionId);
    }
}