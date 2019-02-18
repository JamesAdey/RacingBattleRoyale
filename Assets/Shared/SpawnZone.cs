using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpawnArea
{
    public Vector3 position;
    public Quaternion rotation;

    public SpawnArea(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

    public static SpawnArea Origin = new SpawnArea(Vector3.zero, Quaternion.identity);
}

public class SpawnZone : MonoBehaviour {

    public GameObject racerPrefab;
    public Vector3 spawnBoxSize = new Vector3(5, 5, 10);
    public int racersPerRow = 5;

    public float spacing = 2;

    public float racers = 100;

    List<SpawnArea> spawns = new List<SpawnArea>();
    private Transform thisTransform;
    

    // Use this for initialization
    void Start () {
        thisTransform = this.transform;
        CalculateSpawns();
    }
	
	// Update is called once per frame
    void SpawnAllRacers()
    {
        
        for(int i = 0; i < racers; i++)
        {
            GameObject racerObj = (GameObject)Instantiate(racerPrefab, spawns[i].position, spawns[i].rotation);
            BaseCar car = racerObj.GetComponent<BaseCar>();
            car.SetVehicleNumber(i);
            racerObj.name = "Racer " + i;
        }
    }

    public SpawnArea GetSpawn(int num)
    {
        if(num < 0 || num >= spawns.Count)
        {
            return SpawnArea.Origin;
        }
        return spawns[num];
    }

    private void OnDrawGizmos()
    {
        thisTransform = this.transform;
        CalculateSpawns();
        for (int i = 0; i < spawns.Count; i++)
        {
            Gizmos.DrawWireSphere(spawns[i].position, 1f);
            Gizmos.DrawLine(spawns[i].position, spawns[i].position + spawns[i].rotation* Vector3.forward * spawnBoxSize.z * 0.5f);
        }
    }

    public void CalculateSpawns()
    {
        spawns.Clear();
        if (racersPerRow < 1)
        {
            racersPerRow = 1;
        }
        // work out row width
        float rowWidth = racersPerRow * spawnBoxSize.x + (racersPerRow + 1) * spacing;
        float halfWidth = rowWidth / 2;
        
        float rows = Mathf.Ceil(racers / racersPerRow);

        float columnLength = rows * spawnBoxSize.z + (rows + 1) * spacing;
        
        Vector3 spawnPos = new Vector3(-rowWidth / 2, 0, columnLength/2);

        

        for(int i = 0; i < rows; i++)
        {
            spawnPos.x = -rowWidth / 2;
            for(int j = 0; j < racersPerRow;j++)
            {
                spawnPos.x += spawnBoxSize.x;
                Vector3 worldPos = thisTransform.position + thisTransform.rotation*spawnPos;
                spawns.Add(new SpawnArea(worldPos, thisTransform.rotation));
                spawnPos.x += spacing;
            }
            spawnPos.z -= (spawnBoxSize.z + spacing);
        }

    }


}
