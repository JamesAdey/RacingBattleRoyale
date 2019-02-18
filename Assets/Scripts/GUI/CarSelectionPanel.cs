using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSelectionPanel : MonoBehaviour
{

	public int selectionNum;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void MoveLeft ()
	{

	}

	void MoveRight ()
	{
		
	}

	void WrapSelection ()
	{
		if (selectionNum < 0) {
			selectionNum = 0;
		} else if (selectionNum > CarDataStore.maxCars) {

		}
	}
}
