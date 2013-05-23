using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GUIFrameFlexibleSpace : GUIFrameControl
{
	/// <summary>
	/// The minimum size of the space, in pixel.
	/// </summary>
	public Single MinimumSize;

	/// <summary>
	/// The minimum size of the space, relative to its parent.
	/// </summary>
	public Single MinimumSizeRate;

	public override void Draw()
	{
		// TODO: Flexible space works only for the y-axis (regarding MinimumSizeRate).
		var ParentSize = Parent.GetControlSize();
		var Space = Mathf.Max(MinimumSize, MinimumSizeRate * ParentSize.y);
		GUILayout.Space(Space);
		GUILayout.FlexibleSpace();
	}

	public override Vector2 GetControlSize()
	{
		throw new NotSupportedException("Space control does not support the GetControlSize function");
	}

#if UNITY_EDITOR

	protected override void CustomDrawInspector(SerializedObject SerializedObject)
	{
		base.CustomDrawInspector(SerializedObject);

		MinimumSize = EditorGUILayout.FloatField("MinimumSize", MinimumSize);
		MinimumSizeRate = EditorGUILayout.FloatField("MinimumSizeRate", MinimumSizeRate);
	}

	public override List<String> GenerateCode()
	{
		var Result = base.GenerateCode();

		Result.Add(String.Format("\t\t{0}.MinimumSize = {1}F;", Name, MinimumSize));
		Result.Add(String.Format("\t\t{0}.MinimumSizeRate = {1}F;", Name, MinimumSizeRate));

		return Result;
	}

#endif

}