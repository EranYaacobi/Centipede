using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GUIDesigner.Scripts.Controls
{
	public class GUITextField : GUIControl
	{
		#region Variables

		/// <summary>
		/// Indicates whether the text-field supports multi-line input.
		/// </summary>
		public Boolean Multiline;

		/// <summary>
		/// The maximum length of the TextField's text.
		/// </summary>
		public Int32 MaxLength;

		/// <summary>
		/// The character used to mask the text with.
		/// If this is not empty, the TextField becomes a PasswordField.
		/// This also makes multiline irrelevant.
		/// </summary>
		public String PasswordMask;

		#endregion

		#region Functions

		protected override void DrawControl(Rect RelativeArea, GUIStyle ControlGUIStyle)
		{
			var DrawAreaPixels = ToPixels(RelativeArea);

			if (!String.IsNullOrEmpty(PasswordMask))
			{
				if (MaxLength > 0)
					GUIContent.text = GUI.PasswordField(DrawAreaPixels, GUIContent.text, PasswordMask[0], MaxLength, ControlGUIStyle);
				else
					GUIContent.text = GUI.PasswordField(DrawAreaPixels, GUIContent.text, PasswordMask[0], ControlGUIStyle);
			}
			else
			{
				if (Multiline)
				{
					if (MaxLength > 0)
						GUIContent.text = GUI.TextArea(DrawAreaPixels, GUIContent.text, MaxLength, ControlGUIStyle);
					else
						GUIContent.text = GUI.TextArea(DrawAreaPixels, GUIContent.text, ControlGUIStyle);
				}
				else
				{
					if (MaxLength > 0)
						GUIContent.text = GUI.TextField(DrawAreaPixels, GUIContent.text, MaxLength, ControlGUIStyle);
					else
						GUIContent.text = GUI.TextField(DrawAreaPixels, GUIContent.text, ControlGUIStyle);
				}
			}
		}

		protected override String DefaultGUIStyle()
		{
			return "textField";
		}

		#endregion
	}
}