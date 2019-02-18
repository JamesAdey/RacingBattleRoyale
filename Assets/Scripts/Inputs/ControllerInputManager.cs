using UnityEngine;
using System.Collections;

public class ControllerInputManager : MonoBehaviour
{
	
	// Update is called once per frame
	void Update ()
	{
		PlayerInputs.player1Inputs.UpdateController ();	
		PlayerInputs.player2Inputs.UpdateController ();
		PlayerInputs.player3Inputs.UpdateController ();
		PlayerInputs.player4Inputs.UpdateController ();
	}
}
