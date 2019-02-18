using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class NetReader
{
    /// <summary>
    /// This is the connection id of the sender
    /// </summary>
    public int connectionId = -1;
	private byte[] data;
	private int maxLength;
	private int index;

	internal void Reset (byte[] data, int length)
	{
		this.data = data;
		this.maxLength = length;
		this.index = 0;
	}

	private bool WillOverflow (int sizeToRead)
	{
		return (index + sizeToRead > maxLength);
	}

	#region Read Functions

	public byte ReadByte ()
	{
		if (WillOverflow (1)) {
			throw new IndexOutOfRangeException ("Data array overflow when reading byte");
		}
		byte outByte = data [index];
		index += 1;
		return outByte;
	}

	public bool ReadBool ()
	{
		if (WillOverflow (1)) {
			throw new IndexOutOfRangeException ("Data array overflow when reading bool");
		}
		bool outBool = BitConverter.ToBoolean (data, index);
		index += 1;
		return outBool;
	}

	public ushort ReadUShort ()
	{
		if (WillOverflow (2)) {
			throw new IndexOutOfRangeException ("Data array overflow when reading byte");
		}
		ushort outuShort = BitConverter.ToUInt16 (data, index);
		index += 2;
		return outuShort;
	}

	public int ReadInt ()
	{
		if (WillOverflow (4)) {
			throw new IndexOutOfRangeException ("Data array overflow when reading integer");
		}
		int outInt = BitConverter.ToInt32 (data, index);
		index += 4;
		return outInt;
	}

	public float ReadFloat ()
	{
		if (WillOverflow (4)) {
			throw new IndexOutOfRangeException ("Data array overflow when reading float");
		}
		float outFloat = BitConverter.ToSingle (data, index);
		index += 4;
		return outFloat;
	}

	public Vector3 ReadVector3 ()
	{
		if (WillOverflow (12)) {
			throw new IndexOutOfRangeException ("Data array overflow when reading Vector3");
		}
		Vector3 output = Vector3.zero;
		output.x = ReadFloat ();
		output.y = ReadFloat ();
		output.z = ReadFloat ();
		return output;
	}

	#endregion

}
