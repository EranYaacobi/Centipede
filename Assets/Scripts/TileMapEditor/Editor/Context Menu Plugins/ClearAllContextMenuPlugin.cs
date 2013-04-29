using System;
using TileMapEditorPackage.ContextMenuPlugins;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ClearAllContextMenuPlugin : ContextMenuPlugin
{
	public ClearAllContextMenuPlugin()
	{
		// Setting default menu offset.
		MenuGridLocation = new Vector2(0.5F, -4);
	}

	public override Boolean Interact(TileMapEditor Editor, Vector2 ContextMenuTilePosition)
	{
		return ShowButton(Editor, ContextMenuTilePosition, "Clear");
	}

	public override Boolean Action(TileMapEditor Editor, Vector2 ContextMenuTilePosition)
	{
		for (var i = Editor.TilesParent.childCount - 1; i >= 0; i--)
			UnityEngine.Object.DestroyImmediate(Editor.TilesParent.GetChild(i).gameObject);

		return true;
	}
}
