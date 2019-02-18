using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct UIConnection
{
	public Vector2 direction;
	public SelectableUI target;

	public UIConnection (Vector2 dir, SelectableUI tgt)
	{
		direction = dir;
		target = tgt;
	}
}

public class SelectableUI : MonoBehaviour
{

	public List<UIConnection> connections = new List<UIConnection> ();
	RectTransform rectTransform;
	Vector2 canvasCenter;
	// Use this for initialization
	void Start ()
	{
		
	}

	public void Init ()
	{
		rectTransform = GetComponent<RectTransform> ();
		Vector3[] corners = new Vector3[4];
		rectTransform.GetWorldCorners (corners);
		Vector3 avg = Vector3.zero;
		for (int i = 0; i < corners.Length; i++) {
			avg += corners [i];
		}
		avg /= corners.Length;
		canvasCenter = avg;
	}

	public void ConnectNearby ()
	{
			
		connections.Clear ();
		SelectableUI[] uis = FindObjectsOfType<SelectableUI> ();

		Vector2[] directions = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
		// find the nearest boxes to the left
		for (int i = 0; i < directions.Length; i++) {
			float closestDist = float.MaxValue;
			float perfectDist = float.MaxValue;
			float bestDot = 0;
			SelectableUI nearest = null;
			SelectableUI perfect = null;
			Vector2 nearestDir = Vector2.zero;
			Vector2 perfectDir = Vector2.zero;
			for (int j = 0; j < uis.Length; j++) {
				// work out direction
				Vector2 diff = uis [j].canvasCenter - canvasCenter;
				// are they similar
				float newDot = Vector2.Dot (diff.normalized, directions [i]);
				if (newDot > 0.8f) {
					Vector2 distVec = ComponentMultiply (diff, directions [i]);
					float newDist = distVec.sqrMagnitude;
					if (newDist < closestDist) {
						closestDist = newDist;
						bestDot = newDot;
						nearest = uis [j];
						nearestDir = diff.normalized;
					}
					// not the closest... but it's a perfect fit!
					else if (bestDot < newDot && newDot > 0.99) {
						if (newDist < perfectDist) {
							perfectDist = newDist;
							perfect = uis [j];
							perfectDir = diff.normalized;
						}
					}
				}
			}

			if (nearest != null) {
				AddConnection (nearest, nearestDir);
			}
			if (perfect != null) {
				AddConnection (perfect, perfectDir);
			}
		}
	}

	Vector2 ComponentMultiply (Vector2 v1, Vector2 v2)
	{
		return new Vector2 (v1.x * v2.x, v1.y * v2.y);
	}

	/// <summary>
	/// Adds a connection without duplicates
	/// </summary>
	/// <param name="ui">component that wants to connect</param>
	/// <param name="dir">direction from us to component</param>
	void AddConnection (SelectableUI ui, Vector2 dir)
	{
		for (int i = 0; i < connections.Count; i++) {
			if (connections [i].target == ui) {
				return;
			}
		}
		connections.Add (new UIConnection (dir, ui));
	}

	void MakeConnectionsDouble ()
	{
		for (int i = 0; i < connections.Count; i++) {
			connections [i].target.AddConnection (this, -connections [i].direction);
		}
	}

	#if UNITY_EDITOR
	[MenuItem ("ControllerUI/ConnectNav")]
	public static void CreateSelectableConnections ()
	{
		Debug.Log ("connecting...");
		SelectableUI[] uis = FindObjectsOfType<SelectableUI> ();
		for (int i = 0; i < uis.Length; i++) {
			uis [i].Init ();
		}

		for (int i = 0; i < uis.Length; i++) {
			uis [i].ConnectNearby ();
		}

		for (int i = 0; i < uis.Length; i++) {
			uis [i].MakeConnectionsDouble ();
		}

		Debug.Log ("connected");
	}
	#endif

	void OnDrawGizmos ()
	{
		Gizmos.color = Color.yellow;
		for (int i = 0; i < connections.Count; i++) {
			if (connections [i].target != null) {
				Gizmos.DrawLine (transform.position, connections [i].target.transform.position);
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
}
