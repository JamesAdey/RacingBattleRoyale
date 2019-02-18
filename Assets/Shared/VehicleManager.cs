using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleData
{
	// what vehicle instance?
	public BaseCar car;
	// who owns this?
	public bool isLocal = false;
	public int localControllerNum = -1;
	// TODO
	// what vehicle type?
	public int vehicleID = -1;




}

public class VehicleManager : MonoBehaviour
{

	public static VehicleData[] vehicleData;

	public void CreateCars ()
	{
		for (int i = 0; i < vehicleData.Length; i++) {
			CarDataStore.SpawnCar (0, i, Map.GetSpawnPoint (i));

		}
	}

	// Use this for initialization
	void Start ()
	{
		StartCoroutine (ExampleSetup ());
	}

	IEnumerator ExampleSetup ()
	{
		yield return new WaitForSeconds (1);
		int numHumans = PlayerInputs.GetActiveControllers ();
		int[] ctrlNums = PlayerInputs.GetActiveControllerNumbers ();
		int totalPlayers = Map.maxPlayers;
		vehicleData = new VehicleData[totalPlayers];
		int currentControllerNum = 0;
		for (int i = 0; i < vehicleData.Length; i++) {
			vehicleData [i] = new VehicleData ();
			vehicleData [i].vehicleID = 0;
			if (currentControllerNum < numHumans) {
				vehicleData [i].isLocal = true;
				vehicleData [i].localControllerNum = ctrlNums [currentControllerNum];
				currentControllerNum++;
			}
		}
		CreateCars ();

	}

	// Update is called once per frame
	void Update ()
	{
		
	}
}
