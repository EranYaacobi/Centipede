using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GUIDesigner.Scripts.Controls
{
	[ExecuteInEditMode]
	public abstract class GUIControl : MonoBehaviour
	{
		#region Constants

#if UNITY_EDITOR

		/// <summary>
		/// The size of the area (in pixels) in which selecting a control will select its border.
		/// </summary>
		protected const Int32 BorderSelectionSize = 11;

#endif

		#endregion

		#region Customization

		/// <summary>
		/// The minimum size of a control in pixels.
		/// </summary>
		protected virtual Vector2 MinimumSizeInPixels
		{
			get { return new Vector2(5, 5); }
		}

		/// <summary>
		/// The default size when creating a control.
		/// </summary>
		protected virtual Vector2 DefaultSize
		{
			get { return new Vector2(0.2F, 0.1F); }
		}

#if UNITY_EDITOR

		/// <summary>
		/// Override this to hide members from the GUIControl.
		/// </summary>
		public virtual IEnumerable<String> HiddenMembers
		{
			get { return new List<String>(); }
		}

		/// <summary>
		/// Indicates whether the control should be disabled in the editor, so it will be possible to
		/// drag\resize it.
		/// </summary>
		protected virtual Boolean DisableInEditor
		{
			get { return true; }
		}

#endif

		#endregion

		#region Variables

		/// <summary>
		/// The name of the control.
		/// This can be used to search for it.
		/// 
		/// When Save is performed, it also creates a variable in the designer script with that name.
		/// </summary>
		public String Name;

		/// <summary>
		/// The name of the GUIstyle of the control.
		/// </summary>
		public String GUIStyle;

		/// <summary>
		/// The content of the control.
		/// </summary>
		public GUIContent GUIContent = new GUIContent();

		/// <summary>
		/// The size of the control's font.
		/// If 0, the GUIStyle's default font size is used.
		/// </summary>
		public Int32 FontSize;

		/// <summary>
		/// The area of the control (relative to its parent, or to the screen if it doesn't have one).
		/// This may be affected by docking.
		/// </summary>
		public Rect RelativeArea;

		/// <summary>
		/// The (absolute) size of the control, in pixels.
		/// This is calculated every frame by the Draw function.
		/// </summary>
		[HideInInspector]
		public Vector2 SizeInPixel;

		/// <summary>
		/// The margins of the control (relative to its parent, or to the screen if it doesn't have one).
		/// This is relevant only when docking is used.
		/// </summary>
		public GUIControlMargins Margins;

		/// <summary>
		/// The docking of the control, relative to its parent.
		/// </summary>
		public GUIControlDock Dock;

		/// <summary>
		/// The condition needed, for the control not to be grayed-out.
		/// If null, the control is always enabled.
		/// </summary>
		public Func<Boolean> Condition;

		/// <summary>
		/// The parent control of this control.
		/// </summary>
		[HideInInspector]
		public GUIContainer Parent;

		#region Events

		/// <summary>
		/// An event that is raised when the mouse hovers over the control.
		/// </summary>
		public event GUIControlEventHandler<GUIControlEventArgs> MouseHover;

		#endregion

#if UNITY_EDITOR

		/// <summary>
		/// Data gathered by the inspector, regarding the handlers of each event,
		/// that was set by the user, in the inspector.
		/// </summary>
		[HideInInspector]
		public List<EventData> InspectorEventsData = new List<EventData>();

#endif

		#endregion

		#region Functions

		/// <summary>
		/// Initializes the control.
		/// This function should be called only when creating a new control.
		/// </summary>
		public virtual void Initialize(GUIContainer Parent)
		{
			this.Parent = Parent;

			GUIStyle = DefaultGUIStyle();
			RelativeArea = new Rect(0, 0, DefaultSize.x, DefaultSize.y);
			Name = String.Format("{0}_{1}", GetType().Name, UnityEngine.Random.Range(0, Int32.MaxValue));
			Margins = new GUIControlMargins(0, 0, 0, 0);
#if UNITY_EDITOR
			InspectorEventsData = new List<EventData>();

			if (!(this is GUIContainer))
				GUIContent.text = "Enter text here";
#endif
		}

		/// <summary>
		/// Returns the default GUIStyle.
		/// </summary>
		/// <returns></returns>
		protected virtual String DefaultGUIStyle()
		{
			return null;
		}

		/// <summary>
		/// Duplicates the control.
		/// </summary>
		/// <returns></returns>
		public virtual GUIControl Duplicate()
		{
			var NewControl = gameObject.AddComponent(GetType()) as GUIControl;
			NewControl.Initialize(null);
			foreach (var FieldInfo in GetType().GetFields())
			{
				if ((!FieldInfo.FieldType.IsClass) || ((FieldInfo.FieldType == typeof(String)) && (FieldInfo.Name != "Name")))
					FieldInfo.SetValue(NewControl, FieldInfo.GetValue(this));
			}

			NewControl.GUIContent = new GUIContent(GUIContent.text, GUIContent.image, GUIContent.tooltip);
			NewControl.RelativeArea = RelativeArea;
			NewControl.Margins = new GUIControlMargins(Margins.Left, Margins.Right, Margins.Top, Margins.Bottom);
			NewControl.Condition = Condition;

			// TODO: You may need to copy events as well.

#if UNITY_EDITOR
			NewControl.PerformAction(new Vector2(20, 20), MouseAction.Drag);

			foreach (var EventData in InspectorEventsData)
				NewControl.InspectorEventsData.Add(new EventData(EventData.EventName, EventData.HandlerName));
#endif

			return NewControl;
		}


		/// <summary>
		/// Draws the control.
		/// </summary>
		/// <param name="AllocatedRelativeArea">The area allocated to the control by its parent</param>
		public void Draw(Rect AllocatedRelativeArea)
		{
			var GUIEnablingState = GUI.enabled;

			if (Condition != null)
				GUI.enabled = Condition();
			else
				GUI.enabled = true;

#if UNITY_EDITOR
			// Disabling controls while editing.
			if (!Application.isPlaying)
			{
				if (DisableInEditor)
					GUI.enabled = false;
			}
#endif

			var ControlRelativeArea = GetControlArea(AllocatedRelativeArea);
			SizeInPixel = ToPixels(new Vector2(ControlRelativeArea.width, ControlRelativeArea.height));

			var ControlGUIStyle = GUI.skin.FindStyle(GUIStyle) ?? new GUIStyle();
			ControlGUIStyle.fontSize = FontSize;

			DrawControl(ControlRelativeArea, ControlGUIStyle);

			// Checking if the mouse hovers over the control.
			/*if (ToPixels(DockedRelativeArea).Contains(Event.current.mousePosition))
			{
				if (MouseHover != null)
					MouseHover(new GUIControlEventArgs(this));
			}*/

			GUI.enabled = GUIEnablingState;
		}

		/// <summary>
		/// In derived classes, performs the actual drawing of the control.
		/// </summary>
		protected abstract void DrawControl(Rect RelativeArea, GUIStyle ControlGUIStyle);

		/// <summary>
		/// Returns the area of the control, after considering its docking (the returned area is still relative).
		/// </summary>
		/// <returns></returns>
		public Rect GetDockedArea()
		{
			var InnerAreaRelativeSize = new Vector2(1, 1);
			if (Parent != null)
				InnerAreaRelativeSize = Parent.RelativeInnerAreaSize;

			switch (Dock)
			{
				case GUIControlDock.None:
					return RelativeArea;
				case GUIControlDock.Horizontal:
					return new Rect(Margins.Left, RelativeArea.y, InnerAreaRelativeSize.x - (Margins.Left + Margins.Right), RelativeArea.height);
				case GUIControlDock.Vertical:
					return new Rect(RelativeArea.x, Margins.Top, RelativeArea.width, InnerAreaRelativeSize.y - (Margins.Top + Margins.Bottom));
				case GUIControlDock.Full:
					return new Rect(Margins.Left, Margins.Top, InnerAreaRelativeSize.x - (Margins.Left + Margins.Right), InnerAreaRelativeSize.y - (Margins.Top + Margins.Bottom));
				default:
					ErrorMessage("Unexpected Dock value - returing original DrawArea.");
					return RelativeArea;
			}
		}

		/// <summary>
		/// Returns the area of the control, after considering both its docking, and the area allocated to it by
		/// it parent.
		/// Notice that this area is still relative.
		/// </summary>
		/// <returns></returns>
		protected Rect GetControlArea(Rect AllocatedRelativeArea)
		{
			var DockedRelativeArea = GetDockedArea();
			return new Rect(
				AllocatedRelativeArea.x + DockedRelativeArea.x * AllocatedRelativeArea.width,
				AllocatedRelativeArea.y + DockedRelativeArea.y * AllocatedRelativeArea.height,
				AllocatedRelativeArea.width * DockedRelativeArea.width,
				AllocatedRelativeArea.height * DockedRelativeArea.height);
		}

		/// <summary>
		/// Converts the given relative position to pixels, based on the parent's size.
		/// </summary>
		/// <param name="RelativePosition"></param>
		public Vector2 ToPixels(Vector2 RelativePosition)
		{
			var ContainerSizeInPixels = new Vector2(Screen.width, Screen.height);
			if (Parent != null)
				ContainerSizeInPixels = Parent.SizeInPixel;

			return new Vector2(RelativePosition.x * ContainerSizeInPixels.x, RelativePosition.y * ContainerSizeInPixels.y);
		}

		/// <summary>
		/// Converts the given position (in pixels) to a relative position, based on the parent's size.
		/// </summary>
		/// <param name="PositionInPixels"></param>
		public Vector2 ToRelative(Vector2 PositionInPixels)
		{
			var ContainerSizeInPixels = new Vector2(Screen.width, Screen.height);
			if (Parent != null)
				ContainerSizeInPixels = Parent.SizeInPixel;

			return new Vector2(PositionInPixels.x / ContainerSizeInPixels.x, PositionInPixels.y / ContainerSizeInPixels.y);
		}

		/// <summary>
		/// Converts the given Area to pixels, based on the screen's size.
		/// </summary>
		/// <param name="RelativeArea"></param>
		public Rect ToPixels(Rect RelativeArea)
		{
			var ContainerSizeInPixels = new Vector2(Screen.width, Screen.height);
			if (Parent != null)
				ContainerSizeInPixels = Parent.SizeInPixel;

			return new Rect(RelativeArea.x * ContainerSizeInPixels.x, RelativeArea.y * ContainerSizeInPixels.y, RelativeArea.width * ContainerSizeInPixels.x, RelativeArea.height * ContainerSizeInPixels.y);
		}

		/// <summary>
		/// Shows a warning message.
		/// </summary>
		public void WarningMessage(String Message)
		{
			Debug.LogWarning(String.Format("{0} '{1}': {2}", GetType().Name, Name, Message));
		}

		/// <summary>
		/// Shows an error message.
		/// </summary>
		public void ErrorMessage(String Message)
		{
			Debug.LogWarning(String.Format("{0} '{1}': {2}", GetType().Name, Name, Message));
		}

		#endregion

		#region EditorFunctions

#if UNITY_EDITOR

		/// <summary>
		/// Updates the control, including:
		/// * Filling its GUIContent with default text in specific situations.
		/// * Updating Events dictionary.
		/// * Destroying it if it is an orphan.
		/// 
		/// Notice that this does not exist in the actual game, so there will be no overhead there.
		/// </summary>
		protected virtual void Update()
		{
			if (Application.isPlaying)
				return;

			var GenericEvents = GetType().GetEvents().Where(Field => Field.EventHandlerType.IsGenericType).ToList();
			var GUIControlEvents = GenericEvents.Where(Field => Field.EventHandlerType.GetGenericTypeDefinition() == typeof(GUIControlEventHandler<>)).ToList();

			foreach (var Event in GUIControlEvents)
			{
				if (InspectorEventsData.All(EventData => EventData.EventName != Event.Name))
					InspectorEventsData.Add(new EventData(Event.Name));
			}

			InspectorEventsData.RemoveAll(EventData => GUIControlEvents.All(Event => Event.Name != EventData.EventName));


			// Showing "Enter text here" if this is not a container, the text is empty, no texture is used,
			// and the control is not selected
			if ((!(this is GUIContainer)) &&
				(String.IsNullOrEmpty(GUIContent.text)) &&
				(GUIContent.image == null) &&
				(hideFlags != 0))
			{
				GUIContent.text = "Enter text here";
			}

			if ((Parent == null) && (GetComponent<GUIFrame>() == null))
				DestroyControl();
		}

		/// <summary>
		/// Destroys the control immediately, and all its children.
		/// </summary>
		public virtual void DestroyControl()
		{
			DestroyImmediate(this);
		}

		/// <summary>
		/// Shows the given control in the inspector, and returns true if this control was shown as a result.
		/// </summary>
		/// <param name="SelectedControl"></param>
		/// <returns></returns>
		public virtual Boolean ShowInInspector(GUIControl SelectedControl)
		{
			if (this == SelectedControl)
			{
				hideFlags = 0;
				return true;
			}

			hideFlags = HideFlags.HideInInspector;
			return false;
		}

		/// <summary>
		/// Returns the GUIControl if the given position is in it.
		/// </summary>
		/// <param name="AncestorAllocatedRelativeArea">The allocated relative area of the control's ancestor</param>
		/// <param name="Position"></param>
		/// <returns></returns>
		public virtual GUIControl Select(Rect AncestorAllocatedRelativeArea, Vector2 Position)
		{
			var AreaInPixels = GetArea(AncestorAllocatedRelativeArea);

			var TotalAreaInPixels = new Rect(
				AreaInPixels.x - BorderSelectionSize / 2,
				AreaInPixels.y - BorderSelectionSize / 2,
				AreaInPixels.width + BorderSelectionSize,
				AreaInPixels.height + BorderSelectionSize);

			if (TotalAreaInPixels.Contains(Position))
				return this;

			return null;
		}

		/// <summary>
		/// Gets the action to be performed on the control, based on the given position.
		/// </summary>
		/// <param name="AncestorAllocatedRelativeArea">The allocated relative area of the control's ancestor</param>
		/// <param name="Position"></param>
		/// <returns></returns>
		public virtual MouseAction GetAction(Rect AncestorAllocatedRelativeArea, Vector2 Position)
		{
			var AreaInPixels = GetArea(AncestorAllocatedRelativeArea);

			var TotalAreaInPixels = new Rect(
				AreaInPixels.x - BorderSelectionSize / 2,
				AreaInPixels.y - BorderSelectionSize / 2,
				AreaInPixels.width + BorderSelectionSize,
				AreaInPixels.height + BorderSelectionSize);

			if (!TotalAreaInPixels.Contains(Position))
				return MouseAction.None;

			var TopLeftRect = new Rect(
				AreaInPixels.x - BorderSelectionSize / 2,
				AreaInPixels.y - BorderSelectionSize / 2,
				BorderSelectionSize, BorderSelectionSize);
			var TopRightRect = new Rect(
				AreaInPixels.x + AreaInPixels.width - BorderSelectionSize / 2,
				AreaInPixels.y - BorderSelectionSize / 2,
				BorderSelectionSize, BorderSelectionSize);
			var BottomLeftRect = new Rect(
				AreaInPixels.x - BorderSelectionSize / 2,
				AreaInPixels.y + AreaInPixels.height - BorderSelectionSize / 2,
				BorderSelectionSize, BorderSelectionSize);
			var BottomRightRect = new Rect(
				AreaInPixels.x + AreaInPixels.width - BorderSelectionSize / 2,
				AreaInPixels.y + AreaInPixels.height - BorderSelectionSize / 2,
				BorderSelectionSize, BorderSelectionSize);

			var TopRect = new Rect(AreaInPixels.x, AreaInPixels.y - BorderSelectionSize / 2, AreaInPixels.width, BorderSelectionSize);
			var LeftRect = new Rect(AreaInPixels.x - BorderSelectionSize / 2, AreaInPixels.y, BorderSelectionSize, AreaInPixels.height);
			var RightRect = new Rect(AreaInPixels.x + AreaInPixels.width - BorderSelectionSize / 2, AreaInPixels.y, BorderSelectionSize,
									 AreaInPixels.height);
			var BottomRect = new Rect(AreaInPixels.x, AreaInPixels.y + AreaInPixels.height - BorderSelectionSize / 2, AreaInPixels.width,
									  BorderSelectionSize);

			var ParentAction = MouseAction.None;
			if (Dock == GUIControlDock.None)
			{
				if (TopLeftRect.Contains(Position))
					return MouseAction.ResizeUpLeft;

				if (TopRightRect.Contains(Position))
					return MouseAction.ResizeUpRight;

				if (BottomLeftRect.Contains(Position))
					return MouseAction.ResizeDownLeft;

				if (BottomRightRect.Contains(Position))
					return MouseAction.ResizeDownRight;
			}
			else
			{
				if (Parent != null)
					ParentAction = Parent.GetAction(AncestorAllocatedRelativeArea, Position, false);
			}

			if ((Dock != GUIControlDock.Vertical) && (Dock != GUIControlDock.Full))
			{
				if (TopRect.Contains(Position))
					return MouseAction.ResizeUp;
				if (BottomRect.Contains(Position))
					return MouseAction.ResizeDown;
			}

			if ((Dock != GUIControlDock.Horizontal) && (Dock != GUIControlDock.Full))
			{
				if (LeftRect.Contains(Position))
					return MouseAction.ResizeLeft;

				if (RightRect.Contains(Position))
					return MouseAction.ResizeRight;
			}

			if (Dock != GUIControlDock.None)
			{
				if (ParentAction != MouseAction.None)
					return ParentAction;
			}

			return MouseAction.Drag;
		}

		/// <summary>
		/// Performs the given mouse action on the control.
		/// This function assumes that Draw is called every frame (as it calculates the size of the controls).
		/// </summary>
		/// <param name="MouseDelta"></param>
		/// <param name="MouseAction"></param>
		public virtual void PerformAction(Vector2 MouseDelta, MouseAction MouseAction)
		{
			var RelativeMouseDelta = ToRelative(MouseDelta);

			if ((Dock != GUIControlDock.None) && (Parent != null))
			{
				if ((MouseAction == MouseAction.ResizeUpLeft) || (MouseAction == MouseAction.ResizeUpRight) ||
					(MouseAction == MouseAction.ResizeDownLeft) || (MouseAction == MouseAction.ResizeDownRight))
				{
					Parent.PerformAction(MouseDelta, MouseAction);
					return;
				}

				switch (Dock)
				{
					case GUIControlDock.Horizontal:
						if ((MouseAction == MouseAction.ResizeLeft) || (MouseAction == MouseAction.ResizeRight))
						{
							Parent.PerformAction(MouseDelta, MouseAction);
							return;
						}
						break;
					case GUIControlDock.Vertical:
						if ((MouseAction == MouseAction.ResizeUp) || (MouseAction == MouseAction.ResizeDown))
						{
							Parent.PerformAction(MouseDelta, MouseAction);
							return;
						}
						break;
					case GUIControlDock.Full:
						if ((MouseAction == MouseAction.Drag) ||
							(MouseAction == MouseAction.ResizeUp) || (MouseAction == MouseAction.ResizeDown) ||
							(MouseAction == MouseAction.ResizeLeft) || (MouseAction == MouseAction.ResizeRight))
							
						{
							Parent.PerformAction(MouseDelta, MouseAction);
							return;
						}
						break;
				}
			}

			switch (MouseAction)
			{
				case MouseAction.None:
					break;
				case MouseAction.Drag:
					this.RelativeArea.x += RelativeMouseDelta.x;
					this.RelativeArea.y += RelativeMouseDelta.y;
					break;
				case MouseAction.ResizeUp:
					this.RelativeArea.y += RelativeMouseDelta.y;
					this.RelativeArea.height -= RelativeMouseDelta.y;
					break;
				case MouseAction.ResizeDown:
					this.RelativeArea.height += RelativeMouseDelta.y;
					break;
				case MouseAction.ResizeLeft:
					this.RelativeArea.x += RelativeMouseDelta.x;
					this.RelativeArea.width -= RelativeMouseDelta.x;
					break;
				case MouseAction.ResizeRight:
					this.RelativeArea.width += RelativeMouseDelta.x;
					break;
				case MouseAction.ResizeUpLeft:
					this.RelativeArea.y += RelativeMouseDelta.y;
					this.RelativeArea.height -= RelativeMouseDelta.y;
					this.RelativeArea.x += RelativeMouseDelta.x;
					this.RelativeArea.width -= RelativeMouseDelta.x;
					break;
				case MouseAction.ResizeUpRight:
					this.RelativeArea.y += RelativeMouseDelta.y;
					this.RelativeArea.height -= RelativeMouseDelta.y;
					this.RelativeArea.width += RelativeMouseDelta.x;
					break;
				case MouseAction.ResizeDownLeft:
					this.RelativeArea.height += RelativeMouseDelta.y;
					this.RelativeArea.x += RelativeMouseDelta.x;
					this.RelativeArea.width -= RelativeMouseDelta.x;
					break;
				case MouseAction.ResizeDownRight:
					this.RelativeArea.height += RelativeMouseDelta.y;
					this.RelativeArea.width += RelativeMouseDelta.x;
					break;
			}

			var InnerAreaRelativeSize = new Vector2(1, 1);
			if (Parent != null)
				InnerAreaRelativeSize = Parent.RelativeInnerAreaSize;

			this.RelativeArea.width = Mathf.Clamp(this.RelativeArea.width, 0, InnerAreaRelativeSize.x);
			this.RelativeArea.height = Mathf.Clamp(this.RelativeArea.height, 0, InnerAreaRelativeSize.y);
			this.RelativeArea.x = Mathf.Clamp(this.RelativeArea.x, 0, InnerAreaRelativeSize.x - this.RelativeArea.width);
			this.RelativeArea.y = Mathf.Clamp(this.RelativeArea.y, 0, InnerAreaRelativeSize.y - this.RelativeArea.height);
		}

		/// <summary>
		/// Gets the area (in pixels) of the control.
		/// </summary>
		/// <param name="AncestorAllocatedRelativeArea">The allocated relative area of the controls Ancestor</param>
		/// <returns></returns>
		public virtual Rect GetArea(Rect AncestorAllocatedRelativeArea)
		{
			if (Parent == null)
				return ToPixels(GetControlArea(AncestorAllocatedRelativeArea));

			var ParentArea = Parent.GetArea(AncestorAllocatedRelativeArea);
			var ControlRelativeArea = GetControlArea(Parent.GetChildrenAllocatedAreas()[this]);
			var AdjustedControlRelativeArea = new Rect(
				 ControlRelativeArea.x - Parent.RelativeInnerAreaOffset.x,
				 ControlRelativeArea.y - Parent.RelativeInnerAreaOffset.y,
				 ControlRelativeArea.width,
				 ControlRelativeArea.height
			);
			var ControlArea = ToPixels(AdjustedControlRelativeArea);

			return new Rect(
				ControlArea.x + ParentArea.x,
				ControlArea.y + ParentArea.y,
				ControlArea.width,
				ControlArea.height);
		}

		/// <summary>
		/// Generates the declarations of the control.
		/// </summary>
		/// <returns></returns>
		public virtual List<String> GenerateDeclarations()
		{
			return new List<String> {String.Format("\tpublic {0} {1};", GetType().Name, Name)};
		}

		/// <summary>
		/// Generates the code that finds the control, and removes it from the list.
		/// </summary>
		public virtual List<String> GenerateCode()
		{
			var Result = new List<String> { String.Format("\t\t{0} = ({1})Controls.First(Control => Control.Name == \"{0}\");", Name, GetType().Name) };

			foreach (var EventData in InspectorEventsData)
			{
				if (!String.IsNullOrEmpty(EventData.HandlerName))
					Result.Add(String.Format("\t\t{0}.{1} += {2};", Name, EventData.EventName, EventData.HandlerName));
			}

			return Result;
		}

#endif

		#endregion

		#region EditorTypes

#if UNITY_EDITOR

		/// <summary>
		/// Represents the action to be performed by the mouse on the control.
		/// </summary>
		public enum MouseAction
		{
			/// <summary>
			/// No mouse action.
			/// </summary>
			None,

			/// <summary>
			/// The mouse drags the control.
			/// This happens if the mouse click is performed near the center of the control.
			/// </summary>
			Drag,

			/// <summary>
			/// The mouse resizes the control
			/// upwards. This happens if the mouse click is performed near the upper border of the control.
			/// </summary>
			ResizeUp,

			/// <summary>
			/// The mouse resizes the control downwards.
			/// This happens if the mouse click is performed near the buttom border of the control.
			/// </summary>
			ResizeDown,

			/// <summary>
			/// The mouse resizes the control leftwards.
			/// This happens if the mouse click is performed near the left border of the control.
			/// </summary>
			ResizeLeft,

			/// <summary>
			/// The mouse resizes the control rightwards.
			/// This happens if the mouse click is performed near the right border of the control.
			/// </summary>
			ResizeRight,

			/// <summary>
			/// The mouse resizes the control up-leftwards.
			/// This happens if the mouse click is performed near the upper-left corner of the control.
			/// </summary>
			ResizeUpLeft,

			/// <summary>
			/// The mouse resizes the control up-leftwards.
			/// This happens if the mouse click is performed near the upper-right corner of the control.
			/// </summary>
			ResizeUpRight,

			/// <summary>
			/// The mouse resizes the control down-leftwards.
			/// This happens if the mouse click is performed near the down-left corner of the control.
			/// </summary>
			ResizeDownLeft,

			/// <summary>
			/// The mouse resizes the control down-rightwards.
			/// This happens if the mouse click is performed near the down-right corner of the control.
			/// </summary>
			ResizeDownRight,

			/// <summary>
			/// The mouse moves the control to another container.
			/// </summary>
			DragAndDrop,

			/// <summary>
			/// The mouse resizes a column inside the control.
			/// </summary>
			ColumnResize,

			/// <summary>
			/// The mouse reoders a column inside the control.
			/// </summary>
			ColumnReorder,
		}

		/// <summary>
		/// A class used for showing events in the inspector.
		/// </summary>
		[Serializable]
		public class EventData
		{
			/// <summary>
			/// The name of the event.
			/// </summary>
			public String EventName;

			/// <summary>
			/// The name of the event's handler.
			/// </summary>
			public String HandlerName;

			public EventData(String EventName, String HandlerName = "")
			{
				this.EventName = EventName;
				this.HandlerName = HandlerName;
			}
		}

#endif

		#endregion
	}

	public enum GUIControlDock
	{
		/// <summary>
		/// No docking. The control will appear in its drawing area.
		/// </summary>
		None,

		/// <summary>
		/// The control will expand vertically to fit its parent.
		/// </summary>
		Vertical,

		/// <summary>
		/// The control will expand horizontally to fit its parent.
		/// </summary>
		Horizontal,

		/// <summary>
		/// The control will expand in all directions to fit its parent.
		/// </summary>
		Full,
	}

	[Serializable]
	public class GUIControlMargins
	{
		/// <summary>
		/// The margins of the control from the left.
		/// </summary>
		public Single Left;

		/// <summary>
		/// The margins of the control from the right.
		/// </summary>
		public Single Right;

		/// <summary>
		/// The margins of the control from the top.
		/// </summary>
		public Single Top;

		/// <summary>
		/// The margins of the control from the bottom.
		/// </summary>
		public Single Bottom;

		public GUIControlMargins(Single Left, Single Right, Single Top, Single Bottom)
		{
			this.Left = Left;
			this.Right = Right;
			this.Top = Top;
			this.Bottom = Bottom;
		}
	}
}