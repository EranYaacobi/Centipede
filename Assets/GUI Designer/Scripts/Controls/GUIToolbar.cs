using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GUIDesigner.Scripts.Controls
{
	public class GUIToolbar : GUIControl
	{
		#region Customization

#if UNITY_EDITOR

		public override IEnumerable<string> HiddenMembers
		{
			get { return new List<String> {"GUIContent"}; }
		}

#endif

		#endregion

		#region Variables

		/// <summary>
		/// The contents of the toolbar.
		/// </summary>
		public GUIContent[] GUIContents;

		/// <summary>
		/// The currently selected item.
		/// </summary>
		[HideInInspector] public Int32 SelectedIndex;

		/// <summary>
		/// An event that is raised when the currently selected index is changed.
		/// </summary>
		public event GUIControlEventHandler<GUIControlSelectedIndexChangedEventArgs> SelectedIndexChanged;

		#endregion

		#region Functions

		protected override void DrawControl(Rect RelativeArea, GUIStyle ControlGUIStyle)
		{
			if ((GUIContents == null) || (GUIContents.Length == 0))
			{
				GUIContents = new GUIContent[1];
				GUIContents[0] = new GUIContent("Enter text here");
			}

			if ((GUIContents != null) && (GUIContents.Length > 0))
			{
				var PreviousIndex = SelectedIndex;
				SelectedIndex = GUI.Toolbar(ToPixels(RelativeArea), SelectedIndex, GUIContents, ControlGUIStyle);

				if (PreviousIndex != SelectedIndex)
				{
					if (SelectedIndexChanged != null)
						SelectedIndexChanged(new GUIControlSelectedIndexChangedEventArgs(this, SelectedIndex, PreviousIndex));
				}
			}
		}

		protected override String DefaultGUIStyle()
		{
			return "button";
		}

		public override GUIControl Duplicate()
		{
			var NewControl = base.Duplicate() as GUIToolbar;
			NewControl.GUIContents = new GUIContent[GUIContents.Length];

			for (var Index = 0; Index < GUIContents.Length; Index++)
				NewControl.GUIContents[Index] = new GUIContent(GUIContents[Index].text, GUIContents[Index].image, GUIContents[Index].tooltip);

			return NewControl;
		}

		#endregion
	}
}