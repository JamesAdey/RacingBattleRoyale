using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class NetWriter
{
	private byte[] rawData;
	private int maxLength;
	private int index;

	public bool isWriting {
		get {
			return _isWriting;
		}

		private set {
			_isWriting = value;
		}
	}

	private bool _isWriting;

	public void Reset (byte[] data, int length)
	{
		this.rawData = data;
		this.maxLength = length;
		index = 0;
		_isWriting = false;
	}

	private bool WillOverflow (int sizeToWrite)
	{
		return (index + sizeToWrite >= maxLength);
	}

	public byte[] GetRawData ()
	{
		return rawData;
	}

	public int GetWrittenBytes ()
	{
		return index;
	}

	public void StartWriting ()
	{
		isWriting = true;
	}

	public void StopWriting ()
	{
		isWriting = false;
	}

	#region Write Functions

	public bool WriteBytes (byte[] data)
	{
		if (WillOverflow (data.Length)) {
			throw new IndexOutOfRangeException ("Data array overflow when writing bytes");
		}

		data.CopyTo (rawData, index);
		index += data.Length;
		return true;
	}

	public bool WriteByte (byte val)
	{
		if (WillOverflow (1)) {
			throw new IndexOutOfRangeException ("Data array overflow when writing byte");
		}
		rawData [index] = val;
		index += 1;
		return true;
	}

	public bool WriteBool (bool val)
	{
		return WriteBytes (BitConverter.GetBytes (val));
	}

	public bool WriteUShort (ushort val)
	{
		return WriteBytes (BitConverter.GetBytes (val));
	}

	public bool WriteInt (int val)
	{
		return WriteBytes (BitConverter.GetBytes (val));
	}

	public bool WriteFloat (float val)
	{
		return WriteBytes (BitConverter.GetBytes (val));
	}

	public bool WriteVector3 (Vector3 val)
	{
		// write separately to ensure correct order
		bool r = WriteFloat (val.x);
		r = r && WriteFloat (val.y);
		return  r && WriteFloat (val.z);
	}

	#endregion
}
