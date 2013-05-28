using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPosition : MonoBehaviour
{
	public static List<SpawnPosition> SpawnPositions = new List<SpawnPosition>();

	/// <summary>
	/// The minimum interval between spawnings.
	/// </summary>
	public Single SpawnInterval;

	private Boolean ready = true;
	/// <summary>
	/// Indicates if the spawn position is ready for spawning another Centipede.
	/// </summary>
	public Boolean Ready
	{
		get { return ready; }
		set
		{
			if (value)
			{
				Debug.LogError("Trying to set Ready externally!");
				return;
			}

			ready = false;
			StartCoroutine(EnableSpawnPosition());
		}
	}

	private void Awake()
	{
		SpawnPositions.Add(this);
	}

	private IEnumerator EnableSpawnPosition()
	{
		yield return new WaitForSeconds(SpawnInterval);
		ready = true;
	}
}