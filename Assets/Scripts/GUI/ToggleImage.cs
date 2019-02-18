using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Image))]
public class ToggleImage : MonoBehaviour
{

	public Color onColor = Color.white;
	public Color offColor = Color.gray;

	private Image myImage;

	void Awake ()
	{
		myImage = GetComponent<Image> ();
	}

	public void OnToggleChanged (bool val)
	{
		if (val) {
			myImage.color = onColor;
		} else {
			myImage.color = offColor;
		}
	}
}
