using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Pinger : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(Ping());
	}

	private IEnumerator Ping()
	{
		while (enabled)
		{
			yield return new WaitForSeconds(1);
			Debug.Log(PhotonNetwork.GetPing());
		}
	}
}