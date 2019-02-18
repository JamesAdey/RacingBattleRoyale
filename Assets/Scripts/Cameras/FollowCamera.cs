using UnityEngine;
using System.Collections;

public class FollowCamera : MultiPlayerCamera
{
	private Camera cam;
	public Transform target;
	public Vector3 viewOffset;
	private Transform thisTransform;

	public float distance = 5;
	public float height = 1.5f;
	public float speed = 5;
	private Vector3 targetPos;

	// Use this for initialization
	void Start ()
	{
		thisTransform = this.transform;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if (!target) {
			return;
		}

		targetPos = target.position - target.forward * distance + Vector3.up * height;

        thisTransform.position = Vector3.LerpUnclamped (thisTransform.position, targetPos, Time.deltaTime * speed);
		thisTransform.LookAt (target.position + (target.rotation * viewOffset));
	}

	void LateUpdate ()
	{
		if (!target) {
			return;
		}

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
