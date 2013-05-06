using System;
using UnityEngine;
using System.Collections;

public class SetLegsInputButton : MonoBehaviour {

	// Use this for initialization
	void Start()
	{
		var Legs = GetComponentsInChildren<LegMotor>();
		for (int i = 0; i < Legs.Length; i++)
		{
			Legs[i].InputButton = String.Format(Keys.LegActionFormat, i + 1);
		}
	}
}
