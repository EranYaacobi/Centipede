using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// A normal leg motor, handled by the NormalLegsCenter.
/// The public members of this class should not be set directly, but via the
/// NormalLegsCenter to make all legs synchronized.
/// </summary>
public class NormalLegMotor : LegMotor
{
	/// <summary>
	/// The anchor of the back joint, relative to the body.
	/// </summary>
	public Vector3 BackJointAnchor;

	/// <summary>
	/// The anchor of the front joint, relative to the body.
	/// </summary>
	public Vector3 FrontJointAnchor;

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
	/// The maximum speed of the motor, in cycles per second.
	/// </summary>
	public Single MaxMotorSpeed;

	/// <summary>
	/// The number of cycles in second.
	/// </summary>
	public Single CycleSpeed;

	/// <summary>
	/// The lower limit of the joints, relative to their initial length.
	/// </summary>
	public Single LowerLimit;

	/// <summary>
	/// The upper limit of the joints, relative to their initial length.
	/// </summary>
	public Single UpperLimit;

	/// <summary>
	/// The retracted limit of the joints, relative to their initial length.
	/// </summary>
	public Single RetractedLimit;

	/// <summary>
	/// The initial offset of the leg.
	/// </summary>
	public Single InitialOffset;

	/// <summary>
	/// The damping of the motor.
	/// Ranges from 0 (no damping) to 1 (critial damping).
	/// </summary>
	[Range(0, 1)]
	public Single DampingRate;

	/// <summary>
	/// Indicates whether the desired length should be set to the center, when the motor stops.
	/// </summary>
	public Boolean CenterOnStop;

	/// <summary>
	/// The motors of the soles from which the normal leg consists of.
	/// </summary>
	private NormalLegSoleMotor[] SolesMotors;

	/// <summary>
	/// Indicates whether the leg is retracted.
	/// </summary>
	public Boolean Retracted;

	public override void Initialize(Single Mass)
	{
		base.Initialize(Mass);

		SolesMotors = GetComponentsInChildren<NormalLegSoleMotor>();
		foreach (var SoleMotor in SolesMotors)
		{
			SoleMotor.ConnectedBody = ConnectedBody;
			SoleMotor.rigidbody.mass = Mass / 2;
		}
		Common.Assert(SolesMotors.Length == 2);
		UpdateSoles();

		SolesMotors[0].DesiredSoleAngle = InitialOffset;
		SolesMotors[1].DesiredSoleAngle = InitialOffset + 90;

		foreach (var SoleMotor in SolesMotors)
			SoleMotor.Initialize();
	}

	private void UpdateSoles(Single Direction = 0)
	{
		var MotorState = BasicPrismaticJoint.MotorState.Invalid;
		if (Direction > 0)
			MotorState = BasicPrismaticJoint.MotorState.Forward;
		else if (Direction < 0)
			MotorState = BasicPrismaticJoint.MotorState.Backward;

		foreach (var SoleMotor in SolesMotors)
		{
			var LegOffsetInLink = new Vector3(transform.localPosition.x, 0, 0);
			SoleMotor.BackJointAnchor = BackJointAnchor + LegOffsetInLink;
			SoleMotor.FrontJointAnchor = FrontJointAnchor + LegOffsetInLink;
			SoleMotor.Flexibility = Flexibility;
			SoleMotor.ForceConstant = ForceConstant;
			SoleMotor.MaxMotorForce = MaxMotorForce;
			SoleMotor.MaxMotorSpeed = MaxMotorSpeed;
			if (MotorState != BasicPrismaticJoint.MotorState.Invalid)
				SoleMotor.MotorState = MotorState;
			SoleMotor.CycleSpeed = CycleSpeed;
			SoleMotor.SoleAnchor = LegAnchor;
			SoleMotor.DampingRate = DampingRate;
			SoleMotor.CenterOnStop = CenterOnStop;
			
			if (Retracted)
			{
				SoleMotor.LowerLimit = RetractedLimit;
				SoleMotor.UpperLimit = RetractedLimit;
				SoleMotor.collider.enabled = false;

				SoleMotor.BackJointAnchor += 0.1F * transform.up;
				SoleMotor.FrontJointAnchor += 0.1F * transform.up;
			}
			else
			{
				SoleMotor.LowerLimit = LowerLimit;
				SoleMotor.UpperLimit = UpperLimit;
				SoleMotor.collider.enabled = true;
			}
		}
	}

	protected override void UpdateValues()
	{
		base.UpdateValues();
		UpdateSoles();
	}

	protected override void PerformAction()
	{
		Retracted = !Retracted;
		UpdateValues();
	}

	protected override void Move(Single Direction)
	{
		UpdateSoles(Direction);
	}

	protected override void StopMoving()
	{
		base.StopMoving();

		foreach (var SoleMotor in SolesMotors)
			SoleMotor.MotorState = BasicPrismaticJoint.MotorState.Stopped;
	}
}