using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkGUI : MonoBehaviour
{
	[SerializeField]
	private GameObject disconnectedShowObj;
	[SerializeField]
	private GameObject connectedShowObj;

	[SerializeField]
	private InputField addressField;

    private int numClients;
    private int numRacers;

    private void Start()
    {
        Init();
    }

    void Init()
    {
        numClients = 24;
        numRacers = 30;
    }

    public void SetNumClients(string s)
    {
        if(!int.TryParse(s,out numClients))
        {
            numClients = 24;
        }
    }

    public void SetNumRacers(string s)
    {
        if (!int.TryParse(s, out numRacers))
        {
            numRacers = 30;
        }
    }

    public void HostServer ()
	{
        BRNetworkManager.numRacers = numRacers;
        NetworkCore.StartServer (true,numClients);
		// connect to ourselves
		NetworkCore.Connect ("127.0.0.1", 27015);
	}

	public void Connect ()
	{
		NetworkCore.StartClient ();
		NetworkCore.Connect (addressField.text, 27015);
	}

	public void Disconnect ()
	{
		if (NetworkCore.isServer) {
			NetworkCore.CloseServer ();
		} else {
			NetworkCore.Disconnect ();
		}
	}

	void Update ()
	{
		switch (NetworkCore.clientStatus) {
		case ConnectionStatus.Connected:
			connectedShowObj.SetActive (true);
			disconnectedShowObj.SetActive (false);
			break;
		case ConnectionStatus.Disconnected:
			connectedShowObj.SetActive (false);
			disconnectedShowObj.SetActive (true);
			break;
		}

	}
}
