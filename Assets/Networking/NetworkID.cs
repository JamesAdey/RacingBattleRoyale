using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NetworkIDType : byte
{
	scene,
	game
}

[System.Serializable]
public struct NetworkID
{
	public byte idNumber {
		get {
			return _idNumber;
		}
		private set {
			_idNumber = value;
		}
	}

	[SerializeField]
	private byte _idNumber;

	public NetworkIDType type {
		get {
			return _type;
		}
		private set {
			_type = value;
		}
	}

	public override int GetHashCode ()
	{
		return _idNumber + (1024 * (int)_type);
	}

	[SerializeField]
	private NetworkIDType _type;

	internal NetworkID (byte _num, NetworkIDType _typ)
	{
		_idNumber = _num;
		_type = _typ;
	}

	public static bool operator == (NetworkID n1, NetworkID n2)
	{
		return n1.idNumber == n2.idNumber && n1.type == n2.type;
	}

	public static bool operator != (NetworkID n1, NetworkID n2)
	{
		return n1.idNumber != n2.idNumber || n1.type != n2.type;
	}

	public static NetworkID GetDataFromBytes (NetReader reader)
	{
		NetworkID id = new NetworkID ();	
		id.idNumber = reader.ReadByte ();
		id.type = (NetworkIDType)reader.ReadByte ();
		return id;
	}

	public bool WriteBytes (NetWriter writer)
	{
		writer.WriteByte (idNumber);
		writer.WriteByte ((byte)type);
		return true;
	}

    public override string ToString()
    {
        return type + ":" + idNumber;
    }
}
