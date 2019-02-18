using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum RaceState
{
    LOBBY,
    RACING,
    OVER
}

public class Race
{

    private List<BaseCar> racers = new List<BaseCar>();

    public float StartDelay { get; private set; }
    public float KillDelay { get; private set; }
    public int GatesPerLap { get; private set; }
    public RaceState State { get; private set; }
    public bool RaceStarted { get; private set; }

    private float nextKillTime;

    public float roundTime = 20f;
    public int nodeCount = 20;
    private BaseCar lastCar;

    private BRGameController gameController;

    private float updateInterval;
    private float nextUpdateTime;

    public Race(BRGameController ctrl)
    {
        gameController = ctrl;
    }

    public void Init(float updateInterval, float startDelay, float killDelay, int gatesPerLap)
    {
        this.updateInterval = updateInterval;
        nextUpdateTime = 0f;
        StartDelay = startDelay;
        KillDelay = killDelay;
        GatesPerLap = gatesPerLap;
        State = RaceState.LOBBY;
    }

    internal void ForceEnd()
    {
        // force an end to the race
        State = RaceState.OVER;
    }

    public void AddRacer(BaseCar racer)
    {
        racers.Add(racer);
    }

    public BaseCar GetCurrentLeader()
    {
        if (racers.Count > 0)
        {
            return racers[0];
        }

        return null;
    }

    public void StartRace()
    {
        nextKillTime = Time.time + StartDelay;
        State = RaceState.RACING;
    }

    public void UpdateRace()
    {
        if (State != RaceState.RACING)
        {
            return;
        }

        if(Time.time < nextUpdateTime)
        {
            return;
        }
        nextUpdateTime = Time.time + updateInterval;

        // sort the cars
        SortCars();
        
        // check for being lapped
        CheckLapped();
    }

    public void UpdateClients() {

        float killDelta = nextKillTime - Time.time;

        for(int i = 0; i < racers.Count; i++) {
            GamePlayer player = racers[i].pDriver;
            if(player.playerType == PlayerType.REMOTE)
            {
                float timeLeft = killDelta + (racers.Count - i - 1) * KillDelay;
                int placesLeft = Mathf.Min(racers.Count - i,5);
                gameController.SendRaceInfo(player, i+1, placesLeft,timeLeft);
            }
        }
    }

    private void SortCars()
    {
        for (int i = 1; i < racers.Count; i++)
        {
            for (int x = i; x > 0; x--)
            {
                // first sort by gates passed
                if (racers[x].tracking.gatesPassed > racers[x - 1].tracking.gatesPassed)
                {
                    BaseCar temp = racers[x];
                    racers[x] = racers[x - 1];
                    racers[x - 1] = temp;
                }
                // then sort by distance
                else if (racers[x].tracking.gatesPassed == racers[x - 1].tracking.gatesPassed)
                {
                    if (racers[x].tracking.sqrDistToGate < racers[x - 1].tracking.sqrDistToGate)
                    {
                        BaseCar temp = racers[x];
                        racers[x] = racers[x - 1];
                        racers[x - 1] = temp;
                    }
                    else
                    {
                        // in the correct place :)
                        break;
                    }
                }

            }
        }
    }

    private void CheckLapped()
    {
        if (State != RaceState.RACING || racers.Count == 0)
        {
            return;
        }

        // check if last place has swapped
        if (lastCar != racers[racers.Count - 1])
        {
            nextKillTime = Time.time + KillDelay;
            lastCar = racers[racers.Count - 1];
        }
        // check if timer has expired
        else if (Time.time > nextKillTime)
        {
            nextKillTime = Time.time + KillDelay;
            // kill.
            KillRacer(racers[racers.Count - 1]);
            racers.RemoveAt(racers.Count - 1);
            return;
        }

        // find gates of leader
        int topGates = racers[0].tracking.gatesPassed;

        // backtrack up the list
        for (int i = racers.Count - 1; i > 0; i--)
        {
            int gateDiff = topGates - racers[i].tracking.gatesPassed;
            // kill if at least 1 gate ahead
            if (gateDiff > nodeCount)
            {
                KillRacer(racers[i]);
                racers.RemoveAt(i);
                break;
            }
            else if (gateDiff == nodeCount)
            {
                // kill if the leader is closer to the next gate than we are
                if (racers[0].tracking.sqrDistToGate < racers[i].tracking.sqrDistToGate)
                {
                    KillRacer(racers[i]);
                    racers.RemoveAt(i);
                    break;
                }
            }
        }

    }

    void KillRacer(BaseCar racer)
    {
        // notify the game controller to remove this racer
        gameController.RacerOut(racer);
    }

    public void DrawGUI()
    {
        if (racers.Count == 0 || State != RaceState.RACING)
        {
            return;
        }
        float timeLeft = nextKillTime - Time.time;
        GUI.Box(new Rect(Screen.width - 100, 5, 100, 30), timeLeft.ToString());
        GUI.Box(new Rect(Screen.width - 200, 5, 100, 30), racers[racers.Count - 1].name);
        for (int i = 0; i < racers.Count; i++)
        {
            GUI.Box(new Rect(0, 25 * i, 200, 25), racers[i].name + " | " + racers[i].tracking.gatesPassed + " | " + racers[i].tracking.sqrDistToGate.ToString("0.00"));
        }
    }
}

