using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeCar : MonoBehaviour
{

	Transform thisTransform;
	Rigidbody thisRigidbody;

	public float enginePower = 10;
	public float acceleration = 2;
	public float slideFriction = 0.7f;

	public float maxTurnAngle = 30;
	public float minTurnAngle = 5;

	public float currentSpeed = 0f;
	public float maxSpeed = 15;
	public bool isDrifting = true;
	public float minSpeedRadius = 3;
	public float maxSpeedRadius = 10;

	public float minDriftRadius = 5;
	public float maxDriftRadius = -15;

	public float steerDir = 0;
	float turnDir = 1;
	float driftRadius = 0;
	public float turnRadius = 0;

	Vector3 rotationCenter;

	// Use this for initialization
	void Start ()
	{
		thisTransform = this.transform;
		thisRigidbody = this.GetComponent<Rigidbody> ();
	}


	// Update is called once per frame
	void FixedUpdate ()
	{
		currentSpeed = thisRigidbody.velocity.magnitude;
		float steerInput = Input.GetAxis ("Horizontal");
		float driveInput = Input.GetAxis ("Vertical");
		isDrifting = Input.GetKey (KeyCode.Space);

		// do the important calculations
		float speedPercentage = currentSpeed / maxSpeed;
		float speedRadius = Mathf.Lerp (minSpeedRadius, maxSpeedRadius, speedPercentage);
		float turnAngle = Mathf.Lerp (maxTurnAngle, minTurnAngle, speedPercentage);


		// hold turning direction for drift
		if (isDrifting) {
			// map -1 -> 1 onto 0 -> 1
			steerDir = (steerInput + 1) / 2;

			driftRadius = Mathf.Lerp (minDriftRadius, maxDriftRadius, steerDir);
		} else {
			driftRadius = 0;
			turnDir = steerInput;
		}

		turnRadius = (speedRadius + driftRadius) * turnDir;

		rotationCenter = thisRigidbody.position + thisTransform.right * turnRadius;
		float centripetalAcc = 0;
		if (Mathf.Abs (turnRadius) > 1) {
			centripetalAcc = thisRigidbody.velocity.sqrMagnitude / turnRadius;
		}

		// add steering forces
		Vector3 rotationDir = Vector3.zero;

		// drift... ooooooooh
		if (isDrifting) {
			// don't drive, just drift...
			rotationDir = thisTransform.right * centripetalAcc;
			thisRigidbody.AddForce (rotationDir, ForceMode.VelocityChange);
			float angVel = currentSpeed / turnRadius;
			thisTransform.Rotate (thisTransform.up * angVel, Space.Self);
		}
		// drive!
		else {
			// only drive if less than max speed 		
			if (speedPercentage < 1) {
				Vector3 driveForce = thisTransform.forward * driveInput * enginePower * acceleration;

				thisRigidbody.AddForce (driveForce, ForceMode.Acceleration);
			}

			// stop sliding since we're not drifting
			Vector3 relativeVelocity = thisTransform.InverseTransformVector (thisRigidbody.velocity);
			thisRigidbody.AddRelativeForce (Vector3.right * -relativeVelocity.x * slideFriction, ForceMode.VelocityChange);
		}




		Vector3 nextPos = thisRigidbody.position + thisRigidbody.velocity * Time.fixedDeltaTime;
		float angle = Vector3.Angle (thisTransform.forward, thisRigidbody.velocity);

	}

	void OnDrawGizmos ()
	{
		
		if (thisTransform != null) {
			Gizmos.DrawLine (thisTransform.position, rotationCenter);	
			Gizmos.DrawSphere (rotationCenter, 0.5f);
		}
	}
}
