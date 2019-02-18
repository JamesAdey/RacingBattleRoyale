using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedCursor : ControllerCursor
{

	public string xAxisName = "Horizontal";
	public string yAxisName = "Vertical";

	public KeyCode useKeyCode = KeyCode.JoystickButton0;
	public KeyCode cancelKeyCode = KeyCode.JoystickButton1;


	protected override void UpdateInput ()
	{
		inputDir.x = Input.GetAxis (xAxisName);
		inputDir.y = Input.GetAxis (yAxisName);

		// pressed "a"
		if (Input.GetKeyDown (useKeyCode)) {
			UseStart ();

		}
		if (Input.GetKeyUp (useKeyCode)) {
			UseEnd ();
		}

		// pressed "b"
		if (Input.GetKeyDown (cancelKeyCode)) {
			CancelStart ();
		}
		if (Input.GetKeyUp (cancelKeyCode)) {
			CancelEnd ();
		}
	}
}
