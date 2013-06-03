using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GUIDesigner.Scripts.Controls
{
	public class GUITable<T> : GUIScrollView where T : GUITableItem
	{
		#region Variables

		/// <summary>
		/// The items in the list.
		/// </summary>
		public List<T> Items = new List<T>();

		/// <summary>
		/// The relative width of each column (relative to size of the scroll-view - not its inner area).
		/// </summary>
		public List<Single> ColumnsRelativeWidth = new List<Single>();

		/// <summary>
		/// The height of the headers row, in pixels.
		/// </summary>
		public Single HeaderHeight = 30;

		/// <summary>
		/// The height of a row, in pixels.
		/// </summary>
		public Single RowHeight = 20;

		/// <summary>
		/// The GUIStyle of the header cells in the table.
		/// </summary>
		public String HeaderCellGUIStyle = "box";

		/// <summary>
		/// The GUIStyle of the cells in the table.
		/// </summary>
		public String CellGUIStyle = "label";

		/// <summary>
		/// The GUIStyle used to highlight a row in the table (as an overlay).
		/// </summary>
		public String HighlightRowGUIStyle = "box";

		/// <summary>
		/// The offset of cells content, in pixels.
		/// </summary>
		public Vector2 CellOffset = new Vector2(4,0);

		/// <summary>
		/// The order of the T's fields in the table.
		/// </summary>
		[HideInInspector]
		public List<Int32> ColumnsOrder = new List<Int32>();

		/// <summary>
		/// The currently selected row's index.
		/// </summary>
		[HideInInspector]
		public Int32 SelectedIndex = -1;

		/// <summary>
		/// An event that is raised when the currently selected index is changed.
		/// </summary>
		public event GUIControlEventHandler<GUIControlSelectedIndexChangedEventArgs> SelectedIndexChanged;

#if UNITY_EDITOR
		/// <summary>
		/// The currently selected column.
		/// </summary>
		private Int32 SelectedColumnIndex;

		/// <summary>
		/// The X-Axis movement of the mouse in reordering.
		/// </summary>
		private Single ReorderMouseX;

		/// <summary>
		/// A border used to prevent flickering of columns while reordering.
		/// </summary>
		private Single RestoreOrderBorder;

		/// <summary>
		/// The direction of the border.
		/// </summary>
		private Single RestoreOrderBorderDirection;

#endif

		#endregion

		#region Functions

		protected override void DrawControls()
		{
 			base.DrawControls();
			var Members = typeof (T).GetMembers(BindingFlags.Public | BindingFlags.Instance).ToList();
			var FieldsAndProperties = Members.Where(MemberInfo => (MemberInfo is FieldInfo) || (MemberInfo is PropertyInfo));
			var VisibleMembers = FieldsAndProperties.Where(MemberInfo => !Attribute.IsDefined(MemberInfo, typeof(GUITableHideInTableAttribute))).ToList();

#if UNITY_EDITOR
			while (VisibleMembers.Count != ColumnsRelativeWidth.Count)
			{
				if (VisibleMembers.Count > ColumnsRelativeWidth.Count)
					ColumnsRelativeWidth.Add(1F/(VisibleMembers.Count + 0.15F));
				else
					ColumnsRelativeWidth.RemoveAt(ColumnsRelativeWidth.Count - 1);
			}
			while (VisibleMembers.Count != ColumnsOrder.Count)
			{
				if (VisibleMembers.Count > ColumnsOrder.Count)
					ColumnsOrder.Add(ColumnsOrder.Count);
				else
					ColumnsOrder.Remove(ColumnsOrder.Count - 1);
			}
#endif
			
			var ColumnsWidthInPixels = ColumnsRelativeWidth.Select(RelativeWidth => RelativeWidth * SizeInPixel.x).ToList();
			VisibleMembers = ColumnsOrder.Select(ColumnOrder => VisibleMembers[ColumnOrder]).ToList();

			ScrollableRelativeAreaSize = new Vector2(ColumnsRelativeWidth.Sum(), Items.Count * RowHeight / SizeInPixel.y);

			// Draw headers.
			var CurrentPosition = Vector2.zero;
			for (var i = 0; i < VisibleMembers.Count; i++)
			{
				var MemberInfo = VisibleMembers[i];
				var MemberRect = new Rect(CurrentPosition.x, CurrentPosition.y, ColumnsWidthInPixels[i], HeaderHeight);

				// Adding spaces before capital letters.
				var ViewName = Regex.Replace(MemberInfo.Name, "([a-z])([A-Z])", "$1 $2");

				var TempRect = new Rect();
				GUI.Box(MemberRect, ViewName, CreateAlignedGUIStyle(HeaderCellGUIStyle, MemberInfo, ref TempRect));
				CurrentPosition.x += ColumnsWidthInPixels[i];
			}
			CurrentPosition = new Vector2(0, HeaderHeight);

			// Handle row selection.
			DrawRowSelection();

			// Draw items.
			foreach (var Item in Items)
			{
				for (var i = 0; i < VisibleMembers.Count; i++)
				{
					var MemberInfo = VisibleMembers[i];
					var MemberRect = new Rect(CurrentPosition.x, CurrentPosition.y, ColumnsWidthInPixels[i], RowHeight);

					var CustomHandlerAttribute = Attribute.GetCustomAttribute(MemberInfo, typeof (GUITableCustomHandlerAttribute)) as GUITableCustomHandlerAttribute;
					object MemberValue;
					if (MemberInfo is FieldInfo)
						MemberValue = ((FieldInfo)MemberInfo).GetValue(Item);
					else
						MemberValue = ((PropertyInfo)MemberInfo).GetValue(Item, null);
					if (CustomHandlerAttribute != null)
					{
						CustomHandlerAttribute.Handler(Item, MemberValue, MemberRect);
					}
					else
					{
						var CellStyle = CreateAlignedGUIStyle(CellGUIStyle, MemberInfo, ref MemberRect);

						GUI.Label(MemberRect, MemberValue.ToString(), CellStyle);
					}

					CurrentPosition.x += ColumnsWidthInPixels[i];
				}

				CurrentPosition = new Vector2(0, CurrentPosition.y + RowHeight);
			}
		}

		/// <summary>
		/// Draws the currently select row highlighting, and handles input to change the selected row if necessary.
		/// </summary>
		protected void DrawRowSelection()
		{
			if (SelectedIndex != -1)
				GUI.Box(new Rect(0, HeaderHeight + SelectedIndex * RowHeight, ScrollableRelativeAreaSize.x * SizeInPixel.x, RowHeight), new GUIContent(), GUI.skin.FindStyle(HighlightRowGUIStyle) ?? new GUIStyle());

			var MousePosition = Event.current.mousePosition + new Vector2(RelativeInnerAreaOffset.x * SizeInPixel.x, RelativeInnerAreaOffset.y * SizeInPixel.y);

			if ((!Event.current.isMouse) || (Event.current.type != EventType.MouseDown) || (Event.current.button != 0))
				return;

			if (!new Rect(0, 0, SizeInPixel.x, SizeInPixel.y).Contains(MousePosition))
				return;

			MousePosition.y -= HeaderHeight;

			if (SelectedIndex >= Items.Count)
			{
				var InvalidIndex = SelectedIndex;
				SelectedIndex = -1;
				if (SelectedIndexChanged != null)
					SelectedIndexChanged(new GUIControlSelectedIndexChangedEventArgs(this, SelectedIndex, InvalidIndex));
			}

			var PreviousIndex = SelectedIndex;
			for (var Index = 0; Index < Items.Count; Index++)
			{
				MousePosition.y -= RowHeight;
				if (MousePosition.y <= 0)
				{
					PreviousIndex = SelectedIndex;
					SelectedIndex = Index;
					break;
				}
			}

			if (SelectedIndex != PreviousIndex)
			{
				if (SelectedIndexChanged != null)
					SelectedIndexChanged(new GUIControlSelectedIndexChangedEventArgs(this, SelectedIndex, PreviousIndex));
			}
		}

		public override GUIControl Duplicate()
		{
			var NewControl = base.Duplicate() as GUITable<T>;

			NewControl.Items = Items.Select(Item => Item.Duplicate()).Where(Item => Item != null).Cast<T>().ToList();
			NewControl.ColumnsRelativeWidth = new List<Single>(ColumnsRelativeWidth);
			NewControl.ColumnsOrder = new List<Int32>(ColumnsOrder);

			return NewControl;
		}

		/// <summary>
		/// Creates a correctly aligned GUIStyle for the given member.
		/// </summary>
		/// <param name="GUIStyleName"></param>
		/// <param name="MemberInfo"></param>
		/// <param name="MemberRect"> </param>
		/// <returns></returns>
		private GUIStyle CreateAlignedGUIStyle(String GUIStyleName, MemberInfo MemberInfo, ref Rect MemberRect)
		{
			var CreatedGUIStyle = GUI.skin.FindStyle(GUIStyleName) ?? new GUIStyle();
			var AlignmentAttribute = Attribute.GetCustomAttribute(MemberInfo, typeof(GUITableAlignmentAttribute)) as GUITableAlignmentAttribute;
			var IgnoreOffset = false;
			if (AlignmentAttribute != null)
			{
				CreatedGUIStyle.alignment = AlignmentAttribute.Alignment;
				IgnoreOffset = AlignmentAttribute.IgnoreOffset;
			}

			if (!IgnoreOffset)
			{
				MemberRect.x += CellOffset.x;
				MemberRect.y += CellOffset.y;
			}

			return CreatedGUIStyle;
		}

		#endregion

		#region EditorFunctions

#if UNITY_EDITOR

		public override MouseAction GetAction(Rect AncestorAllocatedRelativeArea, Vector2 Position)
		{
			var AreaInPixels = GetArea(AncestorAllocatedRelativeArea);
			var ColumnsWidthInPixels = ColumnsRelativeWidth.Select(RelativeWidth => RelativeWidth * SizeInPixel.x).ToList();

			RestoreOrderBorder = 0;

			var InitialPosition = new Vector2(AreaInPixels.x - SizeInPixel.x * RelativeInnerAreaOffset.x,
											  AreaInPixels.y - SizeInPixel.y * RelativeInnerAreaOffset.y);
			var CurrentPosition = InitialPosition;
			for (int Index = 0; Index < ColumnsWidthInPixels.Count; Index++)
			{
				var ColumnWidthInPixels = ColumnsWidthInPixels[Index];

				var Column = new Rect(CurrentPosition.x, CurrentPosition.y, ColumnWidthInPixels, HeaderHeight);
				var ColumnEnd = new Rect(CurrentPosition.x + ColumnWidthInPixels - BorderSelectionSize / 2, CurrentPosition.y, BorderSelectionSize, RowHeight);
				if (ColumnEnd.Contains(Position))
				{
					SelectedColumnIndex = Index;
					return MouseAction.ColumnResize;
				}
				if (Column.Contains(Position))
				{
					SelectedColumnIndex = Index;
					ReorderMouseX = Position.x - InitialPosition.x;
					return MouseAction.ColumnReorder;
				}

				CurrentPosition.x += ColumnWidthInPixels;
			}

			return base.GetAction(AncestorAllocatedRelativeArea, Position);
		}

		public override void PerformAction(Vector2 MouseDelta, MouseAction MouseAction)
		{
			base.PerformAction(MouseDelta, MouseAction);

			var RelativeMouseDelta = ToRelative(MouseDelta);
			switch (MouseAction)
			{
				case MouseAction.ColumnResize:
					RelativeMouseDelta.x = Mathf.Max(RelativeMouseDelta.x, Mathf.Min(0, 0.03F - ColumnsRelativeWidth[SelectedColumnIndex]));
					if (SelectedColumnIndex + 1 < ColumnsRelativeWidth.Count)
						RelativeMouseDelta.x = Mathf.Min(RelativeMouseDelta.x, Mathf.Max(0, ColumnsRelativeWidth[SelectedColumnIndex + 1] - 0.03F));
					ColumnsRelativeWidth[SelectedColumnIndex] += RelativeMouseDelta.x;
					if (SelectedColumnIndex + 1 < ColumnsRelativeWidth.Count)
						ColumnsRelativeWidth[SelectedColumnIndex + 1] -= RelativeMouseDelta.x;
					break;
				case MouseAction.ColumnReorder:
					ReorderMouseX += MouseDelta.x;
					var ColumnsWidthInPixels = ColumnsRelativeWidth.Select(RelativeWidth => RelativeWidth * SizeInPixel.x).ToList();
					var TotalWidth = 0F;
					var NewColumnIndex = SelectedColumnIndex;
					for (var Index = 0; Index < ColumnsWidthInPixels.Count; Index++)
					{
						var ColumnWidthInPixels = ColumnsWidthInPixels[Index];
						TotalWidth += ColumnWidthInPixels;
						if (ReorderMouseX < TotalWidth)
						{
							if (NewColumnIndex == Index)
								RestoreOrderBorder = 0;

							if (RestoreOrderBorder != 0)
							{
								if (Mathf.Sign(ReorderMouseX.CompareTo(RestoreOrderBorder)) == RestoreOrderBorderDirection)
									NewColumnIndex = Index;
							}
							else
							{
								NewColumnIndex = Index;
							}
							break;
						}
					}

					if (NewColumnIndex != SelectedColumnIndex)
					{
						if (ColumnsRelativeWidth[SelectedColumnIndex] < ColumnsRelativeWidth[NewColumnIndex])
						{
							RestoreOrderBorder = ColumnsWidthInPixels.Take(Mathf.Min(SelectedColumnIndex, NewColumnIndex) + 1).Sum();
							RestoreOrderBorderDirection = Mathf.Sign(SelectedColumnIndex - NewColumnIndex);
							RestoreOrderBorder += 0.01F*RestoreOrderBorderDirection;
						}

						var TempWidth = ColumnsRelativeWidth[NewColumnIndex];
						ColumnsRelativeWidth[NewColumnIndex] = ColumnsRelativeWidth[SelectedColumnIndex];
						ColumnsRelativeWidth[SelectedColumnIndex] = TempWidth;

						var Column = ColumnsOrder[NewColumnIndex];
						ColumnsOrder[NewColumnIndex] = ColumnsOrder[SelectedColumnIndex];
						ColumnsOrder[SelectedColumnIndex] = Column;

						SelectedColumnIndex = NewColumnIndex;
					}

					break;
			}
		}

#endif

		#endregion
	}

	/// <summary>
	/// A base class for a GUITable item.
	/// Any public field in derived class will be shown in a separate column in the table,
	/// unless marked with [HideInInspector].
	/// </summary>
	[Serializable]
	public abstract class GUITableItem
	{
		/// <summary>
		/// Performs simple duplication of the item.
		/// Notice that reference types (except for strings) are not copied to the duplicate.
		/// </summary>
		/// <returns></returns>
		public virtual GUITableItem Duplicate()
		{
			var Constructor = GetType().GetConstructor(new Type[0]);
			if (Constructor == null)
			{
				Debug.LogError(String.Format("Can't perform duplication, as type {0} doesn't have an empty constructor.", GetType().Name));
				return null;
			}
			var NewItem = Constructor.Invoke(new object[0]) as GUITableItem;
			foreach (var FieldInfo in GetType().GetFields())
			{
				if ((!FieldInfo.FieldType.IsClass) || (FieldInfo.FieldType == typeof(String)))
					FieldInfo.SetValue(NewItem, FieldInfo.GetValue(this));
			}

			return NewItem;
		}
	}

	/// <summary>
	/// An attribute that can be used to draw a GUITableItem's field in a custom manner.
	/// </summary>
	public class GUITableCustomHandlerAttribute : Attribute
	{
		/// <summary>
		/// The custom handler that shows the content of the field.
		/// The parameters are the Item, the field in it, and the rectangle in which the field should be put.
		/// </summary>
		public Action<GUITableItem, object, Rect> Handler;

		public GUITableCustomHandlerAttribute(Action<GUITableItem, object, Rect> Handler)
		{
			this.Handler = Handler;
		}
	}

	/// <summary>
	/// An attribute that can be used to specify a GUITableItem's field text alignment.
	/// (relevant only if it does not have a custom handler).
	/// </summary>
	public class GUITableAlignmentAttribute : Attribute
	{
		/// <summary>
		/// The alignment of the field in the table.
		/// </summary>
		public TextAnchor Alignment;

		/// <summary>
		/// Indicates whether the default cell offset should be ignored or not.
		/// </summary>
		public Boolean IgnoreOffset;

		public GUITableAlignmentAttribute(TextAnchor Alignment, Boolean IgnoreOffset = true)
		{
			this.Alignment = Alignment;
			this.IgnoreOffset = IgnoreOffset;
		}
	}

	/// <summary>
	/// An attribute that can be used to specify that a GUITableItem's field shouldn't appear
	/// in the table.
	/// </summary>
	public class GUITableHideInTableAttribute : Attribute
	{
		public GUITableHideInTableAttribute()
		{
		}
	}
}