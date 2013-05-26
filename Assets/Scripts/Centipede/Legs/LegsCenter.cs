using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// An empty "interface" for LegsCenter, in order to enable getting all components that inherits from it
/// (not possible for the generic class).
/// </summary>
public abstract class LegsCenter : MonoBehaviour
{
}

public abstract class LegsCenter<T> : LegsCenter where T : LegMotor
{
	/// <summary>
	/// The initial transform of the leg, relative to the body.
	/// </summary>
	public Vector3 InitialLegTransform;
	
	/// <summary>
	/// The initial total mass of the leg.
	/// </summary>
	public Single InitialLegMass;
	
	void Awake()
	{	
		// Disabling the script on awake, as it should start only when all legs are initialized.
		enabled = false;
	}

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
		Leg.Initialize(InitialLegMass);
	}

	/// <summary>
	/// Updates legs, with the new parameters in the LegsCenter.
	/// </summary>
	/// <param name="Legs"></param>
	protected abstract void UpdateLegs(IEnumerable<T> Legs);
}