using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GUIDesigner.Scripts.Controls
{
	public class GUIHorizontalSlider : GUIControl
	{
		#region Customization

		protected override Vector2 DefaultSize
		{
			get { return new Vector2(0.2F, 0.05F); }
		}

#if UNITY_EDITOR

		public override IEnumerable<string> HiddenMembers
		{
			get { return new List<String> { "GUIContent", "FontSize" }; }
		}

#endif

		#endregion

		#region Variables

		/// <summary>
		/// The current value of the slider.
		/// </summary>
		public Single Value;

		/// <summary>
		/// The value at the left end of the slider.
		/// </summary>
		public Single LeftValue;

		/// <summary>
		/// The value at the right end of the slider.
		/// </summary>
		public Single RightValue;

		/// <summary>
		/// The GUIStyle of the thumb.
		/// </summary>
		public String ThumbGUIStyle = "horizontalSliderThumb";

		/// <summary>
		/// An event that is raised when the value of the slider is changed.
		/// </summary>
		public event GUIControlEventHandler<GUIControlValueChangedEventArgs> ValueChanged;

		#endregion

		#region Functions

		/// <summary>
		/// Draws the control.
		/// </summary>
		protected override void DrawControl(Rect RelativeArea, GUIStyle ControlGUIStyle)
		{
			var OldValue = Value;
			Value = GUI.HorizontalSlider(ToPixels(RelativeArea), Value, LeftValue, RightValue, ControlGUIStyle, ThumbGUIStyle);

			if (OldValue != Value)
			{
				if (ValueChanged != null)
					ValueChanged(new GUIControlValueChangedEventArgs(this, Value, OldValue));
			}
		}

		protected override String DefaultGUIStyle()
		{
			return "horizontalSlider";
		}

		#endregion
	}
}