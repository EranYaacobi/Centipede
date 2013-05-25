using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayers : Photon.MonoBehaviour
{
	/// <summary>
	/// The game-object that should be spawned for each player.
	/// </summary>
	public GameObject SpawnedGameObject;

	private Int32 PlayersReadyCount;

	private void Start()
	{
		if (!PhotonNetwork.connected)
		{
			Debug.Log("Offline mode");
			PhotonNetwork.offlineMode = true;
			PhotonNetwork.CreateRoom("Playground");
		}
		
		photonView.RPC("LoadedLevel", PhotonTargets.MasterClient);
	}

	[RPC]
	private void LoadedLevel()
	{
		PlayersReadyCount += 1;

		if (PlayersReadyCount == PhotonNetwork.playerList.Length)
			StartSpawning();
	}

	private void StartSpawning()
	{
		if (PhotonNetwork.isMasterClient)
		{
			foreach (var PhotonPlayer in PhotonNetwork.playerList)
			{
				var SpawnPosition = gameObject.transform.GetChild(UnityEngine.Random.Range(0, gameObject.transform.GetChildCount()));

				photonView.RPC("SpawnPlayer", PhotonTargets.AllBuffered, SpawnPosition.position, SpawnPosition.rotation, PhotonPlayer, PhotonNetwork.AllocateViewID(0));

				// Destroying the spawning position, so it won't be used again.
				DestroyImmediate(SpawnPosition.gameObject);
			}
		}
	}

	[RPC]
	public void SpawnPlayer(Vector3 Position, Quaternion Rotation, PhotonPlayer Owner, Int32 NetworkViewID)
	{
		Debug.Log("Created player");
		var Player = Instantiate(SpawnedGameObject, Position, Rotation) as GameObject;		
		Player.GetPhotonView().viewID = NetworkViewID;
		Player.GetComponent<PlayerInput>().Initialize(Owner);
		if (Owner == PhotonNetwork.player)
			Camera.mainCamera.GetComponent<LookOnGameObject>().GameObject = Player;

		var CentipedeBuilder = Player.GetComponent<CentipedeBuilder>();
			
		CentipedeBuilder.Owner = Owner;
		Common.Assert(CentipedeBuilder.Legs.Length%2 == 0);
		if (PhotonNetwork.isMasterClient)
			CentipedeBuilder.CreateCentipede();
	}
}