using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BRGameController : NetBehaviour
{

    private static Race currentRace;

    BaseCar[] cars;

    [SerializeField]
    SpawnZone spawnZone;
    internal static bool gameOver;

    [SerializeField]
    private LayerMask explosionForceLayers;

    [SerializeField]
    private PlayerHUD hud;      // TODO refactor this

    private static RaceState cl_raceState = RaceState.OVER;

    public static event VoidEvent RaceRestartEvent;

    public override void ClientStart()
    {
        // do nothing
    }

    public override void OnNewNetworkSceneLoaded()
    {
        NetworkManager.AddNetFunctionListener("update_client_place_info", thisNetworkID, CL_UpdatePlaceInfo);
        NetworkManager.AddNetFunctionListener("update_race_info", thisNetworkID, CL_UpdateRaceInfo);
        NetworkManager.NetworkUpdateEvent += NetworkUpdate;
    }

    public void OnDestroy()
    {
        Debug.Log("destroyed");
        NetworkManager.RemoveNetFunctionListener("update_client_place_info", thisNetworkID);
        NetworkManager.RemoveNetFunctionListener("update_race_info", thisNetworkID);
        NetworkManager.NetworkUpdateEvent -= NetworkUpdate;
    }

    internal void ResetRace()
    {
        if (!NetworkCore.isServer)
        {
            return;
        }
        // setup a new race
        currentRace = new Race(this);
        currentRace.Init(0.4f, 5, 5, 20);

        for (int i = 0; i < cars.Length; i++)
        {
            // find spawn point
            SpawnArea area = spawnZone.GetSpawn(i);
            BaseCar car = cars[i];

            car.pDriver.isGhost = false;
            car.pGhostControl.SetGhostMode(false);

            car.Restart(area.position, area.rotation);
            currentRace.AddRacer(car);

        }

        // notify any listeners of this event
        if(RaceRestartEvent != null)
        {
            RaceRestartEvent();
        }
    }

    internal void SendCarVisuals(ConnectionData conn)
    {
        for (int i = 0; i < cars.Length; i++)
        {
            NetWriter writer = cars[i].PackVisuals();
            NetworkManager.SendMessageToClient(writer, NetworkCore.ReliableSequencedMsg, conn.connectionId);
        }
    }

    internal void BeginRace()
    {
        ResetRace();
        currentRace.StartRace();
    }

    public static RaceState CurrentRaceState()
    {
        if (NetworkCore.isServer)
        {
            if (currentRace == null)
            {
                return RaceState.OVER;
            }
            return currentRace.State;
        }
        else
        {
            return cl_raceState;
        }
    }


    internal void LobbyMode()
    {
        currentRace.ForceEnd();
    }

    internal static BaseCar GetCurrentLeader()
    {
        if (currentRace != null)
        {
            return currentRace.GetCurrentLeader();
        }

        return null;
    }


    private void Update()
    {
        if (currentRace != null)
        {
            currentRace.UpdateRace();

        }
    }

    void NetworkUpdate()
    {
        if (!NetworkCore.isServer)
        {
            return;
        }
        if (currentRace != null)
        {
            currentRace.UpdateClients();

            NetWriter writer = NetworkManager.StartNetworkMessage("update_race_info", thisNetworkID);
            writer.WriteInt((int)CurrentRaceState());
            NetworkManager.SendMessageToOtherClients(writer, NetworkCore.UnreliableSequencedMsg, false);
        }
    }

    void CL_UpdateRaceInfo(NetReader reader)
    {
        int stateInt = reader.ReadInt();
        cl_raceState = (RaceState)stateInt;
    }

    private void OnGUI()
    {
        if (currentRace != null)
        {
            currentRace.DrawGUI();
        }
    }

    public override void ServerStart()
    {
        // do nothing
    }

    public void SV_CreateCars(int count)
    {
        cars = new BaseCar[count];
        for (int i = 0; i < count; i++)
        {
            SV_CreateCar(i, PlayerType.BOT);
        }
    }

    internal void SendRaceInfo(GamePlayer player, int place, int placesLeft, float timeLeft)
    {
        NetWriter writer = NetworkManager.StartNetworkMessage("update_client_place_info", thisNetworkID);
        writer.WriteInt(player.playerNum);
        writer.WriteInt(place);
        writer.WriteInt(placesLeft);
        writer.WriteFloat(timeLeft);
        NetworkManager.SendMessageToClient(writer, NetworkCore.UnreliableSequencedMsg, player.clientConnection);
        //Debug.Log("sending race info");
    }

    void CL_UpdatePlaceInfo(NetReader reader)
    {
        int playerNum = reader.ReadInt();
        int place = reader.ReadInt();
        int placesLeft = reader.ReadInt();
        float timeLeft = reader.ReadFloat();

        hud.SetInfo(place, placesLeft, timeLeft);
    }

    void SV_CreateCar(int slot, PlayerType typ)
    {
        // find spawn point
        SpawnArea area = spawnZone.GetSpawn(slot);

        // create the player car
        NetworkPrefab prefab = NetworkManager.FindNetworkPrefab("player_car");
        GameObject racerObj = NetworkManager.singleton.NetworkInstantiate(prefab, area.position, area.rotation, false);
        BaseCar car = racerObj.GetComponent<BaseCar>();

        // get a player for this car
        GamePlayer player = PlayerInputManager.MakePlayerForCar(car);
        car.pDriver = player;

        // create an entry for this racer
        RaceEntry entry = new RaceEntry(car, slot, "Racer " + slot, typ);
        car.entry = entry;

        car.visuals.carNum = slot;

        // update the visuals
        car.ApplyVisuals();

        cars[slot] = car;

    }

    private Quaternion GetSpawnRot(int slot)
    {
        return Quaternion.identity;
    }

    private Vector3 GetSpawnPos(int slot)
    {
        return Vector3.zero;
    }

    public void RacerOut(BaseCar racer)
    {
        EffectsManager.CarKilled(racer);        // spawn an explosion
        racer.pDriver.isGhost = true;           // now make us into a ghost
        racer.pGhostControl.SetGhostMode(true);
        racer.BroadcastVisuals();
        Utils.ExplosionForce(racer.GetPosition(), 5, 30, explosionForceLayers);

        int pID = ItemManager.singleton.GetPrefabIDForName("mine");
        ItemManager.singleton.SpawnNewTempNetObject(pID, racer.GetPosition(), Quaternion.identity);
    }
}
