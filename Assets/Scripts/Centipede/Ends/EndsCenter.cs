using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// An empty "interface" for EndsCenter, in order to enable getting all components that inherits from it
/// (not possible for the generic class).
/// </summary>
public abstract class EndsCenter : MonoBehaviour
{
}

public abstract class EndsCenter<T> : EndsCenter where T : End
{
	/// <summary>
	/// The initial transform of the end, relative to the body.
	/// </summary>
	public Vector3 InitialEndTransform;
	
	/// <summary>
	/// The initial total mass of the end.
	/// </summary>
	public Single InitialEndMass;
	
	void Awake()
	{	
		// Disabling the script on awake, as it should start only when all end are initialized.
		enabled = false;
	}

	void Start()
	{
		var Ends = GetComponentsInChildren<T>();

		if (Ends.Length == 0)
		{
			enabled = false;
			return;
		}

		UpdateEnds(Ends);

		for (int i = 0; i < Ends.Length; i++)
			InitializeEnd(Ends[i]);
	}

	void Update()
	{
		var Ends = GetComponentsInChildren<T>();

		UpdateEnds(Ends);
	}

	/// <summary>
	/// Initializes an end.
	/// Implementations in derived class should call the base function at the end.
	/// </summary>
	/// <param name="End"></param>
	protected virtual void InitializeEnd(T End)
	{
		if (End.EndPosition == EndPosition.Back)
		{
			End.transform.localPosition -= InitialEndTransform;
		}
		else
		{
			End.transform.localPosition += InitialEndTransform;
			End.transform.Rotate(0, 0, 180);
		}

		End.Initialize(InitialEndMass);
		foreach (var Collider in End.GetComponentsInChildren<Collider>())
			Collider.enabled = true;
	}

	/// <summary>
	/// Updates ends, with the new parameters in the EndsCenter.
	/// </summary>
	/// <param name="Ends"></param>
	protected abstract void UpdateEnds(IEnumerable<T> Ends);
}