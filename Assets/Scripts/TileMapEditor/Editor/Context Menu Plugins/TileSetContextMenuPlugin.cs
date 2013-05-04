using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileMapEditorPackage.ContextMenuPlugins
{
	[Serializable]
	public class TileSetContextMenuPlugin : ContextMenuPlugin
	{
		private readonly Dictionary<GameObject, Texture2D> TileScreenShots = new Dictionary<GameObject, Texture2D>();

		private GameObject SelectedTile;

		private readonly Vector2 OriginalButtonSize;

		public TileSetContextMenuPlugin()
		{
			// Setting default menu offset.
			MenuGridLocation = new Vector2(0, 0);
			ButtonSize = new Vector2(ButtonSize.y, ButtonSize.y) * 2F;
			GridCellSize = ButtonSize;
			OriginalButtonSize = ButtonSize;
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
			foreach (var Texture in TileScreenShots.Values)
				DestroyImmediate(Texture);

			TileScreenShots.Clear();
		}

		public override Boolean Interact(TileMapEditor Editor, Vector2 ContextMenuTilePosition)
		{
			ButtonSize = OriginalButtonSize;
			var TilesCount = Editor.TileMap.CurrentTileSet.Tiles.Count;
			var ButtonGridLocationAngleStep = 0F;
			if (TilesCount > 1)
				ButtonGridLocationAngleStep = 360F / TilesCount;

			var CurrentAngle = 270 + ButtonGridLocationAngleStep / 2;
			ButtonSize /= Mathf.Sqrt(TilesCount / 2F);
			for (int i = 0; i < TilesCount; i++)
			{
				var ButtonGridLocation = new Vector2(Mathf.Cos(CurrentAngle * Mathf.Deg2Rad), Mathf.Sin(CurrentAngle * Mathf.Deg2Rad)); 
				if (ShowTileButton(Editor, Editor.TileMap.CurrentTileSet.Tiles[i], ContextMenuTilePosition, ButtonGridLocation))
					return true;
				CurrentAngle += ButtonGridLocationAngleStep;
			}

			return false;
		}

		private Boolean ShowTileButton(TileMapEditor Editor, GameObject Tile, Vector2 ContextMenuTilePosition, Vector2 ButtonGridLocation)
		{
			if (!TileScreenShots.ContainsKey(Tile))
				CaptureTile(Tile);

			if (TileScreenShots.ContainsKey(Tile))
			{
				if (ShowIconicButton(Editor, ContextMenuTilePosition, ButtonGridLocation, TileScreenShots[Tile]))
				{
					SelectedTile = Tile;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Captures the given tile to a 2d texture, by creating a camera and the tile in front of it,
		/// rendering the camera, and copying the rendered tile into a texture.
		/// </summary>
		/// <param name="Tile"></param>
		private void CaptureTile(GameObject Tile)
		{
			if (Tile == null)
				return;

			GameObject TilesCamera = null;
			GameObject NewTile = null;
			try
			{
				// TODO: Find a better way to ignore intruding objects.
				var Offset = new Vector3(2525, 2525, 2525);
				var CurrentRenderTexture = RenderTexture.active;
				TilesCamera = new GameObject();
				TilesCamera.name = "TilesSetContextMenuCamera";
				TilesCamera.AddComponent<Camera>();
				TilesCamera.transform.position = Offset;
				TilesCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
				TilesCamera.camera.orthographic = true;
				TilesCamera.camera.orthographicSize = 1;

				RenderTexture.active = TilesCamera.camera.targetTexture;

				NewTile = Instantiate(Tile) as GameObject;
				NewTile.name = "TileSetContextMenuTile";
				NewTile.transform.position = Offset + Vector3.forward;
				NewTile.transform.rotation = Quaternion.Euler(0, 0, 0);

				TilesCamera.camera.Render();

				var TileStart =
					TilesCamera.camera.WorldToScreenPoint(NewTile.renderer.bounds.center - NewTile.renderer.bounds.size / 2);
				var TileEnd = TilesCamera.camera.WorldToScreenPoint(NewTile.renderer.bounds.center + NewTile.renderer.bounds.size / 2);

				var TileTopLeft = new Vector2(Mathf.Min(TileStart.x, TileEnd.x), Mathf.Min(TileStart.y, TileEnd.y));
				var TileBottomRight = new Vector2(Mathf.Max(TileStart.x, TileEnd.x), Mathf.Max(TileStart.y, TileEnd.y));

				var Image = new Texture2D(Mathf.CeilToInt(TileBottomRight.x - TileTopLeft.x),
										  Mathf.CeilToInt(TileBottomRight.y - TileTopLeft.y));

				Image.ReadPixels(new Rect(TileTopLeft.x, TileTopLeft.y, Image.width, Image.height), 0, 0);
				Image.Apply();

				TileScreenShots.Add(Tile, Image);

				RenderTexture.active = CurrentRenderTexture;
			}
			finally
			{
				if (TilesCamera != null)
					DestroyImmediate(TilesCamera);
				if (NewTile != null)
					DestroyImmediate(NewTile);
			}
		}

		public override Boolean Action(TileMapEditor Editor, Vector2 ContextMenuTilePosition)
		{
			Editor.Draw(ContextMenuTilePosition, SelectedTile);
			return true;
		}
	}
}