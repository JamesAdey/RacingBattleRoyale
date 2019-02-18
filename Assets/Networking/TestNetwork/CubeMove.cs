using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMove : NetBehaviour
{
	Transform thisTransform;
	public Vector3 movePos;

	public float speed = 1;

	public override void OnNewNetworkSceneLoaded ()
	{
		NetworkManager.AddNetFunctionListener ("cubemove", thisNetworkID, DoMove);
		NetworkManager.NetworkUpdateEvent += NetworkUpdate;
	}

	// Use this for initialization
	public override void Start ()
	{
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

    // Update is called once per frame
    void Update ()
	{

		if (NetworkCore.isServer) {

			movePos.x = Mathf.Sin (Time.time);
			movePos.y = Mathf.Cos (Time.time);
			movePos.z = -Mathf.Sin (Time.time);
			movePos *= speed;
		}
		thisTransform.position = movePos;
	}

	void DoMove (NetReader reader)
	{
		movePos.x = reader.ReadFloat ();
		movePos.y = reader.ReadFloat ();
		movePos.z = reader.ReadFloat ();
	}

	void NetworkUpdate ()
	{

		if (NetworkCore.isServer) {
			NetWriter writer = NetworkManager.StartNetworkMessage ("cubemove", thisNetworkID);
			writer.WriteFloat (thisTransform.position.x);
			writer.WriteFloat (thisTransform.position.y);
			writer.WriteFloat (thisTransform.position.z);
			NetworkManager.SendMessageToOtherClients (writer, NetworkCore.UnreliableMsg,false);
		}

	}
}
