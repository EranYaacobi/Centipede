using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif

/// <summary>
/// A base class for a GUI control.
/// <remarks>
/// All fields in this class (and in derived classes) must be public,
/// or it won't be serialized.
/// </remarks>
/// </summary>
public abstract class GUIFrameControl : ScriptableObject
{
#if UNITY_EDITOR
	/// <summary>
	/// The size of a tab-space in pixels, in the inspector.
	/// </summary>
	protected const Single TabSpace = 14;
#endif

	/// <summary>
	/// The name of the control.
	/// This can be used to search for it.
	/// </summary>
	public String Name;

	/// <summary>
	/// The name of the GUIstyle of the control.
	/// </summary>
	public String GUIStyle;

	/// <summary>
	/// The parent control of this control.
	/// </summary>
	[HideInInspector]
	public GUIFrameControl Parent;

#if UNITY_EDITOR
	/// <summary>
	/// Indicates whether the control is folded out or not in the inspector.
	/// </summary>
	[HideInInspector]
	public Boolean InspectorFoldout;
#endif

	/// <summary>
	/// Draws the control.
	/// </summary>
	public abstract void Draw();

	/// <summary>
	/// Returns the size of the control.
	/// </summary>
	/// <returns></returns>
	public abstract Vector2 GetControlSize();

#if UNITY_EDITOR

	/// <summary>
	/// Initializes the control.
	/// This function should be called only from the editor.
	/// </summary>
	public virtual void Initialize(GUIFrameControl Parent)
	{
		this.Parent = Parent;

		GUIStyle = DefaultGUIStyle();
		Name = String.Format("{0}_{1}", GetType().Name, UnityEngine.Random.Range(0, Int32.MaxValue));
		InspectorFoldout = true;
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
	/// Draws the control inspector.
	/// </summary>
	public void DrawInspector()
	{
		var SerializedObject = new SerializedObject(this);
		SerializedObject.Update();

		if (Parent == null)
			StartTab();

		Name = EditorGUILayout.TextField("Name", Name);
		CustomDrawInspector(SerializedObject);

		if (Parent == null)
			EndTab();

		SerializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// Draws custom fields.
	/// </summary>
	/// <param name="SerializedObject"></param>
	protected virtual void CustomDrawInspector(SerializedObject SerializedObject)
	{
		EditorGUILayout.PropertyField(SerializedObject.FindProperty("GUIStyle"), true);
	}

	/// <summary>
	/// Generates the declarations of the control.
	/// </summary>
	/// <returns></returns>
	public virtual List<String> GenerateDeclarations()
	{
		return new List<String> {
			String.Format("\t[HideInInspector]"),
			String.Format("\tpublic {0} {1};", GetType().Name, Name) };
	}

	/// <summary>
	/// Generates the code that customizes the control.
	/// </summary>
	public virtual List<String> GenerateCode()
	{
		return new List<String> {
			String.Format("\t\t// Control {0}:", Name),
			String.Format("\t\t{0} = ScriptableObject.CreateInstance<{1}>();", Name, GetType().Name),
			String.Format("\t\t{0}.Name = \"{0}\";", Name),
			String.Format("\t\t{0}.GUIStyle = \"{1}\";", Name, GUIStyle),
			String.Format("\t\t{0}.Parent = {1};", Name, Parent ? Parent.Name : "null"),
			String.Format("\t\t#if UNITY_EDITOR"),
			String.Format("\t\t\t{0}.InspectorFoldout = {1};", Name, InspectorFoldout.ToString().ToLower()),
			String.Format("\t\t#endif")
		};
	}

	/// <summary>
	/// Draws a controls' count field, and returns the new list.
	/// </summary>
	/// <param name="Controls"></param>
	/// <returns></returns>
	protected List<GUIFrameControl> ControlsCountField(List<GUIFrameControl> Controls)
	{
		var NewControlsCount = EditorGUILayout.IntField("Controls", Controls.Count);
		if (NewControlsCount < Controls.Count)
		{
			while (NewControlsCount < Controls.Count)
			{
				var RemovedControl = Controls[Controls.Count - 1];
				Controls.RemoveAt(Controls.Count - 1);
				DestroyImmediate(RemovedControl);
			}
				
		}
		else if (NewControlsCount > Controls.Count)
		{
			for (var i = Controls.Count; i < NewControlsCount; i++)
			{
				var NewControl = Controls.LastOrDefault();
				if (NewControl != null)
					Controls.Add(Instantiate(NewControl) as GUIFrameControl);
				else
					Controls.Add(null);
			}
		}

		return Controls;
	}

	/// <summary>
	/// Draws a control field, and returns its new value.
	/// </summary>
	/// <param name="GuiFrameControl"></param>
	/// <returns></returns>
	protected GUIFrameControl ControlField(GUIFrameControl GuiFrameControl)
	{
		var PreviousControl = GuiFrameControl;
		var AddedTab = false;

		MonoScript ControlScript = null;
		if (GuiFrameControl != null)
		{
			ControlScript = MonoScript.FromScriptableObject(GuiFrameControl);
			GuiFrameControl.InspectorFoldout = EditorGUILayout.Foldout(GuiFrameControl.InspectorFoldout, GuiFrameControl.Name);

			if (!GuiFrameControl.InspectorFoldout)
				return GuiFrameControl;

			StartTab();
			AddedTab = true;
		}

		StartTab();

		var NewScript = EditorGUILayout.ObjectField("Control", ControlScript, typeof(MonoScript), false) as MonoScript;
		if (NewScript != null)
		{
			if ((GuiFrameControl == null) || (ControlScript != NewScript))
			{
				var NewControlClass = NewScript.GetClass();
				if (NewControlClass.IsSubclassOf(typeof(GUIFrameControl)))
				{
					GuiFrameControl = CreateInstance(NewControlClass) as GUIFrameControl;
					GuiFrameControl.Initialize(this);
				}
				else
					GuiFrameControl = null;
			}
		}
		else
		{
			GuiFrameControl = null;
		}

		if (PreviousControl != GuiFrameControl)
			DestroyImmediate(PreviousControl);

		if (GuiFrameControl != null)
		{
			if (GuiFrameControl.InspectorFoldout)
			{
				if (!AddedTab)
					StartTab();
				GuiFrameControl.DrawInspector();
				EndTab();
			}
		}

		EndTab();

		return GuiFrameControl;
	}

	/// <summary>
	/// Starts a horizontal tab.
	/// </summary>
	protected void StartTab()
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(TabSpace);
		EditorGUILayout.BeginVertical();
	}

	/// <summary>
	/// Ends a horizontal tab.
	/// </summary>
	protected void EndTab()
	{
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}

	/// <summary>
	/// Generates the code for initializing a controls list.
	/// </summary>
	/// <returns></returns>
	protected List<String> GenerateControlsListCode(List<GUIFrameControl> Controls, String ListName)
	{
		var Result = new List<String> { String.Format("\t\t{0}.{1} = new List<GUIFrameControl>();", Name, ListName) };
		foreach (var Control in Controls)
		{
			if (Control == null)
				Result.Add(String.Format("\t\t{0}.{1}.Add(null);", Name, ListName));
			else
			{
				Result.AddRange(Control.GenerateCode());
				Result.Add(String.Format("\t\t{0}.{1}.Add({2});", Name, ListName, Control.Name));
			}
		}

		return Result;
	}

#endif

}