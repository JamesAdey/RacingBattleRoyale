using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeCar2 : MonoBehaviour
{
	Transform thisTransform;
	Rigidbody thisRigidbody;

	public float currentSpeed = 0f;
	public float maxSpeed = 15;
	public bool isDrifting = true;

	public float grip = 0.8f;

	// Use this for initialization
	void Start ()
	{
		thisTransform = this.transform;
		thisRigidbody = this.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	const float mod = 10;

	void FixedUpdate ()
	{
		currentSpeed = thisRigidbody.velocity.magnitude;
		float steerInput = Input.GetAxis ("Horizontal");
		float driveInput = Input.GetAxis ("Vertical");
		isDrifting = Input.GetKey (KeyCode.Space);

		// do the important calculations
		float speedPercentage = currentSpeed / maxSpeed;
		//float speedRadius = Mathf.Lerp (minSpeedRadius, maxSpeedRadius, speedPercentage);
		//float turnAngle = Mathf.Lerp (maxTurnAngle, minTurnAngle, speedPercentage);

		Vector3 relativeVel = thisTransform.InverseTransformVector (thisRigidbody.velocity);
		Vector3 relativeAngularVel = thisTransform.InverseTransformVector (thisRigidbody.angularVelocity);


		if (Mathf.Abs (driveInput) > 0.1f) {
			thisRigidbody.AddRelativeForce (Vector3.forward * (1 - speedPercentage) * driveInput * mod, ForceMode.Acceleration);
		} else {
			thisRigidbody.AddRelativeForce (Vector3.forward * -relativeVel.z * grip * mod, ForceMode.Acceleration);
		}
		thisRigidbody.AddRelativeTorque (Vector3.up * steerInput * mod, ForceMode.Acceleration);

		thisRigidbody.AddRelativeForce (Vector3.right * -relativeVel.x * grip * mod, ForceMode.Acceleration);
		thisRigidbody.AddRelativeTorque (Vector3.up * -relativeAngularVel.y * grip * mod, ForceMode.Acceleration);
	}
}
