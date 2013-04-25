using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Provides a editor for the <see cref="TileMap"/> component
/// </summary>
[CustomEditor(typeof (TileMap))]
public class TileMapEditor : Editor
{
	private const String TileNameFormat = "Tile_{0}_{1}";

	/// <summary>
	/// Holds the location of the mouse hit location
	/// </summary>
	private Vector3 mouseHitPos;

	private TileMap TileMap;

	/// <summary>
	/// The previous tool.
	/// </summary>
	private Tool PreviousTool;

	/// <summary>
	/// Lets the Editor handle an event in the scene view.
	/// </summary>
	private void OnSceneGUI()
	{
		// If UpdateHitPosition return true we should update the scene views so that the marker will update in real time
		if (this.UpdateHitPosition())
		{
			SceneView.RepaintAll();
		}

		// Calculate the location of the marker based on the location of the mouse
		this.RecalculateMarkerPosition();

		// get a reference to the current event
		Event current = Event.current;

		// if the mouse is positioned over the layer allow drawing actions to occur
		if (this.IsMouseOnLayer())
		{
			// if mouse down or mouse drag event occurred
			if (current.type == EventType.MouseDown || current.type == EventType.MouseDrag)
			{
				if (current.button == 1)
				{
					// if right mouse button is pressed then we erase blocks
					this.Erase();
					current.Use();
				}
				else if (current.button == 0)
				{
					// if left mouse button is pressed then we draw blocks
					this.Draw();
					current.Use();
				}
			}
		}

		// draw a UI tip in scene view informing user how to draw & erase tiles
		Handles.BeginGUI();
		GUI.Label(new Rect(10, Screen.height - 90, 100, 100), "LMB: Draw");
		GUI.Label(new Rect(10, Screen.height - 105, 100, 100), "RMB: Erase");
		if (GUI.Button(new Rect(10, Screen.height - 85, 100, 20), "Clear"))
		{
			for (int i = TileMap.transform.childCount - 1; i >= 0; i--)
			{
				var GameObjectChild = TileMap.transform.GetChild(i).gameObject;
				DestroyImmediate(GameObjectChild);
			}
		}
		Handles.EndGUI();
	}

	/// <summary>
	/// When the <see cref="GameObject"/> is selected set the current tool to the view tool.
	/// </summary>
	private void OnEnable()
	{
		// Get a reference to the tile map component.
		TileMap = (TileMap)this.target;

		PreviousTool = Tools.current;
		Tools.current = Tool.View;
		Tools.viewTool = ViewTool.FPS;
	}

	private void OnDisable()
	{
		Tools.current = PreviousTool;
	}



	/// <summary>
	/// Draws a block at the pre-calculated mouse hit position
	/// </summary>
	private void Draw()
	{
		// Calculate the position of the mouse over the tile layer
		var TableTilePosition = this.GetTilePositionFromMouseLocation();

		// Given the tile position check to see if a tile has already been created at that location
		var Cube = GameObject.Find(String.Format(TileNameFormat, TableTilePosition.x, TableTilePosition.y));

		// If there is already a tile present and it is not a child of the game object we can just exit.
		if (Cube != null && Cube.transform.parent != TileMap.transform)
		{
			return;
		}

		// If no game object was found we will create a cube
		if (Cube == null)
		{
			Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		}

		// Set the cubes position on the tile map
		var LocalTilePosition = new Vector3((TableTilePosition.x*TileMap.TileWidth) + (TileMap.TileWidth/2),
		                                    (TableTilePosition.y*TileMap.TileHeight) + (TileMap.TileHeight/2));
		Cube.transform.position = TileMap.transform.position + LocalTilePosition;

		// Scale the cube to the tile size defined by the TileMap.TileWidth and TileMap.TileHeight fields 
		Cube.transform.localScale = new Vector3(TileMap.TileWidth, TileMap.TileHeight, 1);

		// Set the cubes parent to the game object for organizational purposes
		Cube.transform.parent = TileMap.transform;

		// Give the cube a name that represents it's location within the tile map (this can be used later to retrieve it).
		Cube.name = String.Format(TileNameFormat, TableTilePosition.x, TableTilePosition.y);
	}

	/// <summary>
	/// Erases a block at the pre-calculated mouse hit position
	/// </summary>
	private void Erase()
	{
		// Calculate the position of the mouse over the tile layer
		var TableTilePosition = this.GetTilePositionFromMouseLocation();

		// Given the tile position check to see if a tile has already been created at that location
		var Cube = GameObject.Find(String.Format(TileNameFormat, TableTilePosition.x, TableTilePosition.y));

		// If a game object was found with the same name and it is a child we just destroy it immediately.
		if ((Cube != null) && (Cube.transform.parent == TileMap.transform))
			DestroyImmediate(Cube);
	}

	/// <summary>
	/// Calculates the location in tile coordinates (Column/Row) of the mouse position
	/// </summary>
	/// <returns>Returns a <see cref="Vector2"/> type representing the Column and Row where the mouse of positioned over.</returns>
	private Vector2 GetTilePositionFromMouseLocation()
	{
		// Calculate column and row location from mouse hit location
		var TableTilePosition = new Vector2(this.mouseHitPos.x/TileMap.TileWidth, this.mouseHitPos.y/TileMap.TileHeight);

		// Round the numbers to the nearest whole number using 5 decimal place precision
		TableTilePosition = new Vector2((Int32)Math.Round(TableTilePosition.x, 5, MidpointRounding.ToEven),
									(Int32)Math.Round(TableTilePosition.y, 5, MidpointRounding.ToEven));

		// Clamp the TableTilePosition.
		TableTilePosition.x = Mathf.Clamp(TableTilePosition.x, 0, TileMap.Columns - 1);
		TableTilePosition.y = Mathf.Clamp(TableTilePosition.y, 0, TileMap.Rows - 1);

		// Return the TableTilePosition.
		return TableTilePosition;
	}

	/// <summary>
	/// Returns true or false depending if the mouse is positioned over the tile map.
	/// </summary>
	/// <returns>Will return true if the mouse is positioned over the tile map.</returns>
	private bool IsMouseOnLayer()
	{
		// return true or false depending if the mouse is positioned over the map
		return this.mouseHitPos.x > 0 && this.mouseHitPos.x < (TileMap.Columns * TileMap.TileWidth) &&
			   this.mouseHitPos.y > 0 && this.mouseHitPos.y < (TileMap.Rows * TileMap.TileHeight);
	}

	/// <summary>
	/// Recalculates the position of the marker based on the location of the mouse pointer.
	/// </summary>
	private void RecalculateMarkerPosition()
	{
		// Store the tile location (Column/Row) based on the current location of the mouse pointer
		var TableTilePosition = this.GetTilePositionFromMouseLocation();

		// Store the world tile position.
		var WorldTilePosition = new Vector3(TableTilePosition.x * TileMap.TileWidth, TableTilePosition.y * TileMap.TileHeight, 0);

		// Set the TileMap.MarkerPosition value
		TileMap.MarkerPosition = TileMap.transform.position + new Vector3(WorldTilePosition.x + (TileMap.TileWidth/2), WorldTilePosition.y + (TileMap.TileHeight/2), 0);
	}

	/// <summary>
	/// Calculates the position of the mouse over the tile map in local space coordinates.
	/// </summary>
	/// <returns>Returns true if the mouse is over the tile map.</returns>
	private bool UpdateHitPosition()
	{
		// Build a plane object on the tile map.
		var Plane = new Plane(TileMap.transform.TransformDirection(Vector3.forward), TileMap.transform.position);

		// Create a ray from the mouse's position.
		var MouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		// Stores the hit location returned from the Raycast.
		var Hit = new Vector3();

		// Stores the distance from the hit location.
		Single Distance;

		// Cast a ray to determine what location it intersects with the plane
		if (Plane.Raycast(MouseRay, out Distance))
		{
			// The ray hits the plane so we calculate the hit location in world space.
			Hit = MouseRay.origin + (MouseRay.direction.normalized * Distance);
		}

		// Convert the hit location from world space to local space
		var LocalHitPosition = TileMap.transform.InverseTransformPoint(Hit);

		// If the LocalHitPosition is different then the current mouse hit location set the 
		// new mouse hit location and return true indicating a successful hit test
		if (LocalHitPosition != this.mouseHitPos)
		{
			this.mouseHitPos = LocalHitPosition;
			return true;
		}

		// Return false if the hit test failed
		return false;
	}
}
