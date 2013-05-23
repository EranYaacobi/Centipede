using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(GUIFrame))]
public class GUIFrameEditor : Editor
{
	private readonly String MenuFolder = String.Format("{0}\\Scripts\\Menu\\",Application.dataPath);
	private readonly String EditorFolder = String.Format("{0}\\Scripts\\Menu\\Editor\\", Application.dataPath);

	/// <summary>
	/// The drawn GUI.
	/// </summary>
	private GUIFrame GUIFrame;

	private void OnEnable()
	{
		GUIFrame = (GUIFrame)target;
		if (GUIFrame.Area == null)
		{
			GUIFrame.Area = CreateInstance<GUIFrameArea>();
			GUIFrame.Area.Initialize(null);
		}
	}

	public override void OnInspectorGUI()
	{
		// Drawing default inspector, mainly because otherwise there is a selection problem for an unknown reason.
		DrawDefaultInspector();

		GUIFrame.Area.DrawInspector();

		if (GUILayout.Button("Generate Code"))
		{
			var ClassName = String.Format("{0}GUIFrame", GUIFrame.Name);
			var Declarations = GUIFrame.Area.GenerateDeclarations();
			var Code = GUIFrame.Area.GenerateCode();

			var DesignerFile = String.Format("{0}{1}.Designer.cs", MenuFolder, ClassName);
			
			// Generating auto-generated file.
			var DesignerFileCode = new List<String>();
			DesignerFileCode.Add("using System;");
			DesignerFileCode.Add("using System.Collections;");
			DesignerFileCode.Add("using System.Collections.Generic;");
			DesignerFileCode.Add("using UnityEngine;");
			DesignerFileCode.Add(String.Format("public partial class {0}", ClassName));
			DesignerFileCode.Add("{");
			DesignerFileCode.AddRange(Declarations);
			DesignerFileCode.Add("\tpublic void InitializeComponents()");
			DesignerFileCode.Add("\t{");
			DesignerFileCode.AddRange(Code);
			DesignerFileCode.Add(String.Format("\t\tArea = {0};", GUIFrame.Area.Name));
			DesignerFileCode.Add(String.Format("\t\tName = \"{0}\";", GUIFrame.Name));
			DesignerFileCode.Add("\t}");
			DesignerFileCode.Add("}");

			File.WriteAllLines(DesignerFile, DesignerFileCode.ToArray());

			var ClassFile = String.Format("{0}{1}.cs", MenuFolder, ClassName);
			// Generate actual class if it doesn't exist.
			if (!File.Exists(ClassFile))
			{
				var ClassFileCode = new List<String>
				{
				    "using System;",
				    "using System.Collections.Generic;",
				    "using UnityEngine;",
				    String.Empty,
				    "[ExecuteInEditMode]",
				    String.Format("public partial class {0} : GUIFrame", ClassName),
				    "{",
				    String.Empty,
				    "\tprivate void Reset()",
				    "\t{",
				    "\t\tInitializeComponents();",
				    "\t}",
				    "}"
				};

				File.WriteAllLines(ClassFile, ClassFileCode.ToArray());
			}

			var EditorFile = String.Format("{0}{1}Editor.cs", EditorFolder, ClassName);
			// Generate editor class if it doesn't exist.
			if (!File.Exists(EditorFile))
			{
				var EditorFileCode = new List<String>
				{
					"using UnityEngine;",
					"using UnityEditor;",
					String.Empty,
					String.Format("[CustomEditor(typeof({0}))]", ClassName),
					String.Format("public class {0}Editor : GUIFrameEditor", ClassName),
					"{",
					"}"
				};

				File.WriteAllLines(EditorFile, EditorFileCode.ToArray());
			}

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			// Clearing the console, as there is an error in it, that claims (falsely as it seems, that ImportAsset with ForceSynchronousImport (always) crashes unity).
			//ClearLog();
		}
	}

	/// <summary>
	/// Clears the console, in a very ugly way.
	/// </summary>
	/*private static void ClearLog()
	{
		Assembly Assembly = Assembly.GetAssembly(typeof(SceneView));

		var Type = Assembly.GetType("UnityEditorInternal.LogEntries");
		var Method = Type.GetMethod("Clear");
		Method.Invoke(new object(), null);
	}*/

	/// <summary>
	/// Lets the Editor handle an event in the scene view.
	/// </summary>
	public void OnSceneGUI()
	{
		if (GUIFrame.enabled)
			GUIFrame.Area.Draw();
	}
}