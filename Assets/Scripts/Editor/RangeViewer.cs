using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Unit), true)]
public class RangeViewer : Editor
{
	Unit unit;

	private void OnEnable()
	{
		this.unit = this.target as Unit;
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
}
