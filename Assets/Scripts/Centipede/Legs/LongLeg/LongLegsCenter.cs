using System;
using System.Collections.Generic;
using UnityEngine;

public class LongLegsCenter : LegsCenter<LongLeg>
{
	/// <summary>
	/// The anchor of the back-joint, relative to the body.
	/// </summary>
	public Vector3 BackJointAnchor;

	/// <summary>
	/// The anchor of the front-joint, relative to the body.
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
	/// The speed of the motor when retracting.
	/// </summary>
	public Single MotorRetractingForce;

	/// <summary>
	/// The force of the motor when lengthening.
	/// </summary>
	public Single MotorLengtheningForce;

	/// <summary>
	/// The speed of the motor when retracting.
	/// </summary>
	public Single MotorRetractingSpeed;

	/// <summary>
	/// The speed of the motor when lengthening.
	/// </summary>
	public Single MotorLengtheningSpeed;

	/// <summary>
	/// The retracted length of the leg.
	/// </summary>
	public Single RetractedLength;

	/// <summary>
	/// The maximum length of the leg.
	/// </summary>
	public Single MaximumLength;

	/// <summary>
	/// The amount of time that the leg should lengthen when activated.
	/// </summary>
	public Single LengtheningTime;

	/// <summary>
	/// The amount of time that must be waited before the leg can be activated again.
	/// </summary>
	public Single ReloadingTime;

	/// <summary>
	/// The damping of the motor.
	/// Ranges from 0 (no damping) to 1 (critial damping).
	/// </summary>
	[Range(0, 1)]
	public Single DampingRate;

	protected override void InitializeLeg(LongLeg Leg, Int32 LegIndex)
	{
		Leg.Retracted = true;
		base.InitializeLeg(Leg, LegIndex);
	}

	protected override void UpdateLegs(IEnumerable<LongLeg> Legs)
	{
		foreach (var Leg in Legs)
		{
			Leg.BackJointAnchor = BackJointAnchor;
			Leg.FrontJointAnchor = FrontJointAnchor;
			Leg.Flexibility = Flexibility;
			Leg.ForceConstant = ForceConstant;
			Leg.MotorRetractingForce = MotorRetractingForce;
			Leg.MotorLengtheningForce = MotorLengtheningForce;
			Leg.MotorRetractingSpeed = MotorRetractingSpeed;
			Leg.MotorLengtheningSpeed = MotorLengtheningSpeed;
			Leg.RetractedLength = RetractedLength;
			Leg.MaximumLength = MaximumLength;
			Leg.DampingRate = DampingRate;
			Leg.LengtheningTime = LengtheningTime;
			Leg.ReloadingTime = ReloadingTime;
		}
	}
}