using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(TerrainUtils))]
[CanEditMultipleObjects]
public class TerrainUtilsEditor : Editor
{
	SerializedProperty lineStartProp;
	SerializedProperty lineEndProp;
	SerializedProperty lineWidthProp;


	void OnEnable ()
	{
		lineStartProp = serializedObject.FindProperty ("lineStartPos");
		lineEndProp = serializedObject.FindProperty ("lineEndPos");
		lineWidthProp = serializedObject.FindProperty ("lineWidth");
	}

	public override void OnInspectorGUI ()
	{
		TerrainUtils utils = (TerrainUtils)target;
		serializedObject.Update ();
		if (utils == null) {
			return;
		}

		lineStartProp.vector3Value = EditorGUILayout.Vector3Field ("LineStartPos", lineStartProp.vector3Value);
		lineEndProp.vector3Value = EditorGUILayout.Vector3Field ("LineEndPos", lineEndProp.vector3Value);
		lineWidthProp.floatValue = EditorGUILayout.FloatField ("Line Width", lineWidthProp.floatValue);
		if (EditorGUILayout.ToggleLeft ("Paint Line", false)) {
			utils.PaintLine ();
		}
		serializedObject.ApplyModifiedProperties ();
	}
}
