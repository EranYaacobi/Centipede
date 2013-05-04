using System;
using UnityEngine;

namespace TileMapEditorPackage.ContextMenuPlugins
{
	[Serializable]
	public class FillAllContextMenuPlugin : ContextMenuPlugin
	{
		public FillAllContextMenuPlugin()
		{
			// Setting default menu offset.
			MenuGridLocation = new Vector2(-0.5F, -4);
		}

		public override Boolean Interact(TileMapEditor Editor, Vector2 ContextMenuTilePosition)
		{
			return ShowButton(Editor, ContextMenuTilePosition, MenuGridLocation, "Fill All");
		}

		public override Boolean Action(TileMapEditor Editor, Vector2 ContextMenuTilePosition)
		{
			for (var Column = 0; Column < Editor.TileMap.Columns; Column++)
			{
				for (var Row = 0; Row < Editor.TileMap.Rows; Row++)
				{
					var TableTilePosition = new Vector2(Column, Row);
					if (!Editor.ContainsTile(TableTilePosition))
						Editor.Draw(TableTilePosition);
				}
			}
			return true;
		}
	}
}