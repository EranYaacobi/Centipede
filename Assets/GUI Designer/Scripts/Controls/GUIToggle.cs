using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GUIDesigner.Scripts.Controls
{
	public class GUIToggle : GUIControl
	{
		#region Variables

		/// <summary>
		/// Indicates whether the control is toggled.
		/// </summary>
		public Boolean Toggled;

		/// <summary>
		/// An event that is raised when the toggle checked stat is changed.
		/// </summary>
		public event GUIControlEventHandler<GUIControlCheckedChangedEventArgs> CheckedChanged;

		#endregion

		#region Functions

		/// <summary>
		/// Draws the control.
		/// </summary>
		protected override void DrawControl(Rect RelativeArea, GUIStyle ControlGUIStyle)
		{
			var OldToggledState = Toggled;
			Toggled = GUI.Toggle(ToPixels(RelativeArea), Toggled, GUIContent, ControlGUIStyle);

			if (OldToggledState != Toggled)
			{
				if (CheckedChanged != null)
					CheckedChanged(new GUIControlCheckedChangedEventArgs(this, Toggled));
			}
		}

		protected override String DefaultGUIStyle()
		{
			return "toggle";
		}

		#endregion
	}
}