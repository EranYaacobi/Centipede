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
	
	private Vector3 backJointAnchor;
	/// <summary>
	/// The anchor of the back joint, relative to the body.
	/// </summary>
	public Vector3 BackJointAnchor
	{
		get { return backJointAnchor + SoleInitialTranform; }
		set { backJointAnchor = value; }
	}

	private Vector3 frontJointAnchor;
	/// <summary>
	/// The anchor of the front joint, relative to the body.
	/// </summary>
	public Vector3 FrontJointAnchor
	{
		get { return frontJointAnchor + SoleInitialTranform; }
		set { frontJointAnchor = value; }
	}

	/// <summary>
	/// The flexibility of the joint. This is the maximum difference between the current length of the joint,
	/// to the desired angle, after which maximum force is applied.
	/// </summary>
	public Single Flexibility;

	/// <summary>
	/// The force constant, which is used as a scalar when applying force.
	/// </summary>
	public Single ForceConsant;

	/// <summary>
	/// The maxmimum force of the motor.
	/// </summary>
	public Single MaxMotorForce;

	/// <summary>
	/// The maximum speed of the motor.
	/// </summary>
	public Single MaxMotorSpeed;

	/// <summary>
	/// The number of cycles in second.
	/// </summary>
	public Single CycleSpeed;

	/// <summary>
	/// The lower limit of the joint, relative to the initial length.
	/// </summary>
	public Single LowerLimit;

	/// <summary>
	/// The lower limit of the joint, relative to the initial length.
	/// </summary>
	public Single UpperLimit;

	/// <summary>
	/// The damping of the motor.
	/// </summary>
	[Range(0, 1)]
	public Single Damping;

	/// <summary>
	/// The state of the motor.
	/// </summary>
	public BasicPrismaticJoint.MotorState MotorState;

	/// <summary>
	/// The desired angle of the sole.
	/// </summary>
	public Single DesiredSoleAngle;

	/// <summary>
	/// The back joint created by the script.
	/// </summary>
	private BasicPrismaticJoint BackJoint;

	/// <summary>
	/// The front joint created by the script.
	/// </summary>
	private BasicPrismaticJoint FrontJoint;

	/// <summary>
	/// The initial transform of the sole.
	/// This is used to adjust the joints' remote anchors.
	/// </summary>
	private Vector3 SoleInitialTranform;

	/// <summary>
	/// Inidicates whether the script is initialized.
	/// </summary>
	private Boolean Initialized;

	public void Initialize()
	{
		SoleInitialTranform = transform.localPosition;

		BackJoint = gameObject.AddComponent<BasicPrismaticJoint>();
		BackJoint.Initialize(ConnectedBody, SoleAnchor, BackJointAnchor, Flexibility, ForceConsant, MaxMotorForce, MaxMotorSpeed, LowerLimit, UpperLimit);

		FrontJoint = gameObject.AddComponent<BasicPrismaticJoint>();
		FrontJoint.Initialize(ConnectedBody, SoleAnchor, FrontJointAnchor, Flexibility, ForceConsant, MaxMotorForce, MaxMotorSpeed, LowerLimit, UpperLimit);

		UpdateJoints();

		Initialized = true;
	}

	private void Update()
	{
		if (Initialized)
		{
			UpdateJoints();
			DesiredSoleAngle = (DesiredSoleAngle + ((Int32) MotorState)*CycleSpeed*360*Time.deltaTime + 360)%360;
		}
	}

	private void UpdateJoints()
	{
		BackJoint.Anchor = SoleAnchor;
		BackJoint.RemoteAnchor = BackJointAnchor;
		BackJoint.LowerLimit = LowerLimit;
		BackJoint.UpperLimit = UpperLimit;
		BackJoint.Flexibility = Flexibility;
		BackJoint.ForceConsant = ForceConsant;
		BackJoint.MaxMotorForce = MaxMotorForce;
		BackJoint.MotorSpeed = MaxMotorSpeed * Mathf.Sin(DesiredSoleAngle * Mathf.Deg2Rad);
		BackJoint.State = MotorState;
		BackJoint.Damping = Damping;

		FrontJoint.Anchor = SoleAnchor;
		FrontJoint.RemoteAnchor = FrontJointAnchor;
		FrontJoint.LowerLimit = LowerLimit;
		FrontJoint.UpperLimit = UpperLimit;
		FrontJoint.MaxMotorForce = MaxMotorForce;
		FrontJoint.MotorSpeed = MaxMotorSpeed * Mathf.Sin((DesiredSoleAngle - 90) * Mathf.Deg2Rad);
		FrontJoint.State = MotorState;
		FrontJoint.Damping = Damping;
	}
}
