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
	/// Indicates whether on start, the script should override equilibrium length to be the current distance between the objects.
	/// </summary>
	public Boolean UseDefaultEquilibriumLength;

	/// <summary>
	/// Causes the spring to apply negative force based on velocity, in addition to distance.
	/// Enabling this option causes the spring to be less jumpy.
	/// </summary>
	public Boolean ApplyNegativeForce;

	public void Initialize(Rigidbody ConnectedBody, Vector3 Anchor, Vector3 RemoteAnchor, Single Stiffness, Boolean ApplyNegativeForce)
	{
		Initialize(ConnectedBody, Anchor, RemoteAnchor, Stiffness, ApplyNegativeForce, (ConnectedBody.transform.TransformPoint(RemoteAnchor) - transform.TransformPoint(Anchor)).magnitude);
		// We don't set UseDefaultEquilibriumLength to true, as the default equilibrium length was already used.
	}

	public void Initialize(Rigidbody ConnectedBody, Vector3 Anchor, Vector3 RemoteAnchor, Single Stiffness, Boolean ApplyNegativeForce, Single EquilibriumLength)
	{
		this.ConnectedBody = ConnectedBody;
		this.Anchor = Anchor;
		this.RemoteAnchor = RemoteAnchor;
		this.Stiffness = Stiffness;
		this.ApplyNegativeForce = ApplyNegativeForce;
		this.EquilibriumLength = EquilibriumLength;
		this.UseDefaultEquilibriumLength = false;
	}

	void Start()
	{
		if (UseDefaultEquilibriumLength)
			EquilibriumLength = (ConnectedBody.transform.TransformPoint(RemoteAnchor) - transform.TransformPoint(Anchor)).magnitude;
	}

	// Update is called once per frame
	void Update()
	{
		var Position = transform.TransformPoint(Anchor);
		var OtherPosition = ConnectedBody.transform.TransformPoint(RemoteAnchor);
		var CurrentLength = (OtherPosition - Position).magnitude;
		var RelaxedDistance = CurrentLength - EquilibriumLength;
		var ForceDirection = (OtherPosition - Position).normalized;
		var AppliedForce = ForceDirection * RelaxedDistance * Stiffness;

		ConnectedBody.rigidbody.AddForce(-AppliedForce, ForceMode.Force);
		rigidbody.AddForce(AppliedForce, ForceMode.Force);
		//Debug.DrawLine(Position, Position + ForceDirection * EquilibriumLength);
		Debug.DrawLine(Position, OtherPosition);

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
}
