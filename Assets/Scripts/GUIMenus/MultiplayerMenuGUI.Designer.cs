using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GUIDesigner.Scripts;
using GUIDesigner.Scripts.Controls;

public partial class MultiplayerMenuGUI
{
	public GUIArea MainArea;
	public GUIArea CenterArea;
	public GUIArea ButtonsArea;
	public GUIButton JoinRoomButton;
	public GUIButton CreateRoomButton;
	public GUIArea ListArea;
	public MultiplayerGUITable GamesTable;
	public GUIArea BottomArea;
	public GUIButton BackButton;

	private void Start()
	{
		var Controls = GetComponents<GUIControl>().ToList();
		MainArea = (GUIArea)Controls.First(Control => Control.Name == "MainArea");
		CenterArea = (GUIArea)Controls.First(Control => Control.Name == "CenterArea");
		ButtonsArea = (GUIArea)Controls.First(Control => Control.Name == "ButtonsArea");
		JoinRoomButton = (GUIButton)Controls.First(Control => Control.Name == "JoinRoomButton");
		JoinRoomButton.Click += JoinRoom;
		CreateRoomButton = (GUIButton)Controls.First(Control => Control.Name == "CreateRoomButton");
		CreateRoomButton.Click += CreateRoom;
		ListArea = (GUIArea)Controls.First(Control => Control.Name == "ListArea");
		GamesTable = (MultiplayerGUITable)Controls.First(Control => Control.Name == "GamesTable");
		BottomArea = (GUIArea)Controls.First(Control => Control.Name == "BottomArea");
		BackButton = (GUIButton)Controls.First(Control => Control.Name == "BackButton");
		BackButton.Click += SwitchBack;
		Initialize();
	}
}
