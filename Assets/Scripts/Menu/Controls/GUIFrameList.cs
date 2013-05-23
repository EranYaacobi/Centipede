using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif

/// <summary>
/// A GUIFrame list, that can be used to show\select items.
/// Notice that the list items cannot be edited from the editor, but only in a game.
/// </summary>
public class GUIFrameList : GUIFrameControl
{
	/// <summary>
	/// The action to be performed when an item is selected (meaning when the selected item is changed).
	/// This can be null.
	/// </summary>
	public Action Action;

	/// <summary>
	/// The items to show in the list.
	/// </summary>
	public IList Items;

	/// <summary>
	/// The index of the selected item.
	/// </summary>
	public Int32 SelectedItemInex;

	/// <summary>
	/// The currently selected item.
	/// </summary>
	public object SelectedItem
	{
		get { return Items[SelectedItemInex]; }
	}

	/// <summary>
	/// The GUIStyle of the list's box.
	/// </summary>
	public String BoxGUIStyle;

	public override void Draw()
	{
		GUILayout.BeginVertical(BoxGUIStyle);

		if (Items != null)
		{
			var NewSelectedItemInex = GUILayout.SelectionGrid(SelectedItemInex,
			                                                  Items.Cast<object>().Select(Item => Item.ToString()).ToArray(), 1,
			                                                  GUIStyle);
			if (NewSelectedItemInex != SelectedItemInex)
			{
				SelectedItemInex = NewSelectedItemInex;
				if (Action != null)
					Action();
			}
		}

		GUILayout.FlexibleSpace();

		GUILayout.EndVertical();
	}

	public override Vector2 GetControlSize()
	{
		return Parent.GetControlSize();
	}

	/// <summary>
	/// The currently selected item, as the given type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T GetSelectedItem<T>()
	{
		return (T)Items[SelectedItemInex];
	}

#if UNITY_EDITOR

	public override void Initialize(GUIFrameControl Parent)
	{
		base.Initialize(Parent);

		//Items = new List<String>();
		BoxGUIStyle = "box";
		SelectedItemInex = 0;
	}

	protected override String DefaultGUIStyle()
	{
		return "label";
	}

	protected override void CustomDrawInspector(SerializedObject SerializedObject)
	{
		base.CustomDrawInspector(SerializedObject);

		//EditorGUILayout.PropertyField(SerializedObject.FindProperty("Items"), true);
		BoxGUIStyle = EditorGUILayout.TextField("BoxGUIStyle", BoxGUIStyle);
	}

	public override List<String> GenerateCode()
	{
		var Result = base.GenerateCode();

		//const String ListName = "Items";
		//Result.Add(String.Format("\t\t{0}.{1} = new List<String>();", Name, ListName));
		/*foreach (var Item in Items)
		{
			var ItemText = "String.Empty";
			if (Item != null)
				ItemText = String.Format("\"{0}\"", Item);
			Result.Add(String.Format("\t\t{0}.{1}.Add({2});", Name, ListName, ItemText));
		}*/

		var ControlId = GUIUtility.GetControlID(FocusType.Passive);
		switch (Event.current.GetTypeForControl(ControlId))
		{
			case EventType.mouseDown:
					GUIUtility.hotControl = ControlId;
				break;
			case EventType.mouseUp:
				break;
		}

		Result.Add(String.Format("\t\t{0}.Action = {1};", Name, Action != null ? Action.ToString() : "null"));
		Result.Add(String.Format("\t\t{0}.BoxGUIStyle = \"{1}\";", Name, BoxGUIStyle));

		return Result;
	}

#endif
}