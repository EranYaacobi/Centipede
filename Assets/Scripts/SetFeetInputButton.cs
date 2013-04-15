using System;
using UnityEngine;
using System.Collections;

public class SetFeetInputButton : MonoBehaviour {

	// Use this for initialization
	void Start()
	{
		var Feet = transform.GetComponentsInChildren<FootMotor>();
		for (int i = 0; i < Feet.Length; i++)
		{
			Feet[i].InputButton = String.Format(Keys.LegActionFormat, i + 1);
		}
	}
}
