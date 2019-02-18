using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIMessage : MonoBehaviour
{

	private static GUIMessage singleton;
	[SerializeField]
	private Text textComp;

	private bool isOpen = false;

	void Awake ()
	{
		singleton = this;
	}

	void OnDestroy ()
	{
		singleton = null;
	}

	void Start ()
	{
		SetVisible (false);
	}

	public void SetVisible (bool show)
	{
		gameObject.SetActive (show);
	}

	public static void Show (string msg)
	{
		if (!singleton) {
			return;
		}
        singleton.isOpen = true;
		singleton.textComp.text = msg;
		singleton.SetVisible (true);
	}
}
