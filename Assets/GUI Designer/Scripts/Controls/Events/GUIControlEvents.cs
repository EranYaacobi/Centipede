using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using GUIDesigner.Scripts.Controls;
using UnityEngine;

/// <summary>
/// A default handler for a GUIControl event.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="EventArgs"></param>
public delegate void GUIControlEventHandler<in T>(T EventArgs) where T : GUIControlEventArgs;

/// <summary>
/// The base class for a GUIControl event arguments.
/// </summary>
public class GUIControlEventArgs : EventArgs
{
	/// <summary>
	/// The control that raised the event.
	/// </summary>
	public readonly GUIControl GUIControl;

	public GUIControlEventArgs(GUIControl GUIControl)
	{
		this.GUIControl = GUIControl;
	}
}

/// <summary>
/// Event arguments for an event where the selected index was changed.
/// </summary>
public class GUIControlSelectedIndexChangedEventArgs : GUIControlEventArgs
{
	/// <summary>
	/// The index of the currently selected item.
	/// </summary>
	public readonly Int32 CurrentIndex;

	/// <summary>
	/// The index of the previously selected item.
	/// </summary>
	public readonly Int32 PreviousIndex;

	public GUIControlSelectedIndexChangedEventArgs(GUIControl GUIControl, Int32 CurrentIndex, Int32 PreviousIndex)
		: base(GUIControl)
	{
		this.CurrentIndex = CurrentIndex;
		this.PreviousIndex = PreviousIndex;
	}
}

/// <summary>
/// Event arguments for an event where the checked state of the control was changed.
/// </summary>
public class GUIControlCheckedChangedEventArgs : GUIControlEventArgs
{
	/// <summary>
	/// Indicates whether the control state's is checked or not.
	/// </summary>
	public readonly Boolean Checked;

	public GUIControlCheckedChangedEventArgs(GUIControl GUIControl, Boolean Checked)
		: base(GUIControl)
	{
		this.Checked = Checked;
	}
}

/// <summary>
/// Event arguments for an event where the value of a control was changed.
/// </summary>
public class GUIControlValueChangedEventArgs : GUIControlEventArgs
{
	/// <summary>
	/// The current value.
	/// </summary>
	public readonly Single CurrentValue;

	/// <summary>
	/// The previous value.
	/// </summary>
	public readonly Single PreviousValue;

	public GUIControlValueChangedEventArgs(GUIControl GUIControl, Single CurrentValue, Single PreviousValue)
		: base(GUIControl)
	{
		this.CurrentValue = CurrentValue;
		this.PreviousValue = PreviousValue;
	}
}