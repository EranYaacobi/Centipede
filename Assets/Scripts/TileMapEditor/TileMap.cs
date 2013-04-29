using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TileMapEditorPackage
{
	/// <summary>
	/// Provides a component for tile mapping.
	/// </summary>
	public class TileMap : MonoBehaviour
	{
		/// <summary>
		/// The tiles sets of which the map can consist.
		/// </summary>
		public TileSet[] TileSets;

		/// <summary>
		/// Gets or sets the number of columns of tiles.
		/// </summary>
		public Int32 Columns;

		/// <summary>
		/// Gets or sets the number of rows of tiles.
		/// </summary>
		public Int32 Rows;

		/// <summary>
		/// Gets or sets the value of the tile width.
		/// </summary>
		public Single TileWidth = 1;

		/// <summary>
		/// Gets or sets the value of the tile height.
		/// </summary>
		public Single TileHeight = 1;

		/// <summary>
		/// A list of marked positions that should be drawn.
		/// </summary>
		[HideInInspector]
		public List<Vector3> MarkedPositions;

#if UNITY_EDITOR

		/// <summary>
		/// Data stored for the editor plugins.
		/// This is stored here as an Object, as ContextMenuPlugin is not available here.
		/// </summary>
		[HideInInspector]
		public Object TileMapEditorPluginsData;
#endif

		/// <summary>
		/// The currently used tile-set.
		/// </summary>
		[HideInInspector]
		public TileSet CurrentTileSet;

		/// <summary>
		/// Initializes a new instance of the <see cref="TileMap"/> class.
		/// </summary>
		public TileMap()
		{
			Columns = 20;
			Rows = 10;
		}

		/// <summary>
		/// When the game object is selected this will draw the grid
		/// </summary>
		/// <remarks>Only called when in the Unity editor.</remarks>
		private void OnDrawGizmosSelected()
		{
			// Most of the drawing here is commented out, because gizmos appear before GUI, which makes
			// the context menu items appear behind the grid.
			// Therefore, it was moved to TileMapEditor.
			/*
			// Store map width, height and position
			var MapWidth = Columns*TileWidth;
			var MapHeight = Rows*TileHeight;
			var MapPosition = transform.position;

			// Draw layer border
			Gizmos.color = Color.white;
			Gizmos.DrawLine(MapPosition, MapPosition + new Vector3(MapWidth, 0, 0));
			Gizmos.DrawLine(MapPosition, MapPosition + new Vector3(0, MapHeight, 0));
			Gizmos.DrawLine(MapPosition + new Vector3(MapWidth, 0, 0), MapPosition + new Vector3(MapWidth, MapHeight, 0));
			Gizmos.DrawLine(MapPosition + new Vector3(0, MapHeight, 0), MapPosition + new Vector3(MapWidth, MapHeight, 0));

			// draw tile cells
			Gizmos.color = Color.grey;
			for (Single i = 1; i < Columns; i++)
			{
				Gizmos.DrawLine(MapPosition + new Vector3(i*TileWidth, 0, 0), MapPosition + new Vector3(i*TileWidth, MapHeight, 0));
			}

			for (Single i = 1; i < Rows; i++)
			{
				Gizmos.DrawLine(MapPosition + new Vector3(0, i*TileHeight, 0), MapPosition + new Vector3(MapWidth, i*TileHeight, 0));
			}

			// Draw marker position
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(MarkerPosition, new Vector3(TileWidth, TileHeight, 1)*1.1f);*/

			// Drawing marked positions here, because drawing spheres in Gizmos looks better than drawing circles
			// in Handles (TileMapEditor).
			Gizmos.color = Color.cyan;
			foreach (var MarkedPosition in MarkedPositions)
				Gizmos.DrawSphere(MarkedPosition, Mathf.Min(TileWidth, TileHeight)/3);
		}

		[Serializable]
		public class TileSet
		{
			/// <summary>
			/// The name of the tile set.
			/// Shown in the editor when choosing the current tile set.
			/// </summary>
			public String Name;

			/// <summary>
			/// The models used to build the tiles.
			/// </summary>
			public GameObject[] Tiles;

			/// <summary>
			/// The tiles material.
			/// If none, then each tile's material is used for itself only,
			/// and there is no default tile.
			/// </summary>
			public Material Material;
		}
	}
}