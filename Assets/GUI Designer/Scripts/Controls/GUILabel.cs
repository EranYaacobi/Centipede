using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GUIDesigner.Scripts.Controls
{
	public class GUILabel : GUIControl
	{
		#region Functions

		protected override void DrawControl(Rect RelativeArea, GUIStyle ControlGUIStyle)
		{
			GUI.Label(ToPixels(RelativeArea), GUIContent, ControlGUIStyle);
		}

		protected override String DefaultGUIStyle()
		{
			return "label";
		}

		#endregion
	}
}