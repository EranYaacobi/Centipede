using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using GUIDesigner.Scripts.Controls;
using UnityEngine;

[Serializable]
public class MultiplayerGUITableItem : GUITableItem
{
	/// <summary>
	/// The name of the game.
	/// </summary>
	[GUITableAlignment(TextAnchor.MiddleLeft, false)]
	public String Name
	{
		get { return RoomInfo.name; }	
	}

	/// <summary>
	/// The game's description.
	/// </summary>
	[GUITableAlignment(TextAnchor.MiddleLeft, false)]
	public String Description
	{
		get { return RoomInfo.customProperties["Description"].ToString(); }
	}

	/// <summary>
	/// A more suitable view for the table of the players.
	/// </summary>
	[GUITableAlignment(TextAnchor.MiddleCenter)]
	public String Players
	{
		get { return String.Format("{0}/{1}", RoomInfo.playerCount, RoomInfo.maxPlayers); }
	}

	/// <summary>
	/// The room info associated with the game.
	/// </summary>
	[GUITableHideInTable]
	[HideInInspector]
	public readonly RoomInfo RoomInfo;

	public MultiplayerGUITableItem(RoomInfo RoomInfo)
	{
		this.RoomInfo = RoomInfo;
	}
}