using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LocalPlayerData
{
	public BaseCar targetCar;
	// inputs
	public LocalPlayerInput inputs;
	// camera
	public MultiPlayerCamera camera;
	public Rect cameraRect;
}

public class LocalPlayerManager : MonoBehaviour
{
	private static LocalPlayerManager singleton;
	public LocalPlayerData[] localPlayerData;


	public GameObject cameraPrefab;

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
		InitLocalData ();
	}

	/// <summary>
	/// Gets the data for specified player index.
	/// </summary>
	/// <returns>The data for player.</returns>
	/// <param name="id">0 based local player index</param>
	LocalPlayerData GetDataForPlayer (int id)
	{
		return localPlayerData [id];
	}

	void InitLocalData ()
	{
		localPlayerData = new LocalPlayerData[PlayerInputs.GetLocalPlayerCount ()];
		LocalPlayerInput[] localInputs = PlayerInputs.GetPlayerInputMapping ();

		Debug.Log ("local inputs " + localInputs.Length);
		Debug.Log ("player data " + localPlayerData.Length);

		int activeControllers = PlayerInputs.GetActiveControllers ();
		Debug.Log ("active controllers " + activeControllers);
		Rect[] cameraRects = new Rect[0];
		if (activeControllers == 1) {
			cameraRects = new Rect[1];
			cameraRects [0] = new Rect (0, 0, 1, 1);
		} else if (activeControllers == 2) {
			cameraRects = new Rect[2];
			cameraRects [0] = new Rect (0, 0.5f, 1, 0.5f);
			cameraRects [1] = new Rect (0, 0, 1, 0.5f);
		} else if (activeControllers == 3) {
			cameraRects = new Rect[3];
			cameraRects [0] = new Rect (0, 0.5f, 0.5f, 0.5f);
			cameraRects [1] = new Rect (0.5f, 0.5f, 0.5f, 0.5f);
			cameraRects [2] = new Rect (0, 0, 1, 0.5f);
		} else if (activeControllers == 4) {
			cameraRects = new Rect[4];
			cameraRects [0] = new Rect (0, 0.5f, 0.5f, 0.5f);
			cameraRects [1] = new Rect (0.5f, 0.5f, 0.5f, 0.5f);
			cameraRects [2] = new Rect (0, 0, 0.5f, 0.5f);
			cameraRects [3] = new Rect (0.5f, 0, 0.5f, 0.5f);
		} else {
			Debug.LogError ("Unsupported number of controllers");
		}

		int nextControllerNum = 0;
		for (int i = 0; i < localInputs.Length; i++) {
			// assign input data
			localPlayerData [i].inputs = localInputs [i];
			localPlayerData [i].cameraRect = cameraRects [nextControllerNum];
			// advance screen number if the right hand side or a full controller input is detected
			if (localInputs [i].inputType == ControllerType.full || localInputs [i].inputType == ControllerType.right) {
				nextControllerNum++;
			}
			// we have a split controller...
			if (localInputs [i].inputType == ControllerType.left) {
				// half the width
				localPlayerData [i].cameraRect.width /= 2;
			} else if (localInputs [i].inputType == ControllerType.right) {
				// half the width and translate it right by half
				localPlayerData [i].cameraRect.width /= 2;
				localPlayerData [i].cameraRect.x += localPlayerData [i].cameraRect.width;
			}
		}
		CreateCameras ();
	}

	void CreateCameras ()
	{
		for (int i = 0; i < localPlayerData.Length; i++) {
			GameObject newObj = Instantiate (cameraPrefab, Map.initialCameraSpawn.position, Map.initialCameraSpawn.rotation);
			MultiPlayerCamera cam = newObj.GetComponent<MultiPlayerCamera> ();
			localPlayerData [i].camera = cam;
			cam.SetCameraViewRect (localPlayerData [i].cameraRect);

		}
	}

	Rect RectComponentMultiply (Rect r1, Rect r2)
	{
		return new Rect (r1.x * r2.x, r1.y * r2.y, r1.width * r2.width, r1.height * r2.height);
	}

	public static void SetCarForController (int controllerNum, BaseCar car)
	{
		singleton._SetCarForController (controllerNum, car);
	}

	void _SetCarForController (int controllerNum, BaseCar car)
	{
		for (int i = 0; i < localPlayerData.Length; i++) {
			if (localPlayerData [i].inputs.controllerIndex == controllerNum) {
				localPlayerData [i].targetCar = car;
				localPlayerData [i].camera.SetCameraTarget (car.transform);
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
}
