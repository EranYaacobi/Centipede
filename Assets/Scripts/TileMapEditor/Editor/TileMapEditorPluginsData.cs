using System;
using System.Collections.Generic;
using TileMapEditorPackage.ContextMenuPlugins;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This class sole purpose, is to store persistent plugin-data for TileMapEditor, in a manner that
/// enabling chaning that data in the inspector.
/// 
/// It is basically an editor\inspector hack.
/// </summary>
[Serializable]
public class TileMapEditorPluginsData : ScriptableObject
{
	public List<ContextMenuPlugin> ContextMenuPlugins = new List<ContextMenuPlugin>();

	void OnDestroy()
	{
		// Cleaning up the plugins, to avoid Unity's info of leaks being cleaned-up.
		/*foreach (var ContextMenuPlugin in ContextMenuPlugins)
			DestroyImmediate(ContextMenuPlugin);*/
	}
}