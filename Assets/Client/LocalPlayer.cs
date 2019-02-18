using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer
{

    public PlayerInput input = PlayerInput.None;
    public int playerNumber = -1;
    public NetworkID vehicleID;
    internal BaseCar car = null;

    public string driveAxis = "P1Drive";
    public string steerAxis = "P1Steer";
    public string useAxis = "P1Use";

    public void CollectInputs()
    {
        input.driveInput = Input.GetAxisRaw(driveAxis);
        input.steerInput = Input.GetAxisRaw(steerAxis);
        input.useInput = Input.GetAxisRaw(useAxis);
        
    }

}
