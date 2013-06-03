using System;
using System.Linq;
using System.Collections.Generic;
using GUIDesigner.Scripts.Controls;
using GUIDesigner.Scripts.Utilities;
using UnityEngine;

namespace GUIDesigner.Scripts
{
	[ExecuteInEditMode]
	public class GUIFrame : MonoBehaviour
	{
		#region Variables

		/// <summary>
		/// The name of the GUIFrame.
		/// This will also be the name of the created file and class.
		/// </summary>
		public String Name;

		/// <summary>
		/// The area of the GUIFrame.
		/// </summary>
		[HideInInspector]
		public GUIArea Area;

		/// <summary>
		/// The GUI skin used by the GUIFrame.
		/// </summary>
		public GUISkin GUISkin;

#pragma warning disable 0414
		/// <summary>
		/// Stores reference to the script that implements all the events.
		/// </summary>
		private MonoBehaviour Script;
#pragma warning restore 0414

		#endregion

		#region EditorVariables

#if UNITY_EDITOR

		/* When changing the HideFlags of the selected control, the GUIFrameEditor is destroyed and recreated,
		 * which causes all its data to be lost.
		 * Therefore, persistent data is stored here.
		 */

		/// <summary>
		/// The position of the tool-window.
		/// </summary>
		[HideInInspector] public Rect ToolWindowPosition;

		/// <summary>
		/// The currently selected control in the editor.
		/// Therefore, 
		/// </summary>
		[HideInInspector] public GUIControl SelectedControl;

		/// <summary>
		/// The currently applied mouse action.
		/// </summary>
		[HideInInspector] public GUIControl.MouseAction MouseAction;

		/// <summary>
		/// The currently copied control.
		/// </summary>
		[HideInInspector] public GUIControl CopiedControl;

#endif

		#endregion

		#region Functions
		
		private void Awake()
		{
			if (String.IsNullOrEmpty(Name))
				Name = String.Format("{0}", gameObject.name);
			else
			{
				if (Application.isPlaying)
				{
					var ScriptName = String.Format("{0}GUI", Name);
					Script = gameObject.AddComponent(ScriptName) as MonoBehaviour;
					if (Script == null)
						Debug.LogError(String.Format("{0} is missing!", ScriptName));
				}
			}
		}

		private void OnEnable()
		{
			if (Script != null)	
				Script.enabled = true;
		}

		private void OnDisable()
		{
			if (Script != null)
				Script.enabled = false;
		}

		private void OnGUI()
		{
			Draw();
		}

		/// <summary>
		/// Draws the GUIFrame.
		/// </summary>
		public void Draw()
		{
			var PreviousGUISkin = GUI.skin;

			if (GUISkin != null)
				GUI.skin = GUISkin;

			Area.Draw(new Rect(0, 0, 1, 1));

			GUI.skin = PreviousGUISkin;
		}

		#endregion

		#region EditorFunctions

#if UNITY_EDITOR

		private void Reset()
		{
			var OldItem = GetComponent<GUIFrame>();

			// Checking if another GUIFrame already exists.
			if ((OldItem != null) && (OldItem != this))
			{
				if (UnityEditor.EditorUtility.DisplayDialog("Replace existing component?", "A GUIFrame is already added, do you want to replace it?", "Replace", "Cancel"))
				{
					gameObject.AddComponent<DeleteComponent>().ComponentReference = OldItem;
					if (OldItem.Area != null)
						OldItem.Area.DestroyControl();
				}
				else
				{
					gameObject.AddComponent<DeleteComponent>().ComponentReference = this;
					if (this.Area != null)
						this.Area.DestroyControl();
				}
			}
		}

		private void OnDrawGizmosSelected()
		{
			// Drawing a frame around the currently selected control.
			// This is done here, because the Gizmos functions are much more convenient
			// than the Handles functions.
			const Single Depth = -1F;
			var CubeSize = new Vector2(5, 5);

			if (SelectedControl != null)
			{
				var ControlArea = SelectedControl.GetArea(new Rect(0, 0, 1, 1));

				var TopLeft = new Vector3(ControlArea.x, ControlArea.y, Depth);
				var TopRight = new Vector3(ControlArea.x + ControlArea.width, ControlArea.y, Depth);
				var BottomLeft = new Vector3(ControlArea.x, ControlArea.y + ControlArea.height, Depth);
				var BottomRight = new Vector3(ControlArea.x + ControlArea.width, ControlArea.y + ControlArea.height, Depth);

				Gizmos.DrawLine(TopLeft, TopRight);
				Gizmos.DrawLine(TopRight, BottomRight);
				Gizmos.DrawLine(BottomRight, BottomLeft);
				Gizmos.DrawLine(BottomLeft, TopLeft);

				Gizmos.DrawCube(TopLeft, CubeSize);
				Gizmos.DrawCube((TopLeft + TopRight)/2, CubeSize);
				Gizmos.DrawCube(TopRight, CubeSize);
				Gizmos.DrawCube((TopRight + BottomRight)/2, CubeSize);
				Gizmos.DrawCube(BottomRight, CubeSize);
				Gizmos.DrawCube((BottomRight + BottomLeft)/2, CubeSize);
				Gizmos.DrawCube(BottomLeft, CubeSize);
				Gizmos.DrawCube((BottomLeft + TopLeft)/2, CubeSize);
			}
		}

#endif

		#endregion
	}
}