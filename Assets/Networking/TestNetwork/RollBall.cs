using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollBall : NetBehaviour {

    Rigidbody thisRigidbody;
    Transform thisTransform;

    public Vector3 forceVect = Vector3.right;

    public override void Start()
    {
        thisRigidbody = GetComponent<Rigidbody>();
        thisTransform = this.transform;
        base.Start();
    }

    public override void ServerStart()
    {
        // do nothing
    }

    public override void ClientStart()
    {
        // do nothing
    }

    public override void OnNewNetworkSceneLoaded()
    {
        NetworkManager.AddNetFunctionListener("updateBall", thisNetworkID, BallMove);
        NetworkManager.NetworkUpdateEvent += NetworkUpdate;
    }

    void BallMove(NetReader reader)
    {
        thisRigidbody.MovePosition(reader.ReadVector3());
        thisRigidbody.velocity = reader.ReadVector3();
        thisTransform.eulerAngles = reader.ReadVector3();
    }
	
	// Update is called once per frame
	void NetworkUpdate () {
        if (NetworkCore.isServer)
        {
            thisRigidbody.AddForce(forceVect * Mathf.Sin(Time.time) * 2, ForceMode.Acceleration);
            NetWriter writer = NetworkManager.StartNetworkMessage("updateBall", thisNetworkID);
            writer.WriteVector3(thisRigidbody.position);
            writer.WriteVector3(thisRigidbody.velocity);
            writer.WriteVector3(thisTransform.eulerAngles);
            NetworkManager.SendMessageToOtherClients(writer, NetworkCore.UnreliableSequencedMsg,false);
        }
	}
}
