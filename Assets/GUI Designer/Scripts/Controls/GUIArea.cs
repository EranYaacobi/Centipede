using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GUIDesigner.Scripts.Controls
{
	public class GUIArea : GUIContainer
	{
		#region Customization

		protected override Vector2 DefaultSize
		{
			get { return new Vector2(0.25F, 0.25F); }
		}

		#endregion

		#region Functions

		protected override void DrawControl(Rect RelativeArea, GUIStyle ControlGUIStyle)
		{
			var AreaInPixels = ToPixels(RelativeArea);
			GUI.BeginGroup(AreaInPixels, GUIContent, ControlGUIStyle);

			DrawControls();

			GUI.EndGroup();
		}

		protected override String DefaultGUIStyle()
		{
			return "box";
		}

		#endregion
	}
}