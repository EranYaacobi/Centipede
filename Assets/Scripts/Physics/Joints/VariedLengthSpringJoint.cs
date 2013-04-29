using UnityEngine;
using System.Collections;
using System;

public class VariedLengthSpringJoint : MonoBehaviour
{
	/// <summary>
	/// The body connected to the game object via the spring.
	/// </summary>
	public Rigidbody ConnectedBody;
	
	/// <summary>
	/// The anchor of the spring on the game object.
	/// </summary>
	public Vector3 Anchor;

	/// <summary>
	/// The anchor of the spring on the other game object.
	/// </summary>
	public Vector3 RemoteAnchor;

	/// <summary>
	/// The stiffness of the spring - this effects the force applied on the connected game objects, when
	/// not at an equilibrium state.
	/// </summary>
	public Single Stiffness;

	/// <summary>
	/// The length at which the spring is at equilibrium, meaning it applies no force.
	/// This is initialized to the initial distance between the game objects when the spring joint is created.
	/// </summary>
	public Single EquilibriumLength;

	/// <summary>
	/// Indicates whether the script should override equilibrium length to be the current distance between the objects,
	/// and to update it when the anchors change.
	/// </summary>
	public Boolean FixedDefaultEquilibriumLength;

	/// <summary>
	/// Causes the spring to apply negative force based on velocity, in addition to distance.
	/// Enabling this option causes the spring to be less jumpy.
	/// </summary>
	public Boolean ApplyNegativeForce;

	/// <summary>
	/// The initial transform of the joint.
	/// This is used to adjust the length, when the anchors change.
	/// </summary>
	private Vector3 InitialTranform;

	public Vector3 AppliedForce;

	public void Initialize(Rigidbody ConnectedBody, Vector3 Anchor, Vector3 RemoteAnchor, Single Stiffness, Boolean ApplyNegativeForce, Single EquilibriumLength = -1)
	{
		InitialTranform = transform.parent.localPosition + transform.localPosition;

		this.ConnectedBody = ConnectedBody;
		this.Anchor = Anchor;
		this.RemoteAnchor = RemoteAnchor;
		this.Stiffness = Stiffness;
		this.ApplyNegativeForce = ApplyNegativeForce;

		if (EquilibriumLength == -1)
		{
			this.EquilibriumLength = (RemoteAnchor - transform.parent.localPosition - transform.localPosition - Anchor).magnitude;
			this.FixedDefaultEquilibriumLength = true;
		}
		else
		{
			this.EquilibriumLength = EquilibriumLength;
			this.FixedDefaultEquilibriumLength = false;
		}
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (FixedDefaultEquilibriumLength)
			EquilibriumLength = (RemoteAnchor - InitialTranform - Anchor).magnitude;

		var Position = transform.TransformPoint(Anchor);
		var OtherPosition = ConnectedBody.transform.TransformPoint(RemoteAnchor);
		var CurrentLength = (OtherPosition - Position).magnitude;
		var RelaxedDistance = CurrentLength - EquilibriumLength;
		var ForceDirection = (OtherPosition - Position).normalized;
		AppliedForce = ForceDirection * RelaxedDistance * Stiffness;

		ConnectedBody.rigidbody.AddForce(-AppliedForce, ForceMode.Force);
		rigidbody.AddForce(AppliedForce, ForceMode.Force);

		if (ApplyNegativeForce)
		{
			var NextPosition = Position + rigidbody.velocity;
			var NextOtherPosition = OtherPosition + ConnectedBody.rigidbody.velocity;
			var NextLength = (NextOtherPosition - NextPosition).magnitude;
			if (((CurrentLength >= EquilibriumLength) && (NextLength < EquilibriumLength)) ||
				((CurrentLength <= EquilibriumLength) && (NextLength > EquilibriumLength)))
			{
				rigidbody.AddForce(-rigidbody.velocity, ForceMode.Acceleration);
				ConnectedBody.rigidbody.AddForce(-ConnectedBody.rigidbody.velocity, ForceMode.Acceleration);
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		var Position = transform.TransformPoint(Anchor);
		var OtherPosition = ConnectedBody.transform.TransformPoint(RemoteAnchor);
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(Position, OtherPosition);
	}
}
