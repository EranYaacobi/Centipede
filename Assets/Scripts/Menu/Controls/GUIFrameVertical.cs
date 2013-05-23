using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif

public class GUIFrameVertical : GUIFrameControl
{
	/// <summary>
	/// The controls inside the area.
	/// </summary>
	public List<GUIFrameControl> Controls;

	public override void Draw()
	{
		if (String.IsNullOrEmpty(GUIStyle))
			GUILayout.BeginVertical();
		else
			GUILayout.BeginVertical(new GUIStyle(GUIStyle));

		if (Controls != null)
		{
			foreach (var Control in Controls)
			{
				if (Control != null)
					Control.Draw();
			}
		}

		GUILayout.EndVertical();
	}

	public override Vector2 GetControlSize()
	{
		var ParentSize = new Vector2(Screen.width, Screen.height);
		if (Parent != null)
			ParentSize = Parent.GetControlSize();

		return ParentSize;
	}

#if UNITY_EDITOR

	protected override void CustomDrawInspector(SerializedObject SerializedObject)
	{
		base.CustomDrawInspector(SerializedObject);

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

		Result.AddRange(GenerateControlsListCode(Controls, "Controls"));

		return Result;
	}

#endif
}