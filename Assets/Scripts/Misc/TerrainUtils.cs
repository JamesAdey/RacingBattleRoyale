using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TerrainUtils : MonoBehaviour
{

	public Terrain ter;
	public TerrainData data;

	public Vector3 lineStartPos = Vector3.zero;
	public Vector3 lineEndPos = Vector3.zero;
	public float startHeight = 0;
	public float endHeight = 0;
	public float lineWidth = 0;
	Vector3 bottomLeft;
	Vector3 topRight;

	// Use this for initialization
	void Start ()
	{
		ter = GetComponent<Terrain> ();
		data = ter.terrainData;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// work out bottom left
		bottomLeft = Vector3.Cross ((lineStartPos - lineEndPos).normalized, Vector3.up);
		bottomLeft *= lineWidth * 0.5f;
		// bottomLeft is still a direction, so offset it an turn into a position
		topRight = lineEndPos - bottomLeft;
		bottomLeft += lineStartPos;
	}

	public void PaintLine ()
	{

		float xScale = data.heightmapResolution / data.size.x;
		float zScale = data.heightmapResolution / data.size.z;

		bottomLeft -= ter.GetPosition ();
		topRight -= ter.GetPosition ();

		bottomLeft.x *= xScale;
		topRight.x *= xScale;
		bottomLeft.z *= zScale;
		topRight.z *= zScale;

		Vector3 start = lineStartPos - ter.GetPosition ();
		Vector3 end = lineEndPos - ter.GetPosition ();

		start.x *= xScale;
		end.x *= xScale;
		start.z *= zScale;
		end.z *= zScale;
	

		// round to nearest area
		int xBase = Mathf.RoundToInt (bottomLeft.x);
		int yBase = Mathf.RoundToInt (bottomLeft.z);

		// now work out top right
		int xTopRight = Mathf.RoundToInt (topRight.x);
		int yTopRight = Mathf.RoundToInt (topRight.z);
		int xAmount = Mathf.Abs (xTopRight - xBase);
		int yAmount = Mathf.Abs (yTopRight - yBase);

		float baseHeight = lineStartPos.y;
		float heightDiff = lineEndPos.y - lineStartPos.y;

		start.y = 0;
		end.y = 0;
		float[,] heights = new float[yAmount, xAmount];
		for (int x = 0; x < xAmount; x++) {
			for (int y = 0; y < yAmount; y++) {
				int xPos = x + xBase;
				int yPos = y + yBase;
				// check if in range
				float lerp = 0;
				float line = SqrDistToLine (start, end, new Vector3 (xPos, 0, yPos), out lerp);
				if (line < lineWidth * lineWidth * 0.25f) {
					//heights [y, x] = 60 / data.size.y;
					heights [y, x] = (lerp * heightDiff) + baseHeight;
					heights [y, x] /= data.size.y;
				} else {
					heights [y, x] = data.GetHeight (xPos, yPos) / data.size.y;
				}
			}
		}
		data.SetHeights (xBase, yBase, heights);
	}

	float SqrDistToLine (Vector3 startPos, Vector3 endPos, Vector3 point, out float distToStart)
	{
		Vector3 startPosDir = point - startPos;
		Vector3 lineDir = (endPos - startPos);
		float mul = Vector3.Dot (startPosDir, lineDir) / lineDir.sqrMagnitude;
		// store distance to start
		distToStart = mul;
		if (mul > 1 || mul < 0) {
			return float.PositiveInfinity;
		} else {
			Vector3 targetLinePos = startPos + mul * lineDir;
			return (targetLinePos - point).sqrMagnitude;
		}
	}

	public void OnDrawGizmos ()
	{
		Gizmos.DrawCube (lineStartPos, Vector3.one);
		Gizmos.DrawCube (lineEndPos, Vector3.one);
		Gizmos.DrawLine (lineStartPos, lineEndPos);
		Gizmos.color = Color.blue;
		Gizmos.DrawCube (bottomLeft, Vector3.one);
		Gizmos.color = Color.red;
		Gizmos.DrawCube (topRight, Vector3.one);
	}
}
