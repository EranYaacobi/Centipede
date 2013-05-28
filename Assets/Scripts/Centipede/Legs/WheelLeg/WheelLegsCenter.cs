using System;
using System.Collections.Generic;
using UnityEngine;

public class WheelLegsCenter : LegsCenter<WheelLeg>
{
	/// <summary>
	/// The suspension distance of the wheel, meaning the maximum distance that the wheel can be at from
	/// its pivot. Higher values means that the wheel can be further from the pivot, causing the centipede's
	/// body more vibrating.
	/// </summary>
	public Single SuspensionDistance;

	/// <summary>
	/// The suspension distance of the wheel when it is retracted.
	/// </summary>
	public Single RetractedSuspensionDistance;

	/// <summary>
	/// The force of the spring that tries to maintain the suspension distance.
	/// </summary>
	public Single SuspensionSpring;

	/// <summary>
	/// The damping applied on the spring that tries to maintain the suspension distance.
	/// </summary>
	public Single SuspensionDamper;

	/// <summary>
	/// The target position of the suspension spring.
	/// A value of 0 maps to fully extended suspension, and 1 maps to fully compressed suspension
	/// </summary>
	[Range(0, 1)]
	public Single SuspensionTargetPosition;

	/// <summary>
	/// The force of the motor.
	/// </summary>
	public Single MotorForce;

	/// <summary>
	/// The force of the brakes.
	/// Brakes are applied when trying to move in the opposite direction of the current movement.
	/// </summary>
	public Single BrakeForce;

	protected override void InitializeLeg(WheelLeg Leg, Int32 LegIndex)
	{
		Leg.Retracted = true;
		base.InitializeLeg(Leg, LegIndex);
	}

	protected override void UpdateLegs(IEnumerable<WheelLeg> Legs)
	{
		foreach (var Leg in Legs)
		{
			Leg.SuspensionDistance = SuspensionDistance;
			Leg.RetractedSuspensionDistance = RetractedSuspensionDistance;
			Leg.SuspensionSpring = SuspensionSpring;
			Leg.SuspensionDamper = SuspensionDamper;
			Leg.SuspensionTargetPosition = SuspensionTargetPosition;
			Leg.MotorForce = MotorForce;
			Leg.BrakeForce = BrakeForce;
		}
	}
}