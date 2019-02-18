using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleThruster : MonoBehaviour
{
	Transform thisTransform;
	ParticleSystem particles;
	bool lastThrust = false;
	public float force = 20;

	// Use this for initialization
	void Start ()
	{
		particles = GetComponent<ParticleSystem> ();
		thisTransform = this.transform;
		particles.Stop (true);
	}

	public void SetThruster (bool thrust)
	{
		if (thrust != lastThrust) {
			lastThrust = thrust;
			if (thrust) {
				particles.Play (true);
			} else {
				particles.Stop (true);
			}
		}
	}

	public Transform GetTransform ()
	{
		return thisTransform;
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
}
