using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNetworkManager : NetworkManager {
    public override void ClientStart()
    {
        // do nothing
    }

    public override void OnMyClientJoinedGame()
    {
        // do nothing
    }

    public override void ServerStart()
    {
        // do nothing
    }

    protected override IEnumerator SV_InitialNetworkUpdate(ConnectionData conn)
    {
        PrintMappings();
        yield return new WaitForEndOfFrame();
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
