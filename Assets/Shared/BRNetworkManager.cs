using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BRNetworkManager : NetworkManager
{
    public static int numRacers = 10;

    [SerializeField]
    private BRGameController gameController;

    public override void ClientStart()
    {
        // do nothing
    }

    public override void OnMyClientJoinedGame()
    {

    }

    public override void ServerStart()
    {
        
        NetworkPrefab prefab = NetworkManager.FindNetworkPrefab("client_inputs");
        NetworkManager.singleton.NetworkInstantiate(prefab, Vector3.zero, Quaternion.identity, false);
        Debug.Log("Creating cars...");
        gameController.SV_CreateCars(numRacers);
    }

    protected override IEnumerator SV_InitialNetworkUpdate(ConnectionData conn)
    {
        // print the mappings
        PrintMappings();
        // wait a bit
        yield return new WaitForEndOfFrame();

        gameController.SendCarVisuals(conn);
        // done
        NetworkCore.DoneInitialNetworkUpdate(conn.connectionId);
    }

    internal override void OnClientDisconnected(int connectionId)
    {
        // do nothing
    }

    internal override void OnNewClientConnected(int connectionId)
    {
        // do nothing
    }
}
