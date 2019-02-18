using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameMenu : MonoBehaviour {

    [SerializeField]
    private BRGameController gameController;

    [SerializeField]
    private GameObject menuObj;
    private bool menuOpen = false;

    public void DoQuit()
    {
        Application.Quit();
    }

    public void ResetRound()
    {
        gameController.ResetRace();
        
    }

    public void LobbyMode()
    {
        gameController.LobbyMode();
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            menuOpen = !menuOpen;
        }

        menuObj.SetActive(menuOpen);
    }

}
