using System;
using UnityEngine;
using System.Collections;

public class SetLegsInputButton : MonoBehaviour {

	void Awake()
	{
		// Disabling the script on awake, as it should start only when all legs are initialized.
		enabled = false;
	}

	void Start()
	{
		var Legs = GetComponentsInChildren<LegMotor>();
		for (int i = 0; i < Legs.Length; i++)
		{
			Legs[i].InputButton = String.Format(Keys.LegActionFormat, i + 1);
		}
	}
}
