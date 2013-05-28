using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayers : Photon.MonoBehaviour
{
	/// <summary>
	/// The game-object that should be spawned for each player.
	/// </summary>
	public GameObject SpawnedGameObject;

	/// <summary>
	/// The minimum interval between spawnings.
	/// </summary>
	public Single SpawnInterval;

	/// <summary>
	/// The amount of player that reported that they've finished loading.
	/// </summary>
	private Int32 PlayersReadyCount;

	/// <summary>
	/// Indicates that the game was started.
	/// </summary>
	private Boolean GameStarted;

	private void Start()
	{
		GameStarted = false;

		if (!PhotonNetwork.connected)
		{
			Debug.Log("Offline mode");
			PhotonNetwork.offlineMode = true;
			PhotonNetwork.CreateRoom("Playground");
		}

		foreach (var Spawn in transform.Cast<Transform>())
		{
			var SpawnScript = Spawn.gameObject.AddComponent<SpawnPosition>();
			SpawnScript.SpawnInterval = SpawnInterval;
		}
		
		photonView.RPC("LoadedLevel", PhotonTargets.MasterClient);
	}

	[RPC]
	private void LoadedLevel()
	{
		PlayersReadyCount += 1;

		if ((!GameStarted) && (PlayersReadyCount == PhotonNetwork.playerList.Length))
		{
			StartSpawning();
			GameStarted = true;
		}
	}

	private void StartSpawning()
	{
		Debug.Log(String.Format("Starting spawning (PlayerCount: {0}", PhotonNetwork.playerList.Length));
		var SpawnPositions = SpawnPosition.SpawnPositions.ToList();

		foreach (var Player in PhotonNetwork.playerList)
		{
			var Spawn = SpawnPositions[UnityEngine.Random.Range(0, SpawnPositions.Count)];
			Spawn.Ready = false;
			SpawnPositions.Remove(Spawn);

			CreatePlayer(Spawn.transform.position, Spawn.transform.rotation, Player);
		}
	}

	private void OnPhotonPlayerConnected(PhotonPlayer Player)
	{
		if (PhotonNetwork.isMasterClient)
		{
			if (GameStarted)
			{
				Debug.Log("Player joined!");
				var Spawn = SpawnPosition.SpawnPositions[UnityEngine.Random.Range(0, SpawnPosition.SpawnPositions.Count)];
				Spawn.Ready = false;

				CreatePlayer(Spawn.transform.position, Spawn.transform.rotation, Player);
			}
		}
	}

	/// <summary>
	/// Respawns the player.
	/// </summary>
	/// <param name="Player"></param>
	/// <param name="PreviousCentipede"> </param>
	public void Respawn(PhotonPlayer Player, Int32 PreviousCentipede)
	{
		if (PhotonNetwork.isMasterClient)
		{
			PhotonNetwork.Destroy(PhotonView.Find(PreviousCentipede).gameObject);

			var AvailableSpawnPositions = SpawnPosition.SpawnPositions.Where(Spawn => Spawn.Ready).ToList();
			var ChosenSpawn = AvailableSpawnPositions[UnityEngine.Random.Range(0, AvailableSpawnPositions.Count)];
			ChosenSpawn.Ready = false;

			CreatePlayer(ChosenSpawn.transform.position, ChosenSpawn.transform.rotation, Player);
		}
	}

	/// <summary>
	/// Creates a player's centipede.
	/// </summary>
	/// <param name="Position"></param>
	/// <param name="Rotation"></param>
	/// <param name="Owner"></param>
	private void CreatePlayer(Vector3 Position, Quaternion Rotation, PhotonPlayer Owner)
	{
		if (PhotonNetwork.isMasterClient)
		{
			var Player = PhotonNetwork.InstantiateSceneObject(SpawnedGameObject.name, Position, Rotation, 0, null);

			Player.GetComponent<CentipedeNetworkObject>().Initialize(new CentipedeNetworkObjectData(null, Owner));
		}
	}
}