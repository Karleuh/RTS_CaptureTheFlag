using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Unit), true)]
public class RangeViewer : Editor
{
	Unit unit;
	Formation formation;
	BasicUnit basicUnit;

	private void OnEnable()
	{
		this.unit = this.target as Unit;
		this.formation = this.target as Formation;
		this.basicUnit = this.target as BasicUnit;
	}

	protected virtual void OnSceneGUI()
	{
		Handles.color = Color.red;
		Handles.CircleHandleCap(0, this.unit.transform.position, Quaternion.LookRotation(Vector3.up), this.unit.MinRange, EventType.Repaint);

		Handles.color = Color.green;
		Handles.CircleHandleCap(0, this.unit.transform.position, Quaternion.LookRotation(Vector3.up), this.unit.MaxRange, EventType.Repaint);

		Handles.color = Color.blue;
		Handles.CircleHandleCap(0, this.unit.transform.position, Quaternion.LookRotation(Vector3.up), this.unit.LineOfSight, EventType.Repaint);
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (formation != null && GUILayout.Button("Debug Path"))
			this.formation.DebugPath();

		if (basicUnit != null && GUILayout.Button("Debug Path"))
			this.basicUnit.DebugPath();

	}
}



