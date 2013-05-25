using System;
using System.Collections.Generic;
using UnityEngine;

public class NaturalWheelLegsCenter : LegsCenter<NaturalWheelLegMotor>
{
	/// <summary>
	/// The maximum angular velocity of the wheel.
	/// This affects the amount of torque applied, based on the ratio between the current
	/// angular velocity, and the maximum angular velocity.
	/// </summary>
	public Single MaximumAngularVelocity;

	/// <summary>
	/// The torque of the wheel motor.
	/// </summary>
	public Single MotorTorque;

	/// <summary>
	/// The torque of the brakes.
	/// Brakes are applied when the wheel is rotating in the opposite direction of the motor movement.
	/// </summary>
	public Single BrakeTorque;

	/// <summary>
	/// The maximum slip rate after which the motor shuts down.
	/// </summary>
	[Range(0, 1)]
	public Single MaximumSlipRate;

	/// <summary>
	/// The anchor of the back suspension joint, relative to the body.
	/// </summary>
	public Vector3 BackSuspensionJointAnchor;

	/// <summary>
	/// The anchor of the front suspension joint, relative to the body.
	/// </summary>
	public Vector3 FrontSuspensionJointAnchor;

	/// <summary>
	/// The force constant, which is used as a scalar when applying force.
	/// </summary>
	public Single SuspensionJointsForceConstant;

	/// <summary>
	/// The maxmimum force of the suspension joints' motors.
	/// </summary>
	public Single SuspensionJointsMaxMotorForce;

	/// <summary>
	/// The lower limit of the suspension joints, relative to their initial length.
	/// </summary>
	public Single SuspensionLowerLimit;

	/// <summary>
	/// The upper limit of the suspension joints, relative to their initial length.
	/// </summary>
	public Single SuspensionUpperLimit;

	/// <summary>
	/// The retracted length of the suspension joints, relative to their initial length.
	/// </summary>
	public Single SuspensionRetractedLength;

	/// <summary>
	/// The damping of the suspension joints' motors.
	/// Ranges from 0 (no damping) to 1 (critial damping).
	/// </summary>
	[Range(0, 1)]
	public Single SuspensionDampingRate;

	protected override void InitializeLeg(NaturalWheelLegMotor Leg, Int32 LegIndex)
	{
		Leg.Retracted = true;
		base.InitializeLeg(Leg, LegIndex);
	}

	protected override void UpdateLegs(IEnumerable<NaturalWheelLegMotor> Legs)
	{
		foreach (var Leg in Legs)
		{
			Leg.MaximumAngularVelocity = MaximumAngularVelocity;
			Leg.MotorTorque = MotorTorque;
			Leg.BrakeTorque = BrakeTorque;
			Leg.MaximumSlipRate = MaximumSlipRate;
			Leg.BackSuspensionJointAnchor = BackSuspensionJointAnchor;
			Leg.FrontSuspensionJointAnchor = FrontSuspensionJointAnchor;
			Leg.SuspensionJointsForceConstant = SuspensionJointsForceConstant;
			Leg.SuspensionJointsMaxMotorForce = SuspensionJointsMaxMotorForce;
			Leg.SuspensionLowerLimit = SuspensionLowerLimit;
			Leg.SuspensionUpperLimit = SuspensionUpperLimit;
			Leg.SuspensionDampingRate = SuspensionDampingRate;
			Leg.SuspensionRetractedLength = SuspensionRetractedLength;
		}
	}
}