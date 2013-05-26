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
	public Single ForceConstant;

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
	/// Ranges from 0 (no damping) to 1 (critial damping).
	/// </summary>
	[Range(0, 1)]
	public Single DampingRate;

	/// <summary>
	/// The state of the motor.
	/// </summary>
	public BasicPrismaticJoint.MotorState MotorState;

	/// <summary>
	/// Indicates whether the desired length should be set to the center, when the motor stops.
	/// </summary>
	public Boolean CenterOnStop;
	
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
		BackJoint.Initialize(ConnectedBody, SoleAnchor, BackJointAnchor, Flexibility, ForceConstant, MaxMotorForce, MaxMotorSpeed, LowerLimit, UpperLimit, DampingRate, CenterOnStop, true);

		FrontJoint = gameObject.AddComponent<BasicPrismaticJoint>();
		FrontJoint.Initialize(ConnectedBody, SoleAnchor, FrontJointAnchor, Flexibility, ForceConstant, MaxMotorForce, MaxMotorSpeed, LowerLimit, UpperLimit, DampingRate, CenterOnStop, true);

		UpdateJoints();

		Initialized = true;
	}

	private void FixedUpdate()
	{
		if (Initialized)
		{
			UpdateJoints();
			DesiredSoleAngle = (DesiredSoleAngle + ((Int32)MotorState) * CycleSpeed * 360 * Time.fixedDeltaTime + 360) % 360;
		}
	}

	private void UpdateJoints()
	{
		BackJoint.Anchor = SoleAnchor;
		BackJoint.RemoteAnchor = BackJointAnchor;
		BackJoint.LowerLimit = LowerLimit;
		BackJoint.UpperLimit = UpperLimit;
		BackJoint.Flexibility = Flexibility;
		BackJoint.ForceConstant = ForceConstant;
		BackJoint.MaxMotorForce = MaxMotorForce;
		BackJoint.MotorSpeed = MaxMotorSpeed * Mathf.Sin(DesiredSoleAngle * Mathf.Deg2Rad);
		if (MotorState != BasicPrismaticJoint.MotorState.Stopped)
			BackJoint.State = BasicPrismaticJoint.MotorState.Forward;
		else
			BackJoint.State = MotorState;
		BackJoint.DampingRate = DampingRate;
		BackJoint.CenterOnStop = CenterOnStop;

		FrontJoint.Anchor = SoleAnchor;
		FrontJoint.RemoteAnchor = FrontJointAnchor;
		FrontJoint.LowerLimit = LowerLimit;
		FrontJoint.UpperLimit = UpperLimit;
		FrontJoint.MaxMotorForce = MaxMotorForce;
		FrontJoint.MotorSpeed = MaxMotorSpeed * Mathf.Sin((DesiredSoleAngle - 90) * Mathf.Deg2Rad);
		if (MotorState != BasicPrismaticJoint.MotorState.Stopped)
			FrontJoint.State = BasicPrismaticJoint.MotorState.Forward;
		else
			FrontJoint.State = MotorState;
		FrontJoint.DampingRate = DampingRate;
		FrontJoint.CenterOnStop = CenterOnStop;
	}
}
