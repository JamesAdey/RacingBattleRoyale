using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class EffectsManager : NetBehaviour
{
    public GameObject explosion;
    private static EffectsManager singleton;

    private void Awake()
    {
        singleton = this;
    }

    internal static void CarKilled(BaseCar baseCar)
    {
        NetWriter writer = NetworkManager.StartNetworkMessage("fx_spawn", singleton.thisNetworkID);
        writer.WriteVector3(baseCar.transform.position);
        NetworkManager.SendMessageToAllClients(writer, NetworkCore.UnreliableMsg, false);
    }

    internal static void SpawnExplosion(Vector3 pos)
    {
        NetWriter writer = NetworkManager.StartNetworkMessage("fx_spawn", singleton.thisNetworkID);
        writer.WriteVector3(pos);
        NetworkManager.SendMessageToAllClients(writer, NetworkCore.UnreliableMsg, false);
    }

    public override void OnNewNetworkSceneLoaded()
    {
        NetworkManager.AddNetFunctionListener("fx_spawn", thisNetworkID, CL_SpawnEffect);
    }

    public void OnDestroy()
    {
        NetworkManager.RemoveNetFunctionListener("fx_spawn", thisNetworkID);
    }

    void CL_SpawnEffect(NetReader reader)
    {
        Vector3 pos = reader.ReadVector3();
        Instantiate(singleton.explosion, pos, Quaternion.identity);
    }

    public override void ServerStart()
    {
        
    }

    public override void ClientStart()
    {
        
    }
}