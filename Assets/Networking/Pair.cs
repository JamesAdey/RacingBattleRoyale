using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pair structure for storing data
/// </summary>
public class Pair<A,B>
{
	public A left;
	public B right;

	public void Set (A l, B r)
	{
		left = l;
		right = r;
	}
}
