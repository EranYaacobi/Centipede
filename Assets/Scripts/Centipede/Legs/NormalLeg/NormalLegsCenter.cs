using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class NormalLegsCenter : LegsCenter<NormalLeg>
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
	/// The maxmimum force of the motor.
	/// </summary>
	public Single MaxMotorForce;

	/// <summary>
	/// The maximum speed of the motor of each sole in each leg, in cycles per second.
	/// </summary>
	public Single MaxMotorSpeed;

	/// <summary>
	/// The number of cycles in second.
	/// </summary>
	public Single CycleSpeed;

	/// <summary>
	/// The lower limit of the joints of each sole in each leg, relative to their initial length.
	/// </summary>
	public Single LowerLimit;

	/// <summary>
	/// The upper limit of the joints of each sole in each leg, relative to their initial length.
	/// </summary>
	public Single UpperLimit;

	/// <summary>
	/// The retracted limit of the joints of each sole in each leg, relative to their initial length.
	/// </summary>
	public Single RetractedLimit;

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

	protected override void InitializeLeg(NormalLeg Leg, Int32 LegIndex)
	{
		Leg.InitialOffset = LegIndex * (1080F / 7);

		base.InitializeLeg(Leg, LegIndex);
	}

	protected override void UpdateLegs(IEnumerable<NormalLeg> Legs)
	{
		foreach (var Leg in Legs)
		{
			Leg.BackJointAnchor = BackJointAnchor;
			Leg.FrontJointAnchor = FrontJointAnchor;
			Leg.Flexibility = Flexibility;
			Leg.ForceConstant = ForceConstant;
			Leg.MaxMotorForce = MaxMotorForce;
			Leg.MaxMotorSpeed = MaxMotorSpeed;
			Leg.LowerLimit = LowerLimit;
			Leg.UpperLimit = UpperLimit;
			Leg.RetractedLimit = RetractedLimit;
			Leg.CycleSpeed = CycleSpeed;
			Leg.DampingRate = DampingRate;
			Leg.CenterOnStop = CenterOnStop;
		}
	}
}
