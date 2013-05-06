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

	/// <summary>
	/// The torque applied to rotate the transform to the restrainted by transform.
	/// A value of Single.Infinity, indicates that the restraint should be used with
	/// transforms, not with rigidbody.
	/// </summary>
	public Single TorqueConstant;

	void Start()
	{
		if (RestrictedByTranform == null)
			RestrictedByTranform = transform.parent;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Restraint();
	}

	/// <summary>
	/// Applies the restraint.
	/// This function can be called even when the script is disabled.
	/// </summary>
	public void Restraint()
	{
		if (!Single.IsPositiveInfinity(TorqueConstant))
		{
			var TranformDirection = (transform.right + transform.up).normalized;
			var RestrictedByTranformDirection = (RestrictedByTranform.right + RestrictedByTranform.up).normalized;

			var BaseTorque = Vector3.Cross(TranformDirection, RestrictedByTranformDirection);
			var Torque = TorqueConstant * BaseTorque.normalized * Mathf.Pow(BaseTorque.magnitude, 2);

			if (!ReverseRestraint)
				transform.rigidbody.AddTorque(Torque, ForceMode.Force);
			else
				RestrictedByTranform.rigidbody.AddTorque(-Torque, ForceMode.Force);
		}
		else
		{
			if (!ReverseRestraint)
				transform.transform.rotation = RestrictedByTranform.rotation;
			else
				RestrictedByTranform.rotation = transform.transform.rotation;
		}
	}
}
