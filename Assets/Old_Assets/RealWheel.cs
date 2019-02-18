using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealWheel : MonoBehaviour
{

	public WheelType wheelType;
	public Transform wheelGraphic;

	RealCar car;

	public Vector3 steerAxis = Vector3.up;
	public Vector3 turnAxis = Vector3.right;
	public WheelCollider wheelCollider;

	// Use this for initialization
	void Start ()
	{
		if (!wheelGraphic) {
			Debug.LogError ("WHEEL WITH NO GRAPHICS!!");
			return;
		}
		car = this.GetComponentInParent<RealCar> ();
		if (!car) {
			Debug.LogError ("WHEEL WITH NO CAR!!");
			return;
		}
		wheelCollider = this.GetComponent<WheelCollider> ();
		// auto fill the cars wheel arrays
		if (wheelType == WheelType.front) {
			car.AddFrontWheel (this);
		} else {
			car.AddRearWheel (this);
		}
	}

	// Update is called once per frame
	void Update ()
	{
		Vector3 pos;
		Quaternion rot;
		wheelCollider.GetWorldPose (out pos, out rot);
		wheelGraphic.position = pos;
		// rotation from moving
		Quaternion movementRot = Quaternion.Euler (turnAxis * wheelCollider.rpm * Time.deltaTime);
		wheelGraphic.rotation = rot * movementRot;

	}
}
