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
	/// The anchor of the front spring, relative to the body.
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
	/// The maximum speed of the motor.
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
	/// The initial offset of the leg.
	/// </summary>
	public Single InitialOffset;

	/// <summary>
	/// The damping of the motor.
	/// </summary>
	[Range(0, 10)]
	public Single Damping;

	/// <summary>
	/// The motors of the soles from which the normal leg consists of.
	/// </summary>
	private NormalLegSoleMotor[] SolesMotors;

	/// <summary>
	/// Indicates whether the leg is retracted.
	/// </summary>
	public Boolean Retracted;

	public override void Initialize()
	{
		base.Initialize();

		SolesMotors = transform.GetComponentsInChildren<NormalLegSoleMotor>();
		foreach (var SoleMotor in SolesMotors)
			SoleMotor.ConnectedBody = ConnectedBody;
		Common.Assert(SolesMotors.Length == 2);
		UpdateSoles(0);

		SolesMotors[0].DesiredSoleAngle = InitialOffset + 90;
		SolesMotors[1].DesiredSoleAngle = InitialOffset + 270;

		foreach (var SoleMotor in SolesMotors)
			SoleMotor.Initialize();
	}

	private void UpdateSoles(Single Direction)
	{
		var MotorState = BasicPrismaticJoint.MotorState.Stopped;
		if (Direction > 0)
			MotorState = BasicPrismaticJoint.MotorState.Forward;
		else if (Direction < 0)
			MotorState = BasicPrismaticJoint.MotorState.Backward;

		foreach (var SoleMotor in SolesMotors)
		{
			SoleMotor.BackJointAnchor = BackJointAnchor;
			SoleMotor.FrontJointAnchor = FrontJointAnchor;
			SoleMotor.Flexibility = Flexibility;
			SoleMotor.ForceConstant = ForceConstant;
			SoleMotor.MaxMotorForce = MaxMotorForce;
			SoleMotor.MaxMotorSpeed = MaxMotorSpeed;
			SoleMotor.LowerLimit = LowerLimit;
			SoleMotor.UpperLimit = UpperLimit;
			SoleMotor.MotorState = MotorState;
			SoleMotor.CycleSpeed = CycleSpeed;
			SoleMotor.Damping = Damping;
			
			if (Retracted)
			{
				SoleMotor.LowerLimit = LowerLimit;
				SoleMotor.UpperLimit = LowerLimit;
			}
		}
	}

	protected override void UpdateValues()
	{
		base.UpdateValues();
		UpdateSoles(0);
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