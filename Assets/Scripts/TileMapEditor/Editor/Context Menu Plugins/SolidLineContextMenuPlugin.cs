using System;
using TileMapEditorPackage.ContextMenuPlugins;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SolidLineContextMenuPlugin : ContextMenuPlugin
{
	public SolidLineContextMenuPlugin()
	{
		// Setting default menu offset.
		MenuGridLocation = new Vector2(0.5F, -3);
	}

	public override Boolean Interact(TileMapEditor Editor, Vector2 ContextMenuTilePosition)
	{
		return ShowButton(Editor, ContextMenuTilePosition, MenuGridLocation, "Solid Line");
	}

	public override Boolean Action(TileMapEditor Editor, Vector2 ContextMenuTilePosition)
	{
		Editor.MarkedPositions.Clear();
		Editor.SelectedPositions.Clear();

		var LineStart = new Vector2(Mathf.Clamp(0, ContextMenuTilePosition.x, Editor.TileMap.Rows),
									Mathf.Clamp(0, ContextMenuTilePosition.y, Editor.TileMap.Columns));
		var CurrentTilePosition = Editor.GetCurrentTilePosition();
		var LineEnd = new Vector2(Mathf.Clamp(0, CurrentTilePosition.x, Editor.TileMap.Rows),
								  Mathf.Clamp(0, CurrentTilePosition.y, Editor.TileMap.Columns));

		var Width = (Int32)(LineEnd.x - LineStart.x);
		var Heigth = (Int32)(LineEnd.y - LineStart.y);
		var Maximum = Mathf.Max(Mathf.Abs(Width), Math.Abs(Heigth));

		if ((Width != 0) || (Heigth != 0))
		{
			var ColumnStep = Width / (Single)Maximum;
			var RowStep = Heigth / (Single)Maximum;
			for (int i = 0; i <= Maximum; i++)
			{
				var Column = Mathf.RoundToInt(LineStart.x + i * ColumnStep);
				var Row = Mathf.RoundToInt(LineStart.y + i * RowStep);
				var TableTilePosition = new Vector2(Column, Row);
				if (!Editor.ContainsTile(TableTilePosition))
					Editor.AddMarkedSelectedPosition(TableTilePosition);
			}
		}
		else
		{
			if (!Editor.ContainsTile(LineStart))
				Editor.AddMarkedSelectedPosition(LineStart);
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