using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public abstract class LegsCenter<T> : MonoBehaviour where T : LegMotor
{
	/// <summary>
	/// The initial transform of the leg, relative to the body.
	/// </summary>
	public Vector3 InitialLegTransform;
	
	/// <summary>
	/// The total mass of the leg.
	/// </summary>
	public Single LegMass;

	void Start()
	{
		var Legs = GetComponentsInChildren<T>();

		if (Legs.Length == 0)
		{
			enabled = false;
			return;
		}

		UpdateLegs(Legs);

		for (int i = 0; i < Legs.Length; i++)
			InitializeLeg(Legs[i], i);
	}

	void Update()
	{
		var Legs = GetComponentsInChildren<T>();

		UpdateLegs(Legs);
	}

	/// <summary>
	/// Initializes a leg.
	/// Implementations in derived class should call the base function at the end.
	/// </summary>
	/// <param name="Leg"></param>
	/// <param name="LegIndex"></param>
	protected virtual void InitializeLeg(T Leg, Int32 LegIndex)
	{
		Leg.transform.localPosition += InitialLegTransform;
		Leg.Initialize(LegMass);
	}

	/// <summary>
	/// Updates legs, with the new parameters in the LegsCenter.
	/// </summary>
	/// <param name="Legs"></param>
	protected abstract void UpdateLegs(IEnumerable<T> Legs);
}
