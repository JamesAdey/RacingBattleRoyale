using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
	
	public abstract void Fire ();

	public abstract void AddAmmoPickup ();
}
