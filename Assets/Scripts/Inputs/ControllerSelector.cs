using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerSelector : MonoBehaviour
{

	public ControllerData ctrlData;

	GameObject thisGameObject;
	Transform thisTransform;
	public Text ctrlNumberText;

	public Toggle fullCtrlToggle;
	public Toggle splitCtrlToggle;

	Vector3 desiredPos;
	float lastPosUpdateTime;

	// Use this for initialization
	void Start ()
	{
		thisGameObject = gameObject;
		thisTransform = transform;
		fullCtrlToggle.isOn = true;
	}

	public void SetControllerData (ControllerData data)
	{
		ctrlData = data;
		ctrlNumberText.text = (data.number + 1).ToString ();
		gameObject.SetActive (data.active);
	}

	public int GetNumPlayersOnController ()
	{
		if (fullCtrlToggle.isOn) {
			return 1;
		}
		if (splitCtrlToggle.isOn) {
			return 2;
		}
		return 0;
	}

	public void SetDesiredPos (Vector3 p)
	{
		desiredPos = p;
		lastPosUpdateTime = Time.time;
	}

	// Update is called once per frame
	void Update ()
	{
		thisTransform.position = Vector3.Lerp (thisTransform.position, desiredPos, Time.time - lastPosUpdateTime);
	}
}
