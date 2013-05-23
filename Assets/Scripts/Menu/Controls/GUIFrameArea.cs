using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif

public class GUIFrameArea : GUIFrameControl
{
	/// <summary>
	/// The margins of the area, relative to its parent's size.
	/// </summary>
	public Vector2 RelativeMargins = new Vector2(0.1F, 0.1F);

	/// <summary>
	/// The maximum margins of the area, in pixels.
	/// </summary>
	public Vector2 MaximumMargins = new Vector2(100, 100);

	/// <summary>
	/// The controls inside the area.
	/// </summary>
	public List<GUIFrameControl> Controls;

#if UNITY_EDITOR
	/// <summary>
	/// Indicates whether the margins are folded out or not in the inspector.
	/// </summary>
	[HideInInspector]
	public Boolean MarginsInspectorFoldout;
#endif

	public override void Draw()
	{
		var Area = GetControlArea();
		GUILayout.BeginArea(Area, new GUIStyle(GUIStyle));

		if (Controls != null)
		{
			foreach (var Control in Controls)
			{
				if (Control != null)
					Control.Draw();
			}
		}

		GUILayout.EndArea();
	}

	public override Vector2 GetControlSize()
	{
		var Area = GetControlArea();

		return new Vector2(Area.width, Area.height);
	}

	private Rect GetControlArea()
	{
		var ParentSize = new Vector2(Screen.width, Screen.height);
		if (Parent != null)			
			ParentSize = Parent.GetControlSize();

		var TopLeft = new Vector2(
			Mathf.Min(RelativeMargins.x * ParentSize.x, MaximumMargins.x),
			Mathf.Min(RelativeMargins.y * ParentSize.y, MaximumMargins.y));

		var BottomRight = new Vector2(
			Mathf.Max(ParentSize.x * (1 - RelativeMargins.x), ParentSize.x - MaximumMargins.x),
			Mathf.Max(ParentSize.y * (1 - RelativeMargins.y), ParentSize.y - MaximumMargins.y));

		return new Rect(TopLeft.x, TopLeft.y, BottomRight.x - TopLeft.x, BottomRight.y - TopLeft.y);
	}

#if UNITY_EDITOR

	public override void Initialize(GUIFrameControl Parent)
	{
		base.Initialize(Parent);

		MarginsInspectorFoldout = true;
	}

	protected override String DefaultGUIStyle()
	{
		return "box";
	}

	protected override void CustomDrawInspector(SerializedObject SerializedObject)
	{
		base.CustomDrawInspector(SerializedObject);

		MarginsInspectorFoldout = EditorGUILayout.Foldout(MarginsInspectorFoldout, "Margins");
		if (MarginsInspectorFoldout)
		{
			StartTab();
			RelativeMargins = EditorGUILayout.Vector2Field("Relative Margins", RelativeMargins);
			MaximumMargins = EditorGUILayout.Vector2Field("Maximum Margins", MaximumMargins);
			EndTab();
		}

		Controls = ControlsCountField(Controls);

		StartTab();

		for (int Index = 0; Index < Controls.Count; Index++)
			Controls[Index] = ControlField(Controls[Index]);

		EndTab();
	}

	public override List<String> GenerateDeclarations()
	{
		var Result = base.GenerateDeclarations();

		foreach (var Control in Controls)
		{
			if (Control != null)
				Result.AddRange(Control.GenerateDeclarations());
		}

		return Result;
	}

	public override List<String> GenerateCode()
	{
		var Result = base.GenerateCode();

		Result.Add(String.Format("\t\t{0}.RelativeMargins = new Vector2({1}F, {2}F);", Name, RelativeMargins.x, RelativeMargins.y));
		Result.Add(String.Format("\t\t{0}.MaximumMargins = new Vector2({1}F, {2}F);", Name, MaximumMargins.x, MaximumMargins.y));
		Result.Add(String.Format("\t\t#if UNITY_EDITOR"));
		Result.Add(String.Format("\t\t\t{0}.MarginsInspectorFoldout = {1};", Name, MarginsInspectorFoldout.ToString().ToLower()));
		Result.Add(String.Format("\t\t#endif"));

		Result.AddRange(GenerateControlsListCode(Controls, "Controls"));
		
		return Result;
	}

#endif
}