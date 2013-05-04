using System;
using System.Collections.Generic;
using System.Linq;
using TileMapEditorPackage.ContextMenuPlugins;
using UnityEngine;
using UnityEditor;
using TileMapEditorPackage;

/// <summary>
/// Provides an editor for the <see cref="ContextMenuPluginHost"/> component
/// This is not inside the TileMapeEditorPackage.ContextMenuPlugins, because in that case unity
/// fails to find this class.
/// </summary>
[CustomEditor(typeof(ContextMenuPluginHost))]
public class ContextMenuPluginHostEditor : Editor
{
	public override void OnInspectorGUI()
	{
		var ContextMenuPlugin = ((ContextMenuPluginHost)target).ContextMenuPlugin as ContextMenuPlugin;

		if (ContextMenuPlugin == null)
		{
			DrawDefaultInspector();
		}
		else
		{
			// Creating a serialized object, that will be used to update changes in the original class.
			var SerializedObject = new SerializedObject(ContextMenuPlugin);
			SerializedObject.Update();

			SerializedProperty Iterator = SerializedObject.GetIterator();
			for (bool EnterChildren = true; Iterator.NextVisible(EnterChildren); EnterChildren = false)
				EditorGUILayout.PropertyField(Iterator, true, new GUILayoutOption[0]);
			SerializedObject.ApplyModifiedProperties();
		}
	}
}