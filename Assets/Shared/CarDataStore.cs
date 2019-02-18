using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDataStore : MonoBehaviour
{
	private static CarDataStore singleton;
	public GameObject[] cars = new GameObject[0];

	void Awake ()
	{
		singleton = this;
	}

	void OnDestroy ()
	{
		singleton = null;
	}

	public static int maxCars {
		get {
			if (!singleton) {
				return -1;
			}
			return singleton.cars.Length;
		}
	}

	public static void SpawnCar (int carId, int vehicleId, Transform spawn)
	{
		singleton.CreateCar (carId, vehicleId, spawn.position, spawn.rotation);
	}

	void CreateCar (int carID, int vehicleID, Vector3 spawnPos, Quaternion spawnRot)
	{
		GameObject newObj = Instantiate (cars [carID], spawnPos, spawnRot);
		BaseCar carScript = newObj.GetComponent<BaseCar> ();
		carScript.SetVehicleNumber (vehicleID);
		carScript.InitVehicle ();
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
