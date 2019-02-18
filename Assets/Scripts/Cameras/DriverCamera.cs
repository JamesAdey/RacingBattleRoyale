using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriverCamera : MultiPlayerCamera
{
	private Camera cam;
	public Transform target;
	private Transform thisTransform;

	void Start ()
	{
		thisTransform = this.transform;
	}

	// Update is called once per frame
	void LateUpdate ()
	{
		thisTransform.position = target.position;
		thisTransform.rotation = target.rotation;
	}


	public override void SetCameraTarget (Transform t)
	{
		target = t;
	}

	public override void SetCameraViewRect (Rect rect)
	{
		// get camera and assign
		cam = this.GetComponent<Camera> ();
		cam.rect = rect;
	}
}
