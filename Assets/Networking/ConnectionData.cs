using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionData
{
	public int connectionId;
	public bool active;

	public ConnectionData (int _connId)
	{
		connectionId = _connId;
		active = false;
	}
}
