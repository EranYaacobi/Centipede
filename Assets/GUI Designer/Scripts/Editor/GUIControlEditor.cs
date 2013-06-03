using System;
using System.Linq;
using System.Collections.Generic;
using GUIDesigner.Scripts.Controls;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GUIControl))]
public class GUIControlEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUIUtility.LookLikeInspector();

		var Control = (GUIControl)target;
		this.serializedObject.Update();

		SerializedProperty Iterator = serializedObject.GetIterator();
		for (Boolean EnterChildren = true; Iterator.NextVisible(EnterChildren); EnterChildren = false)
		{
			if (!Control.HiddenMembers.Contains(Iterator.name))
				EditorGUILayout.PropertyField(Iterator, true, new GUILayoutOption[0]);
		}

		foreach (var EventData in Control.InspectorEventsData)
			EventData.HandlerName = EditorGUILayout.TextField(EventData.EventName, EventData.HandlerName);

		this.serializedObject.ApplyModifiedProperties();
	}
}