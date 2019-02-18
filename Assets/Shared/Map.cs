using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
	private static Map singleton;
	public Transform cameraSpawn;
	public Transform[] carSpawns;

	public static int maxPlayers {
		get {
			if (singleton != null) {
				return singleton.carSpawns.Length;
			} else {
				return 0;
			}
		}
	}

	public static Transform initialCameraSpawn {
		get {
			if (!singleton) {
				return null;
			}
			return singleton.cameraSpawn;
		}
	}

	public static Transform GetSpawnPoint (int spawnNum)
	{
		if (spawnNum < 0 || spawnNum > singleton.carSpawns.Length) {
			spawnNum = 0;
		}
		return singleton.carSpawns [spawnNum];
	}

	void Awake ()
	{
		singleton = this;
	}

	void OnDestroy ()
	{
		singleton = null;
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
