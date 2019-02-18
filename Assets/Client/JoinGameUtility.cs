using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinGameUtility : MonoBehaviour {

    public static LocalClientInput localClientInput = null;

    public GameObject joinGameButton;

    private void Start()
    {
        joinGameButton.SetActive(false);
    }

    private void Update()
    {
        if (!NetworkManager.HasJoinedGame)
        {
            joinGameButton.SetActive(false);
            return;
        }

        if(!LocalClientInput.HasClient)
        {
            joinGameButton.SetActive(true);
            return;
        }

        joinGameButton.SetActive(false);
    }

    // Use this for initialization
    public void JoinGame () {
        if (localClientInput != null)
        {
            localClientInput.AddLocalPlayer();
        }
	}

    private void OnDestroy()
    {
        localClientInput = null;
    }
}
