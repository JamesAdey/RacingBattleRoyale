using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControllerType
{
	full,
	left,
	right
}

public struct ControllerData
{
	public string name;
	public int number;
	public bool active;

	public ControllerData (string _name, int _number, bool _active)
	{
		name = _name;
		number = _number;
		active = _active;
	}
}

public struct LocalPlayerInput
{
	public int playerNumber;
	public int controllerIndex;
	public ControllerType inputType;

	public LocalPlayerInput (int plNum, int ctrlNum, ControllerType typ)
	{
		playerNumber = plNum;
		controllerIndex = ctrlNum;
		inputType = typ;
	}
}

public class PlayerInputs : MonoBehaviour
{
	private static int numOfPlayers = 1;
	private static int connectedControllers = 0;
	private static int totalControllers = 0;

	private static ControllerData[] controllerData = new ControllerData[0];

	public static XBoxCtrlInputs player1Inputs = new XBoxCtrlInputs (1);
	public static XBoxCtrlInputs player2Inputs = new XBoxCtrlInputs (2);
	public static XBoxCtrlInputs player3Inputs = new XBoxCtrlInputs (3);
	public static XBoxCtrlInputs player4Inputs = new XBoxCtrlInputs (4);

	private static LocalPlayerInput[] playerInputMapping = new LocalPlayerInput[0];

	public static int GetLocalPlayerCount ()
	{
		return numOfPlayers;
	}

	public static int GetActiveControllers ()
	{
		return connectedControllers;
	}

	public static int GetTotalControllers ()
	{
		return totalControllers;
	}

	public static ControllerData[] GetControllerData ()
	{
		return controllerData;
	}

	/// <summary>
	/// Gets an array of active controller numbers
	/// </summary>
	/// <returns>The active controller number array.</returns>
	public static int[] GetActiveControllerNumbers ()
	{
		List<int> ctrl = new List<int> ();
		for (int i = 0; i < controllerData.Length; i++) {
			if (controllerData [i].active) {
				ctrl.Add (controllerData [i].number);
			}
		}
		return ctrl.ToArray ();
	}

	public static LocalPlayerInput[] GetPlayerInputMapping ()
	{
		return playerInputMapping;
	}

	public static XBoxCtrlInputs GetInputsForControllerNumber (int num)
	{
		if (num == 0) {
			return player1Inputs;
		} else if (num == 1) {
			return player2Inputs;
		} else if (num == 2) {
			return player3Inputs;
		} else if (num == 3) {
			return player4Inputs;
		} else {
			return null;
		}
	}

	void Awake ()
	{
		DontDestroyOnLoad (this);
		// THERE CAN BE ONLY ONE!!!!
		PlayerInputs[] inputs = FindObjectsOfType<PlayerInputs> ();
		if (inputs.Length > 1) {
			Destroy (gameObject);
		}
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		// check how many controllers connected
		string[] names = Input.GetJoystickNames ();
		if (names.Length != controllerData.Length) {
			controllerData = new ControllerData[names.Length];
			totalControllers = names.Length;
		}

		connectedControllers = 0;
		for (int i = 0; i < names.Length; i++) {
			controllerData [i].name = names [i];
			controllerData [i].number = i;
			if (names [i] == null || names [i] == "") {
				controllerData [i].active = false;
				continue;
			}

			controllerData [i].active = true;
			connectedControllers++;
		}
		numOfPlayers = Mathf.Clamp (numOfPlayers, 1, connectedControllers * 2);
	}

	public static void IncreasePlayers ()
	{
		numOfPlayers++;
	}

	public static void DecreasePlayers ()
	{
		numOfPlayers--;
	}

	public static void CreateInputMappings (ControllerMenuSetup setup)
	{
		List<LocalPlayerInput> inputs = new List<LocalPlayerInput> (8);
		int playerNumber = 0;
		numOfPlayers = 0;
		for (int i = 0; i < setup.ctrls.Count; i++) {
			ControllerSelector ctrl = setup.ctrls [i];
			if (!ctrl.ctrlData.active) {
				continue;
			}

			if (ctrl.GetNumPlayersOnController () == 1) {
				inputs.Add (new LocalPlayerInput (playerNumber, ctrl.ctrlData.number, ControllerType.full));
				numOfPlayers += 1;
			} else if (ctrl.GetNumPlayersOnController () == 2) {
				inputs.Add (new LocalPlayerInput (playerNumber, ctrl.ctrlData.number, ControllerType.left));
				inputs.Add (new LocalPlayerInput (++playerNumber, ctrl.ctrlData.number, ControllerType.right));
				numOfPlayers += 2;
			} else {
				Debug.LogError ("MORE THAN 2 PEOPLE ON CONTROLLER " + i);
			}
			playerNumber++;

		}
		playerInputMapping = inputs.ToArray ();
	}
}
