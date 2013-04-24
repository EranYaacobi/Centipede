using System;
using UnityEngine;
using System.Collections;

public class RestraintRotation : MonoBehaviour
{
	/// <summary>
	/// The transform by which this transform should be restrainted.
	/// </summary>
	public Transform RestrictedByTranform;

	/// <summary>
	/// Indicates whether restraining should be reversed
	/// </summary>
	public Boolean ReverseRestraint;

	void Start()
	{
		if (RestrictedByTranform == null)
			RestrictedByTranform = transform.parent;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (!ReverseRestraint)
			transform.rotation = RestrictedByTranform.rotation;
		else
			RestrictedByTranform.rotation = transform.rotation;
	}
}
