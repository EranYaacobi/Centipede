using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public partial class MainMenuGUIFrame : GUIFrame
{
	public void Reset()
	{
		InitializeComponents();
	}

	public void Start()
	{
		MultiplayerButton.Action = SwitchToFrame<MultiplayerMenuGUIFrame>;
		QuitButton.Action = Application.Quit;
	}
}
