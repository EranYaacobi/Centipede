using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GUIDesigner.Scripts;
using GUIDesigner.Scripts.Controls;

public partial class MainMenuGUI
{
	public GUIArea GUIArea_1157020965;
	public GUIArea GUIArea_977234804;
	public GUIButton GUIButton_2085473440;
	public GUIButton GUIButton_955361902;
	public GUIButton GUIButton_1226782659;
	public GUIButton GUIButton_444708301;
	public GUIButton GUIButton_790021684;
	public GUIButton GUIButton_520576317;

	private void Start()
	{
		var Controls = GetComponents<GUIControl>().ToList();
		GUIArea_1157020965 = (GUIArea)Controls.First(Control => Control.Name == "GUIArea_1157020965");
		GUIArea_977234804 = (GUIArea)Controls.First(Control => Control.Name == "GUIArea_977234804");
		GUIButton_2085473440 = (GUIButton)Controls.First(Control => Control.Name == "GUIButton_2085473440");
		GUIButton_955361902 = (GUIButton)Controls.First(Control => Control.Name == "GUIButton_955361902");
		GUIButton_1226782659 = (GUIButton)Controls.First(Control => Control.Name == "GUIButton_1226782659");
		GUIButton_444708301 = (GUIButton)Controls.First(Control => Control.Name == "GUIButton_444708301");
		GUIButton_444708301.Click += Multiplayer;
		GUIButton_790021684 = (GUIButton)Controls.First(Control => Control.Name == "GUIButton_790021684");
		GUIButton_520576317 = (GUIButton)Controls.First(Control => Control.Name == "GUIButton_520576317");
		GUIButton_520576317.Click += Quit;
		Initialize();
	}
}
