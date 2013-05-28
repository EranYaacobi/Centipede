using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public partial class MultiplayerMenuGUIFrame : GUIFrame
{
	private void Reset()
	{
		InitializeComponents();
	}

	private void Start()
	{
		GamesList.Items = new List<GameInfo>();

		// Trying to connect to the Photon cloud.
		if (!PhotonNetwork.connected)
			PhotonNetwork.ConnectUsingSettings("v1.0");

		RegisterCustomTypes.Register();

		PhotonNetwork.automaticallySyncScene = true;

		HostButton.Action = Host;
		HostButton.Condition = () => (PhotonNetwork.room == null);
		ConnectButton.Action = Connect;
		ConnectButton.Condition = () => ((GamesList.Items.Count > 0) && (PhotonNetwork.room == null));
	}

	private void Update()
	{
		GamesList.Items = PhotonNetwork.GetRoomList().Select(RoomInfo => new GameInfo(RoomInfo)).ToList();
	}

	protected override void SwitchBack()
	{
		base.SwitchBack();

		PhotonNetwork.Disconnect();
		MasterServer.UnregisterHost();
	}

	/// <summary>
	/// Hosts a game.
	/// </summary>
	private void Host()
	{
		PhotonNetwork.CreateRoom(String.Format("Test_{0}", UnityEngine.Random.Range(0, Int16.MaxValue)));
	}

	/// <summary>
	/// Connects to the selected game.
	/// </summary>
	private void Connect()
	{
		var GameInfo = GamesList.GetSelectedItem<GameInfo>();
		PhotonNetwork.JoinRoom(GameInfo.RoomInfo.name);
	}

	private void OnPhotonPlayerConnected(PhotonPlayer Player)
	{
		if (PhotonNetwork.isMasterClient)
			PhotonNetwork.LoadLevel(1);
	}

	/// <summary>
	/// A wrapper for room info.
	/// </summary>
	private struct GameInfo
	{
		/// <summary>
		/// The game's room info.
		/// </summary>
		public readonly RoomInfo RoomInfo;

		public GameInfo(RoomInfo RoomInfo)
		{
			this.RoomInfo = RoomInfo;
		}

		public override string ToString()
		{
			return String.Format("{0}", RoomInfo.name);
		}
	}
}
