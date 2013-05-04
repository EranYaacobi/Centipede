using System;
using UnityEditor;
using UnityEngine;

namespace TileMapEditorPackage.ContextMenuPlugins
{
	[Serializable]
	public class SolidRectangleContextMenuPlugin : ContextMenuPlugin
	{
		public SolidRectangleContextMenuPlugin()
		{
			// Setting default menu offset.
			MenuGridLocation = new Vector2(-0.5F, -3);
		}

		public override Boolean Interact(TileMapEditor Editor, Vector2 ContextMenuTilePosition)
		{
			return ShowButton(Editor, ContextMenuTilePosition, MenuGridLocation, "Solid Rectangle");
		}

		public override Boolean Action(TileMapEditor Editor, Vector2 ContextMenuTilePosition)
		{
			Editor.MarkedPositions.Clear();
			Editor.SelectedPositions.Clear();

			var TempStart = new Vector2(Mathf.Clamp(0, ContextMenuTilePosition.x, Editor.TileMap.Rows),
			                            Mathf.Clamp(0, ContextMenuTilePosition.y, Editor.TileMap.Columns));
			var CurrentTilePosition = Editor.GetCurrentTilePosition();
			var TempEnd = new Vector2(Mathf.Clamp(0, CurrentTilePosition.x, Editor.TileMap.Rows),
			                          Mathf.Clamp(0, CurrentTilePosition.y, Editor.TileMap.Columns));
			var RectangleStart = new Vector2(Mathf.Min(TempStart.x, TempEnd.x),
			                                 Mathf.Min(TempStart.y, TempEnd.y));
			var RectangleEnd = new Vector2(Mathf.Max(TempStart.x, TempEnd.x),
			                               Mathf.Max(TempStart.y, TempEnd.y));

			for (var Column = (Int32) RectangleStart.x; Column <= (Int32) RectangleEnd.x; Column++)
			{
				for (var Row = (Int32) RectangleStart.y; Row <= (Int32) RectangleEnd.y; Row++)
				{
					var TableTilePosition = new Vector2(Column, Row);
					if (!Editor.ContainsTile(TableTilePosition))
						Editor.AddMarkedSelectedPosition(TableTilePosition);
				}
			}

			if (MouseClicked(MouseButton.Left))
			{
				foreach (var SelectedPosition in Editor.SelectedPositions)
					Editor.Draw(SelectedPosition);

				Editor.MarkedPositions.Clear();
				Editor.SelectedPositions.Clear();

				return true;
			}

			return false;
		}
	}
}