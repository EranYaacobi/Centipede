using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GUIDesigner.Scripts.Controls
{
	public class GUIScrollView : GUIContainer
	{
		#region Customization
		
#if UNITY_EDITOR

		public override IEnumerable<string> HiddenMembers
		{
			get { return new List<String> { "GUIContent", "FontSize", "GUIStyle" }; }
		}

		protected override Boolean DisableInEditor
		{
			get { return false; }
		}

#endif

		public override Vector2 RelativeInnerAreaSize
		{
			get { return new Vector2(Mathf.Max(1, ScrollableRelativeAreaSize.x), Mathf.Max(1, ScrollableRelativeAreaSize.y)); }
		}

		public override Vector2 RelativeInnerAreaOffset
		{
			get { return RelativeScrollPosition; }
		}

		#endregion

		#region Variables

		/// <summary>
		/// The scrollable relative area of the control.
		/// </summary>
		public Vector2 ScrollableRelativeAreaSize = Vector2.zero;

		/// <summary>
		/// Indicates whether the GUIArea's Horizontl scrollbar should always be shown.
		/// </summary>
		public Boolean AlwaysShowHorizontlScrollbar;

		/// <summary>
		/// Indicates whether the GUIArea's Vertical scrollbar should always be shown.
		/// </summary>
		public Boolean AlwaysShowVerticalScrollbar = true;

		// TODO: Bug: Using custom GUIStyles doesn't work for some reason...
		/*/// <summary>
		/// The GUIStyle of the horizontal scroll-bar.
		/// </summary>
		public String HorizontalGUIStyle = "horizontalScrollbar";

		/// <summary>
		/// The GUIStyle of the vertical scroll-bar.
		/// </summary>
		public String VerticalGUIStyle = "verticalScrollbar";*/

		/// <summary>
		/// The scrolling position of the control.
		/// </summary>
		public Vector2 RelativeScrollPosition = new Vector2(0, 0);

		#endregion

		#region Functions

		public override void Initialize(GUIContainer Parent)
		{
			base.Initialize(Parent);

			Dock = GUIControlDock.Full;
		}

		protected override String DefaultGUIStyle()
		{
			return "box";
		}

		protected override void DrawControl(Rect RelativeArea, GUIStyle ControlGUIStyle)
		{
			var InnerRelativeArea = new	Rect(0, 0, ScrollableRelativeAreaSize.x, ScrollableRelativeAreaSize.y);
			if (AlwaysShowHorizontlScrollbar)
				InnerRelativeArea.width = Mathf.Max(ScrollableRelativeAreaSize.x, RelativeArea.width);
			if (AlwaysShowVerticalScrollbar)
				InnerRelativeArea.height = Mathf.Max(ScrollableRelativeAreaSize.y, RelativeArea.height);

			var ScrollPositionInPixels = ToPixels(RelativeScrollPosition);
			var AreaInPixels = ToPixels(RelativeArea);
			ScrollPositionInPixels = GUI.BeginScrollView(
				AreaInPixels,
				ScrollPositionInPixels,
				ToPixels(InnerRelativeArea),
				AlwaysShowHorizontlScrollbar, AlwaysShowVerticalScrollbar);
				//HorizontalGUIStyle, VerticalGUIStyle);

			RelativeScrollPosition = ToRelative(ScrollPositionInPixels);

			DrawControls();

			GUI.EndScrollView();
		}

		#endregion

		#region EditorFunctions

#if UNITY_EDITOR

		public override GUIControl Select(Rect AncestorAllocatedRelativeArea, Vector2 Position)
		{
			return base.Select(AncestorAllocatedRelativeArea, Position - new Vector2(RelativeArea.x, RelativeArea.y));
		}

#endif

		#endregion
	}
}