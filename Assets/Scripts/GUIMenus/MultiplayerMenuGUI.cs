using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GUIDesigner.Scripts;
using GUIDesigner.Scripts.Controls;

public partial class MultiplayerMenuGUI : MonoBehaviour
{
	/// <summary>
	/// Performs custom initialization of the GUIFrame.
	/// Use this function instead of Start().
	/// </summary>
	private void Initialize()
	{
		// Setting buttons.
		CreateRoomButton.Condition = () => (PhotonNetwork.room == null);
		JoinRoomButton.Condition = () => ((GamesTable.SelectedIndex != -1) && (PhotonNetwork.room == null));
	}

	private void OnEnable()
	{
		if (!PhotonNetwork.connected)
			PhotonNetwork.ConnectUsingSettings("v1.0");

		RegisterCustomTypes.Register();

		PhotonNetwork.automaticallySyncScene = true;
	}


	private void Update()
	{
		GamesTable.Items = PhotonNetwork.GetRoomList().Select(RoomInfo => new MultiplayerGUITableItem(RoomInfo)).ToList();
	}

	private void SwitchBack(GUIControlEventArgs EventArgs)
	{
		GameObject.Find("MainMenu").GetComponent<GUIFrame>().enabled = true;

		PhotonNetwork.Disconnect();
		MasterServer.UnregisterHost();

		GetComponent<GUIFrame>().enabled = false;
	}

	private void CreateRoom(GUIControlEventArgs EventArgs)
	{
		PhotonNetwork.CreateRoom(String.Format("Test_{0}", UnityEngine.Random.Range(0, Int16.MaxValue)), true, true, 10,
		                         new Hashtable {{"Description", "This is a test."}}, new []{"Description"});
	}

	private void JoinRoom(GUIControlEventArgs EventArgs)
	{
		if ((GamesTable.SelectedIndex >= 0) && (GamesTable.SelectedIndex < GamesTable.Items.Count))
		{
			var GameInfo = GamesTable.Items[GamesTable.SelectedIndex];
			PhotonNetwork.JoinRoom(GameInfo.Name);
		}
	}

	private void OnPhotonPlayerConnected(PhotonPlayer Player)
	{
		if (PhotonNetwork.isMasterClient)
			PhotonNetwork.LoadLevel(1);
	}
}
