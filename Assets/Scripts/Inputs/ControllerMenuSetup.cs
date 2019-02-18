using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControllerMenuSetup : MonoBehaviour
{

	public static string nextLevelName = "Mountainside";

	public Text playerNumDisplayText;
	public Text controllerNumDisplayText;

	public GameObject ctrlPrefab;
	public ControllerCursor sharedCursor;


	public List<ControllerSelector> ctrls = new List<ControllerSelector> (4);


	public Vector2 ctrlGuiSize = new Vector2 (150, 0);
	public Vector2 gap = new Vector2 (10, 0);

	int controllerCount;
	int totalControllers;
	float newCtrlTime;

	void Awake ()
	{
		for (int i = 0; i < ctrls.Count; i++) {
			ctrls [i].gameObject.SetActive (false);
		}
	}

	void Update ()
	{
		UpdateGraphics ();
	}

	void UpdateGraphics ()
	{
		playerNumDisplayText.text = PlayerInputs.GetLocalPlayerCount ().ToString ();
		controllerNumDisplayText.text = PlayerInputs.GetActiveControllers ().ToString ();

		// keep number of input panels the same as total controllers
		if (totalControllers != PlayerInputs.GetTotalControllers ()) {
			totalControllers = PlayerInputs.GetTotalControllers ();
			for (int i = ctrls.Count; i < totalControllers; i++) {
				GameObject newObj = Instantiate (ctrlPrefab, Vector3.zero, Quaternion.identity);
				ControllerSelector selector = newObj.GetComponent<ControllerSelector> ();
				ctrls.Add (selector);
			}
		}

		// update the appropriate panels based off active controllers
		if (controllerCount != PlayerInputs.GetActiveControllers ()) {
			ControllerData[] controllerData = PlayerInputs.GetControllerData ();

			// set controller data to panels
			controllerCount = PlayerInputs.GetActiveControllers ();
			for (int i = 0; i < controllerData.Length; i++) {
				if (controllerData [i].active) {
					ctrls [i].SetControllerData (controllerData [i]);
				} else {
					ctrls [i].gameObject.SetActive (false);
				}
			}

			// reset the cursor
			sharedCursor.FindNewSelectable ();

			int numGaps = controllerCount - 1;
			Vector2 totalSize = ctrlGuiSize * controllerCount + numGaps * gap;
			Vector2 midPoint = totalSize / 2;
			Vector2 farLeft = -midPoint + (ctrlGuiSize / 2);
			farLeft.x += Display.main.renderingWidth / 2;
			farLeft.y += Display.main.renderingHeight / 2;
			newCtrlTime = Time.time;
			Vector2 nextPos = farLeft;
			for (int i = 0; i < controllerData.Length; i++) {
				if (!controllerData [i].active) {
					continue;
				}
				ctrls [i].SetDesiredPos (nextPos);
				nextPos += gap + ctrlGuiSize;
			}
		}

	}

	public void IncreasePlayerCount ()
	{
		PlayerInputs.IncreasePlayers ();
		Debug.Log ("inc players");
	}

	public void DecreasePlayerCount ()
	{
		PlayerInputs.DecreasePlayers ();
	}

	/// <summary>
	/// Confirms the controller selection, and moves to the next level
	/// </summary>
	public void ConfirmControllerSelection ()
	{
		PlayerInputs.CreateInputMappings (this);
		SceneManager.LoadScene (nextLevelName);
	}
}
