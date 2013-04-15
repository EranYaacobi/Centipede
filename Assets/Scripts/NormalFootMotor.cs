using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// A normal foot motor, handled by the NormalFeetCenter.
/// The public members of this class should not be set directly, but via the NormalFeetCenter,
/// in order to make all feet synchronized.
/// </summary>
public class NormalFootMotor : FootMotor
{
	/// <summary>
	/// The anchor of the back spring, relative to the body.
	/// </summary>
	public Vector3 BackSpringAnchor;

	/// <summary>
	/// The anchor of the front spring, relative to the body.
	/// </summary>
	public Vector3 FrontSpringAnchor;

	/// <summary>
	/// The stiffness of the springs.
	/// </summary>
	public Single Stiffness;

	/// <summary>
	/// The initial length between the foot and the body it's connected to.
	/// </summary>
	public Single InitialLength;

	/// <summary>
	/// The equilibrium-length of the back spring.
	/// </summary>
	public Single BackSpringEquilibriumLength
	{
		get { return BackSpringJoint.EquilibriumLength; }
		set { BackSpringJoint.EquilibriumLength = value; }
	}

	/// <summary>
	/// The equilibrium-length of the front spring.
	/// </summary>
	public Single FrontSpringEquilibriumLength
	{
		get { return FrontSpringJoint.EquilibriumLength; }
		set { FrontSpringJoint.EquilibriumLength = value; }
	}

	/// <summary>
	/// The back spring joint created by the script.
	/// </summary>
	private VariedLengthSpringJoint BackSpringJoint;

	/// <summary>
	/// The front spring joint created by the script.
	/// </summary>
	private VariedLengthSpringJoint FrontSpringJoint;

	/// <summary>
	/// Indicates whether the leg is retracted.
	/// </summary>
	public Boolean Retracted;

	protected override void Initialize()
	{
		BackSpringJoint = gameObject.AddComponent<VariedLengthSpringJoint>();
		BackSpringJoint.Initialize(ConnectedBody, FootAnchor, BackSpringAnchor, Stiffness);

		FrontSpringJoint = gameObject.AddComponent<VariedLengthSpringJoint>();
		FrontSpringJoint.Initialize(ConnectedBody, FootAnchor, FrontSpringAnchor, Stiffness);

		var BackHingeJoint = ConnectedBody.gameObject.AddComponent<HingeJoint>();
		BackHingeJoint.connectedBody = rigidbody;
		BackHingeJoint.anchor = BackSpringAnchor;
		BackHingeJoint.axis = new Vector3(0, 0, 1);
		BackHingeJoint.useSpring = true;
		BackHingeJoint.useLimits = true;
		BackHingeJoint.limits = new JointLimits{min = -45, max = 45};

		var FrontHingeJoint = ConnectedBody.gameObject.AddComponent<HingeJoint>();
		FrontHingeJoint.connectedBody = rigidbody;
		FrontHingeJoint.anchor = FrontSpringAnchor;
		FrontHingeJoint.axis = new Vector3(0, 0, 1);
		FrontHingeJoint.useSpring = true;
		FrontHingeJoint.useLimits = true;
		FrontHingeJoint.limits = new JointLimits{min = -45, max = 45};

		InitialLength = BackSpringJoint.EquilibriumLength;
		Retracted = false;
	}

	protected override void UpdateValues()
	{
		base.UpdateValues();

		BackSpringJoint.ConnectedBody = ConnectedBody;
		BackSpringJoint.Stiffness = Stiffness;
		BackSpringJoint.Anchor = FootAnchor;
		BackSpringJoint.RemoteAnchor = BackSpringAnchor;

		FrontSpringJoint.ConnectedBody = ConnectedBody;
		FrontSpringJoint.Stiffness = Stiffness;
		FrontSpringJoint.Anchor = FootAnchor;
		FrontSpringJoint.RemoteAnchor = FrontSpringAnchor;
	}

	protected override void PerformAction()
	{
		Retracted = !Retracted;
	}

	protected override void Move(Single Direction)
	{
		// Nothing to do here, as NormalFeetCenter takes care of movement.
	}
}
