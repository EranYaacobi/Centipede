using System;
using UnityEditor;
using UnityEngine;

namespace TileMapEditorPackage.ContextMenuPlugins
{
	[Serializable]
	public abstract class ContextMenuPlugin : ScriptableObject
	{
		/* TODO: Find a way in Unity to edit the public members of this script in the inspector.
		 * So far you have managed to enable editing the members, by removing the inheritance from ScriptableObject,
		 * but the down-side is that the name of the script is not shown (which is too important to ignore).
		 * Moreover, you've failed to make a custom-editor for this (though it seems that (though not tested) it is
		 * possible to create a custom-editor for each derived context-menu).
		 */

		protected const Single ButtonMargin = 3F;

		/// <summary>
		/// The size of the button created by the context-menu plugin (if it has one).
		/// </summary>
		protected Vector2 ButtonSize = new Vector2(100, 20);

		/// <summary>
		/// The menu grid-location from the center of the context menu.
		/// </summary>
		public Vector2 MenuGridLocation;

		/// <summary>
		/// Interacts with the user for input.
		/// Returns whether an action should be performed.
		/// Notice that even if true is returned, it cannot be assumed that Action will be called afterwards.
		/// </summary>
		/// <returns></returns>
		public abstract Boolean Interact(TileMapEditor Editor, Vector2 ContextMenuTilePosition);

		/// <summary>
		/// Performs actions.
		/// Returns whether the action was finished.
		/// If false is returned, this function will be called again, until returning true.
		/// </summary>
		/// <returns></returns>
		public abstract Boolean Action(TileMapEditor Editor, Vector2 ContextMenuTilePosition);

		/// <summary>
		/// Utility function that checks whether a specific mouse button was clicked.
		/// Also removes that event if this is the case.
		/// </summary>
		/// <param name="MouseButton"></param>
		/// <returns></returns>
		protected Boolean MouseClicked(MouseButton MouseButton)
		{
			// Get a reference to the current event
			var CurrentEditorEvent = Event.current;

			if ((CurrentEditorEvent.type == EventType.MouseUp) && (CurrentEditorEvent.button == (Int32)MouseButton))
			{
				CurrentEditorEvent.Use();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Utility function that returns the actual location of a button, from it's grid location.
		/// A grid cell is at the size of ButtonSize.
		/// </summary>
		/// <param name="GridLocation"></param>
		/// <returns></returns>
		protected Vector2 PositionFromGridLocation(Vector2 GridLocation)
		{
			return new Vector2(GridLocation.x * (ButtonSize.x + ButtonMargin),
							   GridLocation.y * (ButtonSize.y + ButtonMargin)) - ButtonSize / 2;
		}

		/// <summary>
		/// Utility function that shows a button with the given label.
		/// </summary>
		/// <param name="Editor"></param>
		/// <param name="ContextMenuTilePosition"></param>
		/// <param name="Label"></param>
		/// <returns></returns>
		protected Boolean ShowButton(TileMapEditor Editor, Vector2 ContextMenuTilePosition, String Label)
		{
			var ContextMenuScreenPosition = HandleUtility.WorldToGUIPoint(Editor.TilePositionToWorldPosition(ContextMenuTilePosition));
			var ButtonScreenPosition = PositionFromGridLocation(MenuGridLocation) + ContextMenuScreenPosition;

			return (GUI.Button(new Rect(ButtonScreenPosition.x, ButtonScreenPosition.y, ButtonSize.x, ButtonSize.y), Label));
		}
	}
}