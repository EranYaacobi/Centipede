using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// A normal leg's sole motor, handled by the NormalLegMotor.
/// The public members of this class should not be set directly, but via the
/// NormalLegMotor (which is in turn set by the NormalLegsCenter), in order
/// to make all legs (and soles) synchronized.
/// </summary>
public class NormalLegSoleMotor : MonoBehaviour
{
	/// <summary>
	/// The body to which the sole is connected.
	/// </summary>
	public Rigidbody ConnectedBody;

	/// <summary>
	/// The anchor of the sole.
	/// </summary>
	public Vector3 SoleAnchor;
	
	private Vector3 backSpringAnchor;
	/// <summary>
	/// The anchor of the back spring, relative to the body.
	/// </summary>
	public Vector3 BackSpringAnchor
	{
		get { return backSpringAnchor + SoleInitialTranform; }
		set { backSpringAnchor = value; }
	}

	private Vector3 frontSpringAnchor;
	/// <summary>
	/// The anchor of the front spring, relative to the body.
	/// </summary>
	public Vector3 FrontSpringAnchor
	{
		get { return frontSpringAnchor + SoleInitialTranform; }
		set { frontSpringAnchor = value; }
	}

	/// <summary>
	/// The stiffness of the springs.
	/// </summary>
	public Single Stiffness;

	/// <summary>
	/// The initial length between the sole and the body it's connected to.
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
	/// The initial transform of the sole.
	/// This is used to adjust the springs' remote anchors.
	/// </summary>
	private Vector3 SoleInitialTranform;

	private void Start()
	{
		SoleInitialTranform = transform.localPosition;

		BackSpringJoint = gameObject.AddComponent<VariedLengthSpringJoint>();
		BackSpringJoint.Initialize(ConnectedBody, SoleAnchor, BackSpringAnchor, Stiffness);

		FrontSpringJoint = gameObject.AddComponent<VariedLengthSpringJoint>();
		FrontSpringJoint.Initialize(ConnectedBody, SoleAnchor, FrontSpringAnchor, Stiffness);

		/*var BackHingeJoint = ConnectedBody.gameObject.AddComponent<HingeJoint>();
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
		FrontHingeJoint.limits = new JointLimits{min = -45, max = 45};*/

		InitialLength = BackSpringJoint.EquilibriumLength;
	}

	private void Update()
	{
		BackSpringJoint.ConnectedBody = ConnectedBody;
		BackSpringJoint.Stiffness = Stiffness;
		BackSpringJoint.Anchor = SoleAnchor;
		BackSpringJoint.RemoteAnchor = BackSpringAnchor;

		FrontSpringJoint.ConnectedBody = ConnectedBody;
		FrontSpringJoint.Stiffness = Stiffness;
		FrontSpringJoint.Anchor = SoleAnchor;
		FrontSpringJoint.RemoteAnchor = FrontSpringAnchor;
	}
}
