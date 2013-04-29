using System;
using System.Collections.Generic;
using System.Linq;
using TileMapEditorPackage.ContextMenuPlugins;
using UnityEngine;
using UnityEditor;
using TileMapEditorPackage;

/// <summary>
/// Provides an editor for the <see cref="TileMap"/> component
/// This is not inside the TileMapeEditorPackage, because in that case unity
/// fails to find this class.
/// </summary>
[CustomEditor(typeof (TileMap))]
public class TileMapEditor : Editor
{
	private const String TileNameFormat = "Tile_{0}_{1}";
	private const String TilesParentName = "Tiles";

	private const Single MarkerScale = 1.1F;

	/// <summary>
	/// The tile map being edited.
	/// </summary>
	public TileMap TileMap { get; private set; }

	/// <summary>
	/// The game object that contains all tiles as children.
	/// </summary>
	public Transform TilesParent { get; private set; }

	/// <summary>
	/// A list of the currently selected position.
	/// </summary>
	public List<Vector2> SelectedPositions;

	/// <summary>
	/// A list of marked positions that should be drawn.
	/// </summary>
	public List<Vector3> MarkedPositions
	{
		get { return TileMap.MarkedPositions; }
		set { TileMap.MarkedPositions = value; }
	}

	/// <summary>
	/// The state of the editor.
	/// This is used to prevent performing several actions in paralel.
	/// </summary>
	private TileMapEditorState EditorState;

	/// <summary>
	/// The previous tool.
	/// Used in order to restore the tool used before editing the map.
	/// </summary>
	private Tool PreviousTool;

	/// <summary>
	/// Holds the location of the mouse hit location
	/// </summary>
	private Vector3 MouseHitPosition;

	/// <summary>
	/// Used by editor components or game logic to indicate a tile location.
	/// </summary>
	/// <remarks>This will be hidden from the inspector window. See <see cref="HideInInspector"/></remarks>
	private Vector3 MarkerPosition;

	/// <summary>
	/// The position of the context menu.
	/// </summary>
	private Vector2 ContextMenuPosition;

	/// <summary>
	/// The currently active context menu.
	/// </summary>
	private ContextMenuPlugin ActiveContextMenu;

	/// <summary>
	/// The default tile for drawing.
	/// A value of null for this indicates that the default tile is a cube.
	/// </summary>
	public GameObject DefaultTile;

	/// <summary>
	/// When the <see cref="GameObject"/> is selected set the current tool to the view tool.
	/// </summary>
	private void OnEnable()
	{
		// Get a reference to the tile map component.
		TileMap = (TileMap) target;
		if ((TileMap.CurrentTileSet == null) ||
		    (!TileMap.TileSets.Select(TileSet => TileSet.Name).Contains(TileMap.CurrentTileSet.Name)))
			TileMap.CurrentTileSet = TileMap.TileSets[0];

		TilesParent = TileMap.transform.parent.FindChild(TilesParentName);
		if (TilesParent == null)
		{
			var TilesParentGameObject = new GameObject(TilesParentName);
			TilesParent = TilesParentGameObject.transform;
			TilesParent.parent = TileMap.transform.parent;
			TilesParent.localPosition = Vector3.zero;
		}

		SelectedPositions = new List<Vector2>();
		MarkedPositions = new List<Vector3>();

		PreviousTool = Tools.current;
		Tools.current = Tool.View;
		Tools.viewTool = ViewTool.FPS;
	}

	private void OnDisable()
	{
		Tools.current = PreviousTool;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		// Creating default plugin-data.
		if (TileMap.TileMapEditorPluginsData == null)
			TileMap.TileMapEditorPluginsData = CreateDefaultPlugins();

		// Creating a serialized object, that will be used to update changes in the original class.
		var SerializedObject = new SerializedObject(TileMap.TileMapEditorPluginsData);
		SerializedObject.Update();

		GUILayout.Space(10);
		var ContextMenuPluginsProperty = SerializedObject.FindProperty("ContextMenuPlugins");
		EditorGUILayout.PropertyField(ContextMenuPluginsProperty, true);

		TileMapEditorPluginsData TempPluginsData = null;
		if (GUILayout.Button("Reset Plugins", GUILayout.ExpandWidth(false)))
		{
			TempPluginsData = TileMap.TileMapEditorPluginsData as TileMapEditorPluginsData;
			TileMap.TileMapEditorPluginsData = CreateDefaultPlugins();
		}

		GUILayout.Space(10);
		EditorGUILayout.LabelField("Current Tileset", TileMap.CurrentTileSet.Name);

		SerializedObject.ApplyModifiedProperties();

		// Cleaning up old plugins data, to avoid Unity's info of a leak being cleaned-up.
		if (TempPluginsData != null)
			DestroyImmediate(TempPluginsData);
	}

	private TileMapEditorPluginsData CreateDefaultPlugins()
	{
		var TileMapEditorPluginsData = CreateInstance<TileMapEditorPluginsData>();
		TileMapEditorPluginsData.ContextMenuPlugins.Add(CreateInstance<ClearAllContextMenuPlugin>());
		TileMapEditorPluginsData.ContextMenuPlugins.Add(CreateInstance<RectangleContextMenuPlugin>());
		TileMapEditorPluginsData.ContextMenuPlugins.Add(CreateInstance<SolidRectangleContextMenuPlugin>());
		/*TileMapEditorPluginsData.ContextMenuPlugins.Add(new RectangleContextMenuPlugin());
		TileMapEditorPluginsData.ContextMenuPlugins.Add(new ClearAllContextMenuPlugin());
		TileMapEditorPluginsData.ContextMenuPlugins.Add(new SolidRectangleContextMenuPlugin());*/

		//AssetDatabase.AddObjectToAsset(TileMapEditorPluginsData, "TileMapEditorPluginsData");

		return TileMapEditorPluginsData;
	}

	/// <summary>
	/// Lets the Editor handle an event in the scene view.
	/// </summary>
	private void OnSceneGUI()
	{
		Tools.current = Tool.View;
		Tools.viewTool = ViewTool.Pan;

		// If UpdateHitPosition return true we should update the scene views so that the marker will update in real time.
		if (UpdateHitPosition())
			SceneView.RepaintAll();

		RecalculateMarkerPosition();

		HandleMouseInput();

		DrawMenu();
	}

	/// <summary>
	/// Calculates the position of the mouse over the tile map in local space coordinates.
	/// </summary>
	/// <returns>Returns true if the mouse is over the tile map.</returns>
	private Boolean UpdateHitPosition()
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
			Hit = MouseRay.origin + (MouseRay.direction.normalized*Distance);
		}

		// Convert the hit location from world space to local space
		var LocalHitPosition = TileMap.transform.InverseTransformPoint(Hit);

		// If the LocalHitPosition is different then the current mouse hit location set the 
		// new mouse hit location and return true indicating a successful hit test
		if (LocalHitPosition != MouseHitPosition)
		{
			MouseHitPosition = LocalHitPosition;
			EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.Arrow);
			return true;
		}

		// Return false if the hit test failed
		return false;
	}

	/// <summary>
	/// Recalculates the position of the marker based on the location of the mouse pointer.
	/// </summary>
	private void RecalculateMarkerPosition()
	{
		var TableTilePosition = GetCurrentTilePosition();

		// Store the world tile position.
		var WorldTilePosition = new Vector3(TableTilePosition.x*TileMap.TileWidth, TableTilePosition.y*TileMap.TileHeight, 0);

		// Set the TileMap.MarkerPosition value
		MarkerPosition = TileMap.transform.position + new Vector3(WorldTilePosition.x + (TileMap.TileWidth/2),
		                                                          WorldTilePosition.y + (TileMap.TileHeight/2), 0);
	}

	/// <summary>
	/// Gets the current tile location of the mouse.
	/// </summary>
	/// <returns></returns>
	public Vector2 GetCurrentTilePosition()
	{
		// Calculate column and row location from the mouse hit position.
		var TableTilePosition = new Vector2(MouseHitPosition.x/TileMap.TileWidth, MouseHitPosition.y/TileMap.TileHeight);

		// Round the numbers to the nearest whole number using 5 decimal place precision
		TableTilePosition = new Vector2((Int32) Math.Round(TableTilePosition.x, 5, MidpointRounding.ToEven),
		                                (Int32) Math.Round(TableTilePosition.y, 5, MidpointRounding.ToEven));

		// Clamp the TableTilePosition.
		TableTilePosition.x = Mathf.Clamp(TableTilePosition.x, 0, TileMap.Columns - 1);
		TableTilePosition.y = Mathf.Clamp(TableTilePosition.y, 0, TileMap.Rows - 1);

		// Return the TableTilePosition.
		return TableTilePosition;
	}

	/// <summary>
	/// Gets the current tile location of the mouse.
	/// </summary>
	/// <returns></returns>
	public Vector3 TilePositionToWorldPosition(Vector2 TableTilePosition)
	{
		var LocalTilePosition = new Vector3((TableTilePosition.x*TileMap.TileWidth) + (TileMap.TileWidth/2),
		                                    (TableTilePosition.y*TileMap.TileHeight) + (TileMap.TileHeight/2));

		return TileMap.transform.position + LocalTilePosition;
	}

	/// <summary>
	/// Gets the tile on which the mouse is currently pointing, or null if the is no such tile.
	/// </summary>
	public GameObject GetCurrentTile()
	{
		var TableTilePosition = GetCurrentTilePosition();

		// Get the tile, using the table tile position, via the tile's name.
		var CurrentTileTransform =
			TilesParent.FindChild(String.Format(TileNameFormat, TableTilePosition.x, TableTilePosition.y));

		if (CurrentTileTransform == null)
			return null;

		return CurrentTileTransform.gameObject;
	}

	/// <summary>
	/// Returns whether the given position contains a tile
	/// </summary>
	/// <returns></returns>
	public Boolean ContainsTile(Vector2 TableTilePosition)
	{
		return (TilesParent.FindChild(String.Format(TileNameFormat, TableTilePosition.x, TableTilePosition.y)) != null);
	}

	private void HandleMouseInput()
	{
		// Get a reference to the current event
		var CurrentEditorEvent = Event.current;

		// Checking if this event is an event that we use.
		if ((CurrentEditorEvent.type != EventType.MouseDown) &&
		    (CurrentEditorEvent.type != EventType.MouseUp) &&
		    (CurrentEditorEvent.type != EventType.MouseDrag))
			return;

		// If the context menu is shown, then we shouldn't handle the event.
		if (EditorState == TileMapEditorState.ContextMenu)
			return;

		// If the mouse is not positioned over the layer, there is no need to handle the event.
		if (!IsMouseOnLayer())
			return;

		var CurrentTilePosition = GetCurrentTilePosition();
		var CurrentTile = GetCurrentTile();

		switch ((MouseButton)CurrentEditorEvent.button)
		{
			case MouseButton.Left:
				if (EditorState == TileMapEditorState.Erasing)
					break;

				if ((CurrentEditorEvent.type == EventType.MouseDown) || (CurrentEditorEvent.type == EventType.MouseDrag))
				{
					EditorState = TileMapEditorState.Drawing;
					if ((CurrentTile == null) && (!SelectedPositions.Contains(CurrentTilePosition)))
						AddMarkedSelectedPosition(CurrentTilePosition);
				}
				else if (CurrentEditorEvent.type == EventType.MouseUp)
				{
					foreach (var SelectedPosition in SelectedPositions)
						Draw(SelectedPosition);

					SelectedPositions.Clear();
					MarkedPositions.Clear();
					EditorState = TileMapEditorState.None;
				}
				break;
			case MouseButton.Right:
				Tools.current = Tool.View;
				Tools.viewTool = ViewTool.FPS;
				if (EditorState == TileMapEditorState.Drawing)
					break;
				if (CurrentTile == null)
				{
					if ((CurrentEditorEvent.type == EventType.MouseDown))
					{
						EditorState = TileMapEditorState.ContextMenu;
						ContextMenuPosition = CurrentTilePosition;
					}
					// Not checking against MouseUp, because there is a problem with the right button for some reason.
					else if (CurrentEditorEvent.type != EventType.MouseDrag)
					{
						EditorState = TileMapEditorState.None;
					}
				}
				else
				{
					if ((CurrentEditorEvent.type == EventType.MouseDown) || (CurrentEditorEvent.type == EventType.MouseDrag))
					{
						EditorState = TileMapEditorState.Erasing;
						Erase(CurrentTile);
					}
					else
					{
						// Not checking against MouseUp, because there is a problem with the right button for some reason.
						EditorState = TileMapEditorState.None;
					}
				}
				break;
		}

		CurrentEditorEvent.Use();
	}

	/// <summary>
	/// Returns true or false depending if the mouse is positioned over the tile map.
	/// </summary>
	/// <returns>Will return true if the mouse is positioned over the tile map.</returns>
	private bool IsMouseOnLayer()
	{
		// return true or false depending if the mouse is positioned over the map
		return MouseHitPosition.x > 0 && MouseHitPosition.x < (TileMap.Columns*TileMap.TileWidth) &&
		       MouseHitPosition.y > 0 && MouseHitPosition.y < (TileMap.Rows*TileMap.TileHeight);
	}

	/// <summary>
	/// Add the given tile-position to the selected positions and to the marked position.
	/// </summary>
	/// <param name="TableTilePosition"></param>
	public void AddMarkedSelectedPosition(Vector2 TableTilePosition)
	{
		SelectedPositions.Add(TableTilePosition);
		MarkedPositions.Add(TilePositionToWorldPosition(TableTilePosition));
	}

	/// <summary>
	/// Draws a tile at the position on which the mouse is pointing.
	/// </summary>
	public void Draw(Vector2 TableTilePosition, GameObject TileModel = null)
	{
		GameObject NewTile;

		// Checking if a tile already exist here.
		if (ContainsTile(TableTilePosition))
			return;

		if (TileModel == null)
		{
			if (DefaultTile != null)
			{
				NewTile = Instantiate(DefaultTile) as GameObject;
			}
			else
			{
				// If there is no material in the current tile-set, then we can't create a default tile.
				if (TileMap.CurrentTileSet.Material == null)
					return;

				NewTile = GameObject.CreatePrimitive(PrimitiveType.Cube);
			}
		}
		else
		{
			NewTile = Instantiate(TileModel) as GameObject;
		}

		// Set the tile's position and rotation.
		if (NewTile != null)
		{
			// Set the tile's position on the tile map
			NewTile.transform.position = TilePositionToWorldPosition(TableTilePosition);
			NewTile.transform.rotation = TileMap.transform.rotation;

			// Scale the tile to the tile size defined by the TileMap.TileWidth and TileMap.TileHeight fields 
			NewTile.transform.localScale = new Vector3(TileMap.TileWidth, TileMap.TileHeight, 1);

			// Set the tile's parent to the game object for organizational purposes
			NewTile.transform.parent = TilesParent;

			// Give the tile a name that represents it's location within the tile map (this can be used later to retrieve it).
			NewTile.name = String.Format(TileNameFormat, TableTilePosition.x, TableTilePosition.y);

			// Set the tile's material.
			NewTile.renderer.enabled = true;
			NewTile.renderer.sharedMaterial = TileMap.CurrentTileSet.Material;
			NewTile.renderer.useLightProbes = true;
		}
	}

	/// <summary>
	/// Erases the given tile.
	/// </summary>
	private void Erase(GameObject CurrentTile)
	{
		DestroyImmediate(CurrentTile);
	}

	private void DrawMenu()
	{
		Handles.BeginGUI();

		var TileWidth = WorldToGUIPoint(new Vector3(TileMap.TileWidth, 0)) - WorldToGUIPoint(Vector3.zero);
		var TileHeight = WorldToGUIPoint(new Vector3(0, TileMap.TileHeight)) - WorldToGUIPoint(Vector3.zero);
		var MapWidth = TileMap.Columns*TileWidth;
		var MapHeight = TileMap.Rows*TileHeight;
		var MapPosition = WorldToGUIPoint(TileMap.transform.position);

		// Drawing Border.
		Handles.color = Color.white;
		Handles.DrawLine(MapPosition, MapPosition + MapWidth);
		Handles.DrawLine(MapPosition, MapPosition + MapHeight);
		Handles.DrawLine(MapPosition + MapWidth, MapPosition + MapWidth + MapHeight);
		Handles.DrawLine(MapPosition + MapHeight, MapPosition + MapWidth + MapHeight);

		// Drawing grid.
		Handles.color = Color.gray;
		for (Single i = 1; i < TileMap.Columns; i++)
			Handles.DrawLine(MapPosition + i*TileWidth, MapPosition + i*TileWidth + MapHeight);

		for (Single i = 1; i < TileMap.Rows; i++)
			Handles.DrawLine(MapPosition + i*TileHeight, MapPosition + MapWidth + i*TileHeight);

		// Drawing help text.
		GUI.Label(new Rect(10, Screen.height - 90, 75, 20), "LMB: Draw");
		GUI.Label(new Rect(10, Screen.height - 105, 150, 20), "RMB: ContextMenu\\Erase");

		// Applying context menu.
		if (EditorState == TileMapEditorState.ContextMenu)
		{
			if (ActiveContextMenu == null)
			{
				// Enumerating context menu plugins.
				foreach (var ContextMenuPlugin in ((TileMapEditorPluginsData) TileMap.TileMapEditorPluginsData).ContextMenuPlugins)
				{
					if (ContextMenuPlugin.Interact(this, ContextMenuPosition))
					{
						ActiveContextMenu = ContextMenuPlugin;
						break;
					}
				}
			}

			if (ActiveContextMenu != null)
			{
				if (ActiveContextMenu.Action(this, ContextMenuPosition))
				{
					ActiveContextMenu = null;
					EditorState = TileMapEditorState.None;
				}

				// Get a reference to the current event
				var CurrentEditorEvent = Event.current;
				if ((CurrentEditorEvent.isKey) && (CurrentEditorEvent.keyCode == KeyCode.Escape))
				{
					EditorState = TileMapEditorState.None;
					SelectedPositions.Clear();
					MarkedPositions.Clear();
					CurrentEditorEvent.Use();
				}
			}
			else
			{
				// Get a reference to the current event
				var CurrentEditorEvent = Event.current;

				// Checking if the left mouse key was pressed not on the menu, or if the Esc key was pressed.
				if (((CurrentEditorEvent.type == EventType.MouseUp) && (CurrentEditorEvent.button == 0)) ||
				    ((CurrentEditorEvent.isKey) && (CurrentEditorEvent.keyCode == KeyCode.Escape)))
				{
					EditorState = TileMapEditorState.None;
					CurrentEditorEvent.Use();
				}
				else if ((CurrentEditorEvent.type == EventType.MouseDown) && (IsMouseOnLayer()))
				{
					ContextMenuPosition = GetCurrentTilePosition();
				}
			}
		}

		// Draw marker position
		Handles.color = Color.red;
		var GUIMarkerPosition = WorldToGUIPoint(MarkerPosition);
		Handles.DrawPolyLine(GUIMarkerPosition - (TileWidth + TileHeight)/2*MarkerScale,
		                     GUIMarkerPosition - (TileWidth - TileHeight)/2*MarkerScale,
		                     GUIMarkerPosition + (TileWidth + TileHeight)/2*MarkerScale,
		                     GUIMarkerPosition + (TileWidth - TileHeight)/2*MarkerScale,
		                     GUIMarkerPosition - (TileWidth + TileHeight)/2*MarkerScale);

		// This code is commented-out, because drawing the marked positions using Gizmo functions (done in TileMap) looks better.
		// Draw marked positions.
		/*Handles.color = Color.cyan;
		foreach (var MarkedPosition in MarkedPositions)
			Handles.DrawSolidDisc(WorldToGUIPoint(MarkedPosition), Vector3.Cross(TileMap.transform.up, TileMap.transform.right), Mathf.Min(TileWidth.magnitude, TileHeight.magnitude) / 3);*/

		Handles.EndGUI();
	}

	private Vector3 WorldToGUIPoint(Vector3 WorldPoint)
	{
		var Temp = HandleUtility.WorldToGUIPoint(WorldPoint);
		return new Vector3(Temp.x, Temp.y);
	}
}