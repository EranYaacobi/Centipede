using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif

public class GUIFrameButton : GUIFrameControl
{
	/// <summary>
	/// The text of the button.
	/// </summary>
	public String Text;

	/// <summary>
	/// The condition needed, for the button not to be grayed-out.
	/// If null, then the button is always enabled.
	/// </summary>
	public Func<Boolean> Condition;

	/// <summary>
	/// The action to be performed when the button is clicked.
	/// </summary>
	public Action Action;

	/// <summary>
	/// The size of the button.
	/// In each axis, if not zero, then the button expands to fit its parent.
	/// </summary>
	public Vector2 Size;

	/// <summary>
	/// The maximum size of the button.
	/// In each axis, if not zero, then the button is limited by this, if Size is zero in that axis.
	/// </summary>
	public Vector2 MaximumSize;

#if UNITY_EDITOR
	/// <summary>
	/// Indicates whether the size fields are folded out or not in the inspector.
	/// </summary>
	[HideInInspector]
	public Boolean SizesInspectorFoldout;
#endif

	public override void Draw()
	{
		var GUIEnablingState = GUI.enabled;

		if (Condition != null)
			GUI.enabled = Condition();

		var Options = new List<GUILayoutOption>();
		if (Size.x != 0)
		{
			Options.Add(GUILayout.ExpandWidth(false));
			Options.Add(GUILayout.Width(Size.x));
		}
		else
		{
			Options.Add(GUILayout.ExpandWidth(true));
			if (MaximumSize.x != 0)
				Options.Add(GUILayout.MaxWidth(MaximumSize.x));
		}

		if (Size.y != 0)
		{
			Options.Add(GUILayout.ExpandHeight(false));
			Options.Add(GUILayout.Height(Size.y));
		}
		else
		{
			Options.Add(GUILayout.ExpandHeight(true));
			if (MaximumSize.y != 0)
				Options.Add(GUILayout.MaxHeight(MaximumSize.y));
		}

		if (GUILayout.Button(Text, GUIStyle, Options.ToArray()))
			Action();

		GUI.enabled = GUIEnablingState;
	}

	public override Vector2 GetControlSize()
	{
		return new GUIStyle(GUIStyle).CalcSize(new GUIContent(Text));
	}

#if UNITY_EDITOR

	public override void Initialize(GUIFrameControl Parent)
	{
		base.Initialize(Parent);

		SizesInspectorFoldout = true;
	}

	protected override String DefaultGUIStyle()
	{
		return "button";
	}

	protected override void CustomDrawInspector(SerializedObject SerializedObject)
	{
		base.CustomDrawInspector(SerializedObject);

		Text = EditorGUILayout.TextField("Text", Text);

		SizesInspectorFoldout = EditorGUILayout.Foldout(SizesInspectorFoldout, "Sizes");
		if (SizesInspectorFoldout)
		{
			StartTab();
			Size = EditorGUILayout.Vector2Field("Size", Size);
			MaximumSize = EditorGUILayout.Vector2Field("MaximumSize", MaximumSize);
			EndTab();
		}
	}

	public override List<String> GenerateCode()
	{
		var Result = base.GenerateCode();

		Result.Add(String.Format("\t\t{0}.Text = {1};", Name, String.IsNullOrEmpty(Text) ? "String.Empty" : String.Format("\"{0}\"", Text)));
		Result.Add(String.Format("\t\t{0}.Condition = {1};", Name, Condition != null ? Condition.ToString() : "null"));
		Result.Add(String.Format("\t\t{0}.Action = {1};", Name, Action != null ? Action.ToString() : "null"));
		Result.Add(String.Format("\t\t{0}.Size = new Vector2({1}F, {2}F);", Name, Size.x, Size.y));
		Result.Add(String.Format("\t\t{0}.MaximumSize = new Vector2({1}F, {2}F);", Name, MaximumSize.x, MaximumSize.y));
		Result.Add(String.Format("\t\t#if UNITY_EDITOR"));
		Result.Add(String.Format("\t\t\t{0}.SizesInspectorFoldout = {1};", Name, SizesInspectorFoldout.ToString().ToLower()));
		Result.Add(String.Format("\t\t#endif"));

		return Result;
	}

#endif
}