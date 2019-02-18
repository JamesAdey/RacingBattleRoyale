using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class NetEditorHooks
{
	[MenuItem ("Network/Recalculate Scene IDs")]
	public static void RecalcSceneIDs ()
	{
		NetworkManager.RecalculateSceneIDs ();
	}
}
