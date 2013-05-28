using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// A normal leg, handled by the NormalLegsCenter.
/// The public members of this class should not be set directly, but via the
/// NormalLegsCenter to make all legs synchronized.
/// </summary>
public class NormalLeg : Leg
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
	private NormalLegSole[] Soles;

	/// <summary>
	/// Indicates whether the leg is retracted.
	/// </summary>
	public Boolean Retracted;

	public override void Initialize(Single Mass)
	{
		base.Initialize(Mass);

		Soles = GetComponentsInChildren<NormalLegSole>();
		foreach (var Sole in Soles)
		{
			Sole.ConnectedBody = ConnectedBody;
			Sole.rigidbody.mass = Mass / 2;
		}
		Common.Assert(Soles.Length == 2);
		UpdateSoles();

		Soles[0].DesiredSoleAngle = InitialOffset;
		Soles[1].DesiredSoleAngle = InitialOffset + 90;

		foreach (var SoleMotor in Soles)
			SoleMotor.Initialize();
	}

	private void UpdateSoles(Single Direction = 0)
	{
		var MotorState = BasicPrismaticJoint.MotorState.Invalid;
		if (Direction > 0)
			MotorState = BasicPrismaticJoint.MotorState.Forward;
		else if (Direction < 0)
			MotorState = BasicPrismaticJoint.MotorState.Backward;

		foreach (var Sole in Soles)
		{
			var LegOffsetInLink = new Vector3(transform.localPosition.x, 0, 0);
			Sole.BackJointAnchor = BackJointAnchor + LegOffsetInLink;
			Sole.FrontJointAnchor = FrontJointAnchor + LegOffsetInLink;
			Sole.Flexibility = Flexibility;
			Sole.ForceConstant = ForceConstant;
			Sole.MaxMotorForce = MaxMotorForce;
			Sole.MaxMotorSpeed = MaxMotorSpeed;
			if (MotorState != BasicPrismaticJoint.MotorState.Invalid)
				Sole.MotorState = MotorState;
			Sole.CycleSpeed = CycleSpeed;
			Sole.SoleAnchor = LegAnchor;
			Sole.DampingRate = DampingRate;
			Sole.CenterOnStop = CenterOnStop;
			
			if (Retracted)
			{
				Sole.LowerLimit = Mathf.Lerp(Sole.LowerLimit, RetractedLimit, 10 * Time.deltaTime);
				Sole.UpperLimit = Mathf.Lerp(Sole.UpperLimit, RetractedLimit, 10 * Time.deltaTime);
				Sole.collider.enabled = false;
				Sole.rigidbody.useGravity = false;

				Sole.BackJointAnchor += 0.1F * Vector3.up;
				Sole.FrontJointAnchor += 0.1F * Vector3.up;
			}
			else
			{
				Sole.LowerLimit = Mathf.Lerp(Sole.LowerLimit, LowerLimit, 10 * Time.deltaTime);
				Sole.UpperLimit = Mathf.Lerp(Sole.UpperLimit, UpperLimit, 10 * Time.deltaTime);
				Sole.collider.enabled = true;
				Sole.rigidbody.useGravity = true;
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

		foreach (var Sole in Soles)
			Sole.MotorState = BasicPrismaticJoint.MotorState.Stopped;
	}
}