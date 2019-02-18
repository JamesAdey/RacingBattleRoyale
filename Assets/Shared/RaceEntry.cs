using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaceEntry {
    public BaseCar vehicle;
    public int vehicleNumber;
    public string racerName;
    public PlayerType playerType;
    public RacerState state;

    public RaceEntry(BaseCar car, int number, string name, PlayerType typ)
    {
        this.vehicle = car;
        this.vehicleNumber = number;
        this.racerName = name;
        this.playerType = typ;
        this.state = RacerState.OUT;
    }
}
