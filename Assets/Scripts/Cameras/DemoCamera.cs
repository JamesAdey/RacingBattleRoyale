using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoCamera : MonoBehaviour {

    LeaderCamera leaderCam;
    FollowCamera followCam;

	// Use this for initialization
	void Start () {
        leaderCam = GetComponent<LeaderCamera>();
        followCam = GetComponent<FollowCamera>();
        leaderCam.enabled = true;
        followCam.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            followCam.target = leaderCam.target;
            BaseCar car = followCam.target.GetComponent<BaseCar>();
            car.ChangeInputs("player");
        }
	}
}
