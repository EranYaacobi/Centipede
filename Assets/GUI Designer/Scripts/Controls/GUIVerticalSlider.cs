using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GUIDesigner.Scripts.Controls
{
	public class GUIVerticalSlider : GUIControl
	{
		#region Customization

		protected override Vector2 DefaultSize
		{
			get { return new Vector2(0.05F, 0.2F); }
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
		/// The value at the top end of the slider.
		/// </summary>
		public Single TopValue;

		/// <summary>
		/// The value at the bottom end of the slider.
		/// </summary>
		public Single BottomValue;

		/// <summary>
		/// The GUIStyle of the thumb.
		/// </summary>
		public String ThumbGUIStyle = "verticalSliderThumb";

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
			Value = GUI.VerticalSlider(ToPixels(RelativeArea), Value, TopValue, BottomValue, ControlGUIStyle, ThumbGUIStyle);

			if (OldValue != Value)
			{
				if (ValueChanged != null)
					ValueChanged(new GUIControlValueChangedEventArgs(this, Value, OldValue));
			}
		}

		protected override String DefaultGUIStyle()
		{
			return "verticalSlider";
		}

		#endregion
	}
}