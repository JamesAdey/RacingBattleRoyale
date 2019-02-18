using UnityEngine;
using System.Collections;

[System.Serializable]
public sealed class XBoxCtrlInputs
{
	private int controllerNumber;

	public bool aButton;
	public bool bButton;
	public bool xButton;
	public bool yButton;

	public bool startButton;
	public bool selectButton;

	public float leftStickX;
	public float leftStickY;
	public bool leftStickPressed;
	public float rightStickX;
	public float rightStickY;
	public bool rightStickPressed;

	public float leftTrigger;
	public float rightTrigger;
	public bool leftBumper;
	public bool rightBumper;


	public XBoxCtrlInputs (int playerNum)
	{
		controllerNumber = playerNum;
	}

	// Update is called once per frame
	public void UpdateController ()
	{
		// update our input
		switch (controllerNumber) {
		case 1:
			GetPlayer1Input ();
			break;
		case 2:
			GetPlayer2Input ();
			break;
		case 3:
			GetPlayer3Input ();
			break;

		case 4:
			GetPlayer4Input ();
			break;

		default:
			throw new MissingReferenceException ("This controller index doesn't exist: " + controllerNumber.ToString ());
		}
	}

	private void GetPlayer1Input ()
	{
		/*
		// PC
		leftStickX = Input.GetAxisRaw ("Horizontal");
		leftStickY = Input.GetAxisRaw ("Vertical");
		rightStickX = Input.GetAxisRaw ("Horizontal2");
		rightStickY = Input.GetAxisRaw ("Vertical2");
		leftTrigger = Input.GetAxisRaw ("Fire1");
		rightTrigger = Input.GetAxisRaw ("Fire2");
		leftBumper = Input.GetKey (KeyCode.Q);
		rightBumper = Input.GetKey (KeyCode.U);*/
		// get the input axes
		leftStickX = Input.GetAxisRaw ("Joy1_X");
		leftStickY = Input.GetAxisRaw ("Joy1_Y");
		rightStickX = Input.GetAxisRaw ("Joy1_X2");
		rightStickY = Input.GetAxisRaw ("Joy1_Y2");
		rightTrigger = Input.GetAxisRaw ("Joy1_Fire1");
		leftTrigger = Input.GetAxisRaw ("Joy1_Fire2");

		aButton = Input.GetKey (KeyCode.Joystick1Button0);
		bButton = Input.GetKey (KeyCode.Joystick1Button1);
		xButton = Input.GetKey (KeyCode.Joystick1Button2);
		yButton = Input.GetKey (KeyCode.Joystick1Button3);
		leftBumper = Input.GetKey (KeyCode.Joystick1Button4);
		rightBumper = Input.GetKey (KeyCode.Joystick1Button5);
		selectButton = Input.GetKey (KeyCode.Joystick1Button6);
		startButton = Input.GetKey (KeyCode.Joystick1Button7);
		leftStickPressed = Input.GetKey (KeyCode.Joystick1Button8);
		rightStickPressed = Input.GetKey (KeyCode.Joystick1Button9);

	}

	private void GetPlayer2Input ()
	{
		// get the input axes
		//leftStickX = Input.GetAxisRaw ("Horizontal2");
		//leftStickY = Input.GetAxisRaw ("Vertical2");
		leftStickX = Input.GetAxisRaw ("Joy2_X");
		leftStickY = Input.GetAxisRaw ("Joy2_Y");
		rightStickX = Input.GetAxisRaw ("Joy2_X2");
		rightStickY = Input.GetAxisRaw ("Joy2_Y2");
		rightTrigger = Input.GetAxisRaw ("Joy2_Fire1");
		leftTrigger = Input.GetAxisRaw ("Joy2_Fire2");

		aButton = Input.GetKey (KeyCode.Alpha2);
		bButton = Input.GetKey (KeyCode.Joystick2Button1);
		xButton = Input.GetKey (KeyCode.Joystick2Button2);
		yButton = Input.GetKey (KeyCode.Joystick2Button3);
		leftBumper = Input.GetKey (KeyCode.Joystick2Button4);
		rightBumper = Input.GetKey (KeyCode.Joystick2Button5);
		selectButton = Input.GetKey (KeyCode.Joystick2Button6);
		startButton = Input.GetKey (KeyCode.Joystick2Button7);
		leftStickPressed = Input.GetKey (KeyCode.Joystick2Button8);
		rightStickPressed = Input.GetKey (KeyCode.Joystick2Button9);
	}

	private void GetPlayer3Input ()
	{
		// get the input axes
		//leftStickX = Input.GetAxisRaw ("Horizontal3");
		//leftStickY = Input.GetAxisRaw ("Vertical3");
		leftStickX = Input.GetAxisRaw ("Joy3_X");
		leftStickY = Input.GetAxisRaw ("Joy3_Y");
		rightStickX = Input.GetAxisRaw ("Joy3_X2");
		rightStickY = Input.GetAxisRaw ("Joy3_Y2");
		rightTrigger = Input.GetAxisRaw ("Joy3_Fire1");
		leftTrigger = Input.GetAxisRaw ("Joy3_Fire2");

		aButton = Input.GetKey (KeyCode.Alpha3);
		bButton = Input.GetKey (KeyCode.Joystick3Button1);
		xButton = Input.GetKey (KeyCode.Joystick3Button2);
		yButton = Input.GetKey (KeyCode.Joystick3Button3);
		leftBumper = Input.GetKey (KeyCode.Joystick3Button4);
		rightBumper = Input.GetKey (KeyCode.Joystick3Button5);
		selectButton = Input.GetKey (KeyCode.Joystick3Button6);
		startButton = Input.GetKey (KeyCode.Joystick3Button7);
		leftStickPressed = Input.GetKey (KeyCode.Joystick3Button8);
		rightStickPressed = Input.GetKey (KeyCode.Joystick3Button9);
	}

	private void GetPlayer4Input ()
	{
		// get the input axes
		leftStickX = Input.GetAxisRaw ("Joy4_X");
		leftStickY = Input.GetAxisRaw ("Joy4_Y");
		rightStickX = Input.GetAxisRaw ("Joy4_X2");
		rightStickY = Input.GetAxisRaw ("Joy4_Y2");
		rightTrigger = Input.GetAxisRaw ("Joy4_Fire1");
		leftTrigger = Input.GetAxisRaw ("Joy4_Fire2");

		aButton = Input.GetKey (KeyCode.Joystick4Button0);
		bButton = Input.GetKey (KeyCode.Joystick4Button1);
		xButton = Input.GetKey (KeyCode.Joystick4Button2);
		yButton = Input.GetKey (KeyCode.Joystick4Button3);
		leftBumper = Input.GetKey (KeyCode.Joystick4Button4);
		rightBumper = Input.GetKey (KeyCode.Joystick4Button5);
		selectButton = Input.GetKey (KeyCode.Joystick4Button6);
		startButton = Input.GetKey (KeyCode.Joystick4Button7);
		leftStickPressed = Input.GetKey (KeyCode.Joystick4Button8);
		rightStickPressed = Input.GetKey (KeyCode.Joystick4Button9);
	}

	// TODO make functions for the other 2 players.
}


