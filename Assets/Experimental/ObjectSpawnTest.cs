using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnTest : MonoBehaviour {

    public void SpawnRandomMine()
    {
        if (!NetworkCore.isServer)
        {
            return;
        }
        Vector3 spawnPos = Random.insideUnitSphere;
        spawnPos.y = 0;
        spawnPos *= 20;
        spawnPos += transform.position;

        int id = ItemManager.singleton.GetPrefabIDForName("mine");
        ItemManager.singleton.SpawnNewTempNetObject(id, spawnPos, Quaternion.identity);
    }
}
