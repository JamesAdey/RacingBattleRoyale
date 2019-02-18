using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlayerInput {

    public byte tick;
    public float driveInput;
    public float steerInput;
    public float useInput;

    public static PlayerInput None = new PlayerInput(0,0,0);

    public PlayerInput(float drive, float steer, float use)
    {
        this.driveInput = drive;
        this.steerInput = steer;
        this.useInput = use;
        this.tick = 0;
    }
}
