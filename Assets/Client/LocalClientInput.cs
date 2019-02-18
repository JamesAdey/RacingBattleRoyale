using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalClientInput : NetBehaviour
{

    List<LocalPlayer> localPlayers = new List<LocalPlayer>();

    private byte clientTick;

    public static bool HasClient { get; private set; }

    public override void ServerStart()
    {
        // do nothing
    }

    public override void ClientStart()
    {
        JoinGameUtility.localClientInput = this;
    }

    public override void OnNewNetworkSceneLoaded()
    {
        NetworkManager.NetworkUpdateEvent += NetworkUpdate;
        NetworkManager.AddNetFunctionListener("new_local_player", thisNetworkID, REC_NewLocalPlayer);
    }

    void OnDestroy()
    {
        NetworkManager.NetworkUpdateEvent -= NetworkUpdate;
        NetworkManager.RemoveNetFunctionListener("new_local_player", thisNetworkID);
    }

    void NetworkUpdate()
    {
        if (localPlayers.Count == 0)
        {
            return;
        }
        // collect inputs for our players, send them to the server
        for (int i = 0; i < localPlayers.Count; i++)
        {
            localPlayers[i].CollectInputs();
        }
        // increase our frame tick
        clientTick++;

        NetWriter writer = NetworkManager.StartNetworkMessage("recv_client_inputs", thisNetworkID);
        writer.WriteByte(clientTick);
        writer.WriteByte((byte)localPlayers.Count);
        for (int i = 0; i < localPlayers.Count; i++)
        {
            writer.WriteFloat(localPlayers[i].input.driveInput);
            writer.WriteFloat(localPlayers[i].input.steerInput);
            writer.WriteFloat(localPlayers[i].input.useInput);
        }
        NetworkManager.SendMessageToServer(writer, NetworkCore.UnreliableSequencedMsg);
    }

    public void AddLocalPlayer()
    {
        if(localPlayers.Count >= Globals.MAX_PLAYERS_PER_CLIENT)
        {
            Debug.LogWarning("Can't add another local player, maximum limit reached");
            return;
        }

        // request the server to add a local player
        NetWriter writer = NetworkManager.StartNetworkMessage("req_local_player", thisNetworkID);
        NetworkManager.SendMessageToServer(writer, NetworkCore.AllCostMsg);
    }

    public void REC_NewLocalPlayer(NetReader reader)
    {
        int playerNumber = reader.ReadInt();
        NetworkID vehicleID = NetworkID.GetDataFromBytes(reader);

        // create local player mapping
        LocalPlayer newPlayer = new LocalPlayer();
        newPlayer.playerNumber = playerNumber;
        newPlayer.vehicleID = vehicleID;
        Debug.Log(vehicleID);
        

        // TODO assign the vehicle
        NetObjectID id = NetworkManager.GetObjectForID(vehicleID);
        Debug.Log(id);
        BaseCar  car = id.GetComponent<BaseCar>();

        // assign our vehicle to follow
        Debug.Log("Got local player info from server!");
        localPlayers.Add(newPlayer);

        Camera.main.GetComponent<RaceCamera>().SetCameraTarget(car.thisTransform);

        HasClient = true;
    }
}
