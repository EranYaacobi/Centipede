using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GUIDesigner.Scripts.Controls
{
	public abstract class GUIContainer : GUIControl
	{
		#region Customization

		/// <summary>
		/// The relative size of the inner area.
		/// </summary>
		public virtual Vector2 RelativeInnerAreaSize
		{
			get { return new Vector2(1,1); }
		}

		/// <summary>
		/// The relative offset of the inner area.
		/// </summary>
		public virtual Vector2 RelativeInnerAreaOffset
		{
			get { return new Vector2(0, 0); }
		}

#if UNITY_EDITOR

		protected override Boolean DisableInEditor
		{
			get { return false; }
		}

#endif

		#endregion

		#region Variables

		/// <summary>
		/// The child controls of the GUIContainer.
		/// </summary>
		[HideInInspector]
		public List<GUIControl> Controls;

		#endregion

		#region Functions

		public override void Initialize(GUIContainer Parent)
		{
			base.Initialize(Parent);

			Controls = new List<GUIControl>();
		}

		/// <summary>
		/// Gets for each child control, the relative area allocated to it by the container.
		/// </summary>
		/// <returns></returns>
		public virtual Dictionary<GUIControl, Rect> GetChildrenAllocatedAreas()
		{
			var AllocatedArea = new Rect(0, 0, 1, 1);
			return Controls.ToDictionary(Control => Control, Control => AllocatedArea);
		}

		/// <summary>
		/// Draws the control.
		/// </summary>
		protected virtual void DrawControls()
		{
			foreach (var ControlInfo in GetChildrenAllocatedAreas())
				ControlInfo.Key.Draw(ControlInfo.Value);
		}

		public override GUIControl Duplicate()
		{
			var NewControl = base.Duplicate() as GUIContainer;
			NewControl.Controls = new List<GUIControl>();

			foreach (var Control in Controls)
			{
				var NewChildControl = Control.Duplicate();
				NewChildControl.Parent = NewControl;
				NewControl.Controls.Add(NewChildControl);
			}


			return NewControl;
		}

		#endregion

		#region EditorFunctions

#if UNITY_EDITOR

		protected override void Update()
		{
			base.Update();

			Controls = Controls.Where(Control => Control != null).ToList();
		}

		public override void DestroyControl()
		{
			foreach (var Control in Controls)
				Control.DestroyControl();

			base.DestroyControl();
		}

		public override Boolean ShowInInspector(GUIControl SelectedControl)
		{
			var ChildSelected = false;
			foreach (var Control in Controls)
			{
				if (Control.ShowInInspector(SelectedControl))
					ChildSelected = true;
			}

			if (ChildSelected)
			{
				hideFlags = 0;
				return true;
			}

			return base.ShowInInspector(SelectedControl);
		}

		public override GUIControl Select(Rect AncestorAllocatedRelativeArea, Vector2 Position)
		{
			foreach (var Control in Controls)
			{
				var SelectedControl = Control.Select(AncestorAllocatedRelativeArea, Position);
				if (SelectedControl != null)
					return SelectedControl;
			}

			return base.Select(AncestorAllocatedRelativeArea, Position);
		}

		public override MouseAction GetAction(Rect AncestorAllocatedRelativeArea, Vector2 Position)
		{
			return GetAction(AncestorAllocatedRelativeArea, Position, true);
		}

		/// <summary>
		/// Gets the action to be performed on the control, based on the given position.
		/// </summary>
		/// <param name="AncestorAllocatedRelativeArea"></param>
		/// <param name="Position"></param>
		/// <param name="IncludeChildren"></param>
		/// <returns></returns>
		public MouseAction GetAction(Rect AncestorAllocatedRelativeArea, Vector2 Position, Boolean IncludeChildren)
		{
			if (IncludeChildren)
			{
				foreach (var Control in Controls)
				{
					MouseAction Action;

					Action = Control.GetAction(AncestorAllocatedRelativeArea, Position);

					if (Action != MouseAction.None)
						return Action;
				}

			}
			return base.GetAction(AncestorAllocatedRelativeArea, Position);
		}

		public override List<String> GenerateDeclarations()
		{
			var Result = base.GenerateDeclarations();

			foreach (var Control in Controls)
				Result.AddRange(Control.GenerateDeclarations());

			return Result;
		}

		public override List<String> GenerateCode()
		{
			var Result = base.GenerateCode();

			foreach (var Control in Controls)
				Result.AddRange(Control.GenerateCode());

			return Result;
		}

#endif

		#endregion
	}
}