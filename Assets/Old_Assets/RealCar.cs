using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealCar : MonoBehaviour
{
	float currentSpeed;
	Rigidbody thisRigidbody;

	List<RealWheel> frontWheels = new List<RealWheel> ();
	List<RealWheel> rearWheels = new List<RealWheel> ();

	public float maxSpeed = 20;

	public float driveMotorTorque = 350;
	public float maxSteerAngle = 30;

	public float acceleration = 3;

	public Vector3 comOffset = Vector3.down;

	public void AddFrontWheel (RealWheel wh)
	{
		frontWheels.Add (wh);
	}

	public void AddRearWheel (RealWheel wh)
	{
		rearWheels.Add (wh);
	}

	// Use this for initialization
	void Start ()
	{
		thisRigidbody = this.GetComponent<Rigidbody> ();
	}

	// Update is called once per frame
	void Update ()
	{
		thisRigidbody.centerOfMass = comOffset;
		currentSpeed = thisRigidbody.velocity.magnitude;
		float steerInput = Input.GetAxis ("Horizontal");
		float driveInput = Input.GetAxis ("Vertical");

		float speedPercentage = currentSpeed / maxSpeed;

		float accelerationBonus = (1 - speedPercentage) * acceleration;

		float motorTorque = 0;
		if (speedPercentage < 1) {
			motorTorque = driveMotorTorque;
		}
		motorTorque *= acceleration;

		for (int i = 0; i < frontWheels.Count; i++) {
			WheelCollider wc = frontWheels [i].wheelCollider;
			wc.motorTorque = motorTorque * driveInput;
			wc.steerAngle = maxSteerAngle * steerInput;
		}
		for (int i = 0; i < rearWheels.Count; i++) {
			WheelCollider wc = rearWheels [i].wheelCollider;
			wc.motorTorque = motorTorque * driveInput;
			wc.steerAngle = -maxSteerAngle * steerInput;
		}
	}

	void OnDrawGizmos ()
	{
		if (thisRigidbody) {
			Gizmos.DrawSphere (thisRigidbody.worldCenterOfMass, 0.2f);
		}
	}

	void OnGUI ()
	{
		GUI.Box (new Rect (5, 5, 100, 30), currentSpeed.ToString ());
	}
}
