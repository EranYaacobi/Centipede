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
		return ShowButton(Editor, ContextMenuTilePosition, "Solid Line");
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
		var RatioDivisor = GreatestCommonDivisor(Mathf.Abs(Width), Mathf.Abs(Heigth));

		for (var Column = (Int32)LineStart.x; Column <= (Int32)LineEnd.x; Column += Width / RatioDivisor)
		{
			for (var Row = (Int32)LineStart.y; Row <= (Int32)LineEnd.y; Row += Heigth / RatioDivisor)
			{
				var TableTilePosition = new Vector2(Column, Row);
				if (!Editor.ContainsTile(TableTilePosition))
					Editor.AddMarkedSelectedPosition(new Vector2(Column, Row));
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

	private Int32 GreatestCommonDivisor(Int32 First, Int32 Second)
	{
		var Small = Mathf.Min(First, Second);
		var Large = Mathf.Max(First, Second);
		Int32 Remainder;

		do
		{
			Remainder = Large % Small;
			Large = Small;
			Small = Remainder;
		} while (Remainder > 0);

		return Large;
	}
}
