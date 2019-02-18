using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceMenu : MonoBehaviour {

    [SerializeField]
    private GameObject startObj;
    [SerializeField]
    private GameObject stopObj;

    [SerializeField]
    private BRGameController gameController;

    public void StartRace()
    {
        gameController.BeginRace();
    }

    public void StopRace()
    {
        gameController.LobbyMode();
    }

    void FixedUpdate()
    {
        RaceState state = BRGameController.CurrentRaceState();

        bool canStart = false;
        bool canStop = false;
        switch (state)
        {
            case RaceState.LOBBY:
                canStart = true;
                break;
            case RaceState.RACING:
                canStop = true;
                break;
        }
        startObj.SetActive(canStart);
        stopObj.SetActive(canStop);
    }
}
