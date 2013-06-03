using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GUIDesigner;
using GUIDesigner.Scripts;
using GUIDesigner.Scripts.Controls;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(GUIFrame))]
public class GUIFrameEditor : Editor
{
	#region Constants

	/// <summary>
	/// The minimum size of the ToolsWindow.
	/// </summary>
	private static readonly Vector2 ToolsWindowSize = new Vector2(130, 200);

	/// <summary>
	/// The allocated (relative) area for the GUIFrameEditor, from the screen.
	/// </summary>
	private static readonly Rect AllocatedRelativeArea = new Rect(0, 0, 1, 1);

	#endregion

	#region Variables

	/// <summary>
	/// The drawn GUI.
	/// </summary>
	private GUIFrame GUIFrame;

	/// <summary>
	/// The previous tool.
	/// Used in order to restore the tool used before editing the GUIFrame.
	/// </summary>
	private Tool PreviousTool;

	#endregion

	#region Functions

	private void OnEnable()
	{
		if (Application.isPlaying)
			return;

		if (target != null)
		{
			GUIFrame = (GUIFrame)target;

			if (GUIFrame.Area == null)
			{
				GUIFrame.Area = GUIFrame.gameObject.AddComponent<GUIArea>();
				GUIFrame.Area.Initialize(null);
				GUIFrame.Area.Margins = new GUIControlMargins(0.1F, 0.1F, 0.1F, 0.1F);
				GUIFrame.Area.Dock = GUIControlDock.Full;
			}
		}
		PreviousTool = Tools.current;
		Tools.current = Tool.View;
		Tools.viewTool = ViewTool.FPS;
	}

	private void OnDisable()
	{
		Tools.current = PreviousTool;
	}

	public override void OnInspectorGUI()
	{
		EditorGUIUtility.LookLikeInspector();
		DrawDefaultInspector();

		if (Application.isPlaying)
			return;

		if (GUILayout.Button("Generate Code"))
		{
			GenerateCode();
		}

		CreateMissingEditors();
	}

	/// <summary>
	/// Generates the scripts that will be used to assign functions for the GUIControls.
	/// </summary>
	private void GenerateCode()
	{
		var ClassName = String.Format("{0}GUI", GUIFrame.Name);
		var Declarations = GUIFrame.Area.GenerateDeclarations();
		var Code = GUIFrame.Area.GenerateCode();

		// Creating the menus folder, if it doesn't exist.
		if (!Directory.Exists(GUIDesignerProperties.MenusFolder))
			Directory.CreateDirectory(GUIDesignerProperties.MenusFolder);

		var DesignerFile = String.Format("{0}/{1}.Designer.cs", GUIDesignerProperties.MenusFolder, ClassName);

		// Generating auto-generated file.
		var DesignerFileCode = new List<String>();
		DesignerFileCode.Add("using System;");
		DesignerFileCode.Add("using System.Linq;");
		DesignerFileCode.Add("using System.Collections;");
		DesignerFileCode.Add("using System.Collections.Generic;");
		DesignerFileCode.Add("using UnityEngine;");
		DesignerFileCode.Add("using GUIDesigner.Scripts;");
		DesignerFileCode.Add("using GUIDesigner.Scripts.Controls;");
		DesignerFileCode.Add(String.Empty);
		DesignerFileCode.Add(String.Format("public partial class {0}", ClassName));
		DesignerFileCode.Add("{");
		DesignerFileCode.AddRange(Declarations);
		DesignerFileCode.Add(String.Empty);
		DesignerFileCode.Add(String.Format("\tprivate void Start()"));
		DesignerFileCode.Add("\t{");
		DesignerFileCode.Add("\t\tvar Controls = GetComponents<GUIControl>().ToList();");
		DesignerFileCode.AddRange(Code);
		DesignerFileCode.Add("\t\tInitialize();");
		DesignerFileCode.Add("\t}");
		DesignerFileCode.Add("}");

		File.WriteAllLines(DesignerFile, DesignerFileCode.ToArray());
		var ClassFile = String.Format("{0}/{1}.cs", GUIDesignerProperties.MenusFolder, ClassName);
		// Generate actual class if it doesn't exist.
		if (!File.Exists(ClassFile))
		{
			var ClassFileCode = new List<String>
				{
				    "using System;",
				    "using System.Collections.Generic;",
				    "using UnityEngine;",
					"using GUIDesigner.Scripts;",
					"using GUIDesigner.Scripts.Controls;",
				    String.Empty,
				    String.Format("public partial class {0} : MonoBehaviour", ClassName),
				    "{",
					"\t/// <summary>",
					"\t/// Performs custom initialization of the GUIFrame.",
					"\t/// Use this function instead of Start().",
					"\t/// </summary>",
					"\tprivate void Initialize()",
					"\t{",
					"\t}",
				    "}"
				};

			File.WriteAllLines(ClassFile, ClassFileCode.ToArray());
		}

		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}

	/// <summary>
	/// Creates empty Editors for all GUIControls types that don't have one yet.
	/// This is needed becuase unity doesn't support inheritence in Editors' types.
	/// </summary>
	private void CreateMissingEditors()
	{
		foreach (var ControlType in GetGUIControlTypes())
		{
			if (ControlType.IsGenericType)
				continue;
			var EditorFile = String.Format("{0}/{1}Editor.cs", GUIDesignerProperties.EditorsFolder, ControlType.Name);
			if (!File.Exists(EditorFile))
			{
				var EditorFileCode = new List<String>
				{
					"using UnityEngine;",
					"using UnityEditor;",
					"using GUIDesigner.Scripts.Controls;",
					String.Empty,
					String.Format("[CustomEditor(typeof({0}))]", ControlType.Name),
					String.Format("public class {0}Editor : GUIControlEditor", ControlType.Name),
					"{",
					"}"
				};

				File.WriteAllLines(EditorFile, EditorFileCode.ToArray());
			}
		}
	}

	/// <summary>
	/// Gets all non-abstract types that inherit from GUIControl.
	/// </summary>
	/// <returns></returns>
	private IEnumerable<Type> GetGUIControlTypes()
	{
		return Assembly.GetAssembly(typeof(GUIControl)).GetTypes().Where(Type => Type.IsSubclassOf(typeof(GUIControl)) && (!Type.IsAbstract));
	}

	/// <summary>
	/// Lets the Editor handle an event in the scene view.
	/// </summary>
	public void OnSceneGUI()
	{
		if (Application.isPlaying)
			return;

		if (!GUIFrame.enabled)
			return;

		ShowToolsWindow();

		GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));

		GUIFrame.Draw();
		GUIFrame.Area.ShowInInspector(GUIFrame.SelectedControl);

		GUI.EndGroup();

		HandleMouseInput();
		HandleKeyboardInput();

		RemoveUnusedGUIObjects();

		SceneView.RepaintAll();
	}

	/// <summary>
	/// Shows the tools window.
	/// </summary>
	private void ShowToolsWindow()
	{
		var SelectedContainer = GUIFrame.SelectedControl as GUIContainer;
		if (SelectedContainer != null)
		{
			GUIFrame.ToolWindowPosition = GUILayout.Window(0, GUIFrame.ToolWindowPosition, ToolsWindow, "GUIFrameEditor");
			GUIFrame.ToolWindowPosition.width = ToolsWindowSize.x;
			GUIFrame.ToolWindowPosition.height = ToolsWindowSize.y;
		}
	}

	/// <summary>
	/// Handles mouse input from the user.
	/// </summary>
	private void HandleMouseInput()
	{
		var MousePosition = Event.current.mousePosition;

		if (Event.current.isMouse)
		{
			switch ((MouseButton)Event.current.button)
			{
				case MouseButton.Left:
					switch (Event.current.type)
					{
						case EventType.MouseDown:
							// Updating mouse action for the current control.
							if (GUIFrame.SelectedControl != null)
								GUIFrame.MouseAction = GUIFrame.SelectedControl.GetAction(AllocatedRelativeArea, MousePosition);

							// If there is no action, it means that the mouse is not currently on that control, so
							// getting a new control.
							if (GUIFrame.MouseAction == GUIControl.MouseAction.None)
								GUIFrame.SelectedControl = GUIFrame.Area.Select(AllocatedRelativeArea, MousePosition);

							// If the currently selected control is a container, checking whether we actually selected
							//e on of its children controls.
							if ((GUIFrame.SelectedControl != null) && (GUIFrame.SelectedControl is GUIContainer))
								GUIFrame.SelectedControl = GUIFrame.SelectedControl.Select(AllocatedRelativeArea, MousePosition);

							// Getting an action for the control (as the control may have changed).
							if (GUIFrame.SelectedControl != null)
								GUIFrame.MouseAction = GUIFrame.SelectedControl.GetAction(AllocatedRelativeArea, MousePosition);

							break;

						case EventType.MouseDrag:
							if (GUIFrame.SelectedControl != null)
							{
								GUIFrame.SelectedControl.PerformAction(Event.current.delta, GUIFrame.MouseAction);
								if (GUIFrame.MouseAction == GUIControl.MouseAction.Drag)
								{
									if (GUIFrame.SelectedControl.Parent != null)
									{
										if (!GUIFrame.SelectedControl.Parent.GetArea(AllocatedRelativeArea).Contains(MousePosition))
											GUIFrame.MouseAction = GUIControl.MouseAction.DragAndDrop;
									}
								}

								if (GUIFrame.MouseAction == GUIControl.MouseAction.DragAndDrop)
								{
									if (GUIFrame.SelectedControl.GetArea(AllocatedRelativeArea).Contains(MousePosition))
										GUIFrame.MouseAction = GUIControl.MouseAction.Drag;
								}
							}
							break;

						case EventType.MouseUp:
							if (GUIFrame.MouseAction == GUIControl.MouseAction.DragAndDrop)
							{
								var Container = GUIFrame.Area.Select(AllocatedRelativeArea, MousePosition) as GUIContainer;
								if ((Container != null) && (Container != GUIFrame.SelectedControl))
								{
									var PreviousParentSize = GUIFrame.SelectedControl.Parent.GetArea(AllocatedRelativeArea);
									var NewParentSize = Container.GetArea(AllocatedRelativeArea);
									GUIFrame.SelectedControl.Parent.Controls.Remove(GUIFrame.SelectedControl);
									GUIFrame.SelectedControl.Parent = Container;
									Container.Controls.Add(GUIFrame.SelectedControl);
									// TODO: Bug: this is not accurate.
									var ParentArea = GUIFrame.SelectedControl.Parent.GetArea(AllocatedRelativeArea);
									var MouseRelativePosition = GUIFrame.SelectedControl.Parent.ToRelative(Event.current.mousePosition - new Vector2(ParentArea.x, ParentArea.y));
									GUIFrame.SelectedControl.RelativeArea.x = MouseRelativePosition.x;
									GUIFrame.SelectedControl.RelativeArea.y = MouseRelativePosition.y;
									GUIFrame.SelectedControl.RelativeArea.width *= (PreviousParentSize.width / NewParentSize.width);
									GUIFrame.SelectedControl.RelativeArea.height *= (PreviousParentSize.height / NewParentSize.height);
									GUIFrame.SelectedControl.PerformAction(Vector2.zero, GUIControl.MouseAction.None);
								}
							}
							GUIFrame.MouseAction = GUIControl.MouseAction.None;
							break;
					}
					break;

				case MouseButton.Right:
					switch (Event.current.type)
					{
						case EventType.MouseDown:
							GUIFrame.SelectedControl = null;
							GUIFrame.MouseAction = GUIControl.MouseAction.None;
							break;
					}
					break;
			}
		}

		// Updating cursor.
		var CursorMouseAction = GUIFrame.MouseAction;
		if (CursorMouseAction == GUIControl.MouseAction.None)
		{
			if (GUIFrame.SelectedControl != null)
			{
				CursorMouseAction = GUIFrame.SelectedControl.GetAction(AllocatedRelativeArea, MousePosition);
				if ((CursorMouseAction == GUIControl.MouseAction.None) && (GUIFrame.SelectedControl.Parent != null))
					CursorMouseAction = GUIFrame.SelectedControl.Parent.GetAction(AllocatedRelativeArea, MousePosition);
			}
		}


		var ScreenRect = new Rect(0, 0, Screen.width, Screen.height);
		switch (CursorMouseAction)
		{
			case GUIControl.MouseAction.Drag:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.MoveArrow);
				break;
			case GUIControl.MouseAction.ResizeUp:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.ResizeVertical);
				break;
			case GUIControl.MouseAction.ResizeDown:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.ResizeVertical);
				break;
			case GUIControl.MouseAction.ResizeLeft:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.ResizeHorizontal);
				break;
			case GUIControl.MouseAction.ResizeRight:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.ResizeHorizontal);
				break;
			case GUIControl.MouseAction.ResizeUpLeft:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.ResizeUpLeft);
				break;
			case GUIControl.MouseAction.ResizeUpRight:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.ResizeUpRight);
				break;
			case GUIControl.MouseAction.ResizeDownLeft:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.ResizeUpLeft);
				break;
			case GUIControl.MouseAction.ResizeDownRight:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.ResizeUpRight);
				break;
			case GUIControl.MouseAction.DragAndDrop:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.MoveArrow);
				break;
			case GUIControl.MouseAction.ColumnResize:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.ResizeHorizontal);
				break;
			case GUIControl.MouseAction.ColumnReorder:
				EditorGUIUtility.AddCursorRect(ScreenRect, MouseCursor.MoveArrow);
				break;
		}
	}

	/// <summary>
	/// Handles keyboard input from the user.
	/// </summary>
	private void HandleKeyboardInput()
	{
		if (Event.current.isKey)
		{
			if (Event.current.type == EventType.KeyDown)
			{
				switch (Event.current.keyCode)
				{
					case KeyCode.UpArrow:
						if (GUIFrame.SelectedControl != null)
							GUIFrame.SelectedControl.PerformAction(-Vector2.up, GUIControl.MouseAction.Drag);
						break;
					case KeyCode.LeftArrow:
						if (GUIFrame.SelectedControl != null)
							GUIFrame.SelectedControl.PerformAction(-Vector2.right, GUIControl.MouseAction.Drag);
						break;
					case KeyCode.RightArrow:
						if (GUIFrame.SelectedControl != null)
							GUIFrame.SelectedControl.PerformAction(Vector2.right, GUIControl.MouseAction.Drag);
						break;
					case KeyCode.DownArrow:
						if (GUIFrame.SelectedControl != null)
							GUIFrame.SelectedControl.PerformAction(Vector2.up, GUIControl.MouseAction.Drag);
						break;
					case KeyCode.Delete:
						if (GUIFrame.SelectedControl != null)
						{
							if (GUIFrame.SelectedControl != GUIFrame.Area)
							{
								GUIFrame.SelectedControl.Parent.Controls.Remove(GUIFrame.SelectedControl);
								GUIFrame.SelectedControl.Parent = null;
							}
							Event.current.Use();
						}
						break;
					case KeyCode.D:
						if (Event.current.modifiers == EventModifiers.Shift)
						{
							if ((GUIFrame.SelectedControl != null) && (GUIFrame.SelectedControl != GUIFrame.Area))
							{
								var CreatedControl = GUIFrame.SelectedControl.Duplicate();
								CreatedControl.Parent = GUIFrame.SelectedControl.Parent;
								CreatedControl.Parent.Controls.Add(CreatedControl);
								GUIFrame.SelectedControl = CreatedControl;
							}
							Event.current.Use();
						}
						break;
					case KeyCode.C:
						if (Event.current.modifiers == EventModifiers.Shift)
						{
							if ((GUIFrame.SelectedControl != null) && (GUIFrame.SelectedControl != GUIFrame.Area))
							{
								if (GUIFrame.SelectedControl.Dock == GUIControlDock.Full)
								{
									var ParentControl = GUIFrame.SelectedControl.Parent;
									while ((ParentControl != null) && (ParentControl.Dock == GUIControlDock.Full))
										ParentControl = ParentControl.Parent;

									if ((ParentControl != null) && (ParentControl != GUIFrame.Area))
										GUIFrame.CopiedControl = ParentControl;
									else
										GUIFrame.CopiedControl = null;
								}
								else
								{
									GUIFrame.CopiedControl = GUIFrame.SelectedControl;
								}
							}

							Event.current.Use();
						}
						break;
					case KeyCode.V:
						if (Event.current.modifiers == EventModifiers.Shift)
						{
							var Container = GUIFrame.SelectedControl as GUIContainer;
							if ((Container != null) && (GUIFrame.CopiedControl))
							{
								var CreatedControl = GUIFrame.CopiedControl.Duplicate();
								CreatedControl.Parent = Container;
								CreatedControl.Parent.Controls.Add(CreatedControl);
								GUIFrame.SelectedControl = CreatedControl;
							}
							Event.current.Use();
						}
						break;
					case KeyCode.Escape:
						GUIFrame.SelectedControl = null;
						GUIFrame.MouseAction = GUIControl.MouseAction.None;
						Event.current.Use();
						break;
				}
			}
		}
	}

	/// <summary>
	/// Removes GUIControls that has no parent.
	/// </summary>
	private void RemoveUnusedGUIObjects()
	{
		if (Application.isPlaying)
			return;

		if (!GUIFrame.enabled)
			return;

		var Controls = GUIFrame.GetComponents<GUIControl>().ToList();
		Controls.Remove(GUIFrame.Area);

		foreach (var Control in Controls.Where(Control => Control.Parent == null))
			DestroyImmediate(Control);
	}

	/// <summary>
	/// The ToolsWindow function.
	/// </summary>
	/// <param name="WindowID"></param>
	private void ToolsWindow(Int32 WindowID)
	{
		var SelectedContainer = GUIFrame.SelectedControl as GUIContainer;
		if (SelectedContainer != null)
		{
			foreach (var ControlType in GetGUIControlTypes())
			{
				if (ControlType.IsGenericType)
					continue;
				var ControlName = ControlType.Name;
				// Getting rid of the 'GUI' text at the start.
				ControlName = Regex.Replace(ControlName, "^GUI", "");
				// Adding spaces before capital letters.
				ControlName = Regex.Replace(ControlName, "([a-z])([A-Z])", "$1 $2");
				ControlName = ControlName.Replace("`1", "<T>");
				if (GUILayout.Button(ControlName))
				{
					var CreatedControl = GUIFrame.gameObject.AddComponent(ControlType) as GUIControl;
					CreatedControl.Initialize(SelectedContainer);
					SelectedContainer.Controls.Add(CreatedControl);
					GUIFrame.SelectedControl = CreatedControl;
				}
			}
		}

		GUI.DragWindow();
	}

	#endregion
}