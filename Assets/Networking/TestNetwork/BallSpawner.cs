using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnGUI()
    {
        if(GUI.Button(new Rect(300,0,100,30),"Spawn Ball"))
        {
            NetworkPrefab ball = NetworkManager.FindNetworkPrefab("ball");
            NetworkManager.singleton.NetworkInstantiate(ball, Vector3.zero, Quaternion.identity, false);
        }
    }
}
