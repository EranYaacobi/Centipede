using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GUIDesigner.Scripts.Controls
{
	public class GUIButton : GUIControl
	{
		#region Variables

		/// <summary>
		/// Indicates whether this is a repeat button or not.
		/// </summary>
		public Boolean RepeatButton;

		/// <summary>
		/// An event that is raised when the mouse hovers over the control.
		/// </summary>
		public event GUIControlEventHandler<GUIControlEventArgs> Click;

		#endregion

		#region Functions

		protected override void DrawControl(Rect RelativeArea, GUIStyle ControlGUIStyle)
		{
			var AreaPixels = ToPixels(RelativeArea);

			if (ControlGUIStyle.fontSize == 0)
			{
				ControlGUIStyle.fontSize = (Int32) (AreaPixels.height*0.8F - ControlGUIStyle.padding.vertical);
				var TextSize = ControlGUIStyle.CalcSize(GUIContent);
				if (TextSize.x > AreaPixels.width*0.9F)
					ControlGUIStyle.fontSize = (Int32) (ControlGUIStyle.fontSize*AreaPixels.width*0.9F/TextSize.x);
			}

			Boolean PerformAction;
			if (RepeatButton)
				PerformAction = GUI.RepeatButton(AreaPixels, GUIContent, ControlGUIStyle);
			else
				PerformAction = GUI.Button(AreaPixels, GUIContent, ControlGUIStyle);

			if (PerformAction)
			{
				if (Click != null)
					Click(new GUIControlEventArgs(this));
			}
		}

		protected override String DefaultGUIStyle()
		{
			return "button";
		}

		#endregion
	}
}