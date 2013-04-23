using System;
using UnityEngine;
using System.Collections;

public class NormalLegsCenter : MonoBehaviour
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
	/// The maximum speed of the motor of each sole in each leg.
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
	/// The damping of the motor.
	/// </summary>
	[Range(0, 10)]
	public Single Damping;

	// Use this for initialization
	void Start()
	{
		var Legs = transform.GetComponentsInChildren<NormalLegMotor>();

		if (Legs.Length == 0)
		{
			enabled = false;
			return;
		}

		UpdateLegs();

		for (int i = 0; i < Legs.Length; i++)
		{
			var Leg = Legs[i];
			Leg.InitialOffset = i * (360 / Legs.Length);
			Leg.Initialize();
		}
	}

	private void UpdateLegs()
	{
		var Legs = transform.GetComponentsInChildren<NormalLegMotor>();

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
			Leg.CycleSpeed = CycleSpeed;
			Leg.Damping = Damping;
		}
	}

	public void Update()
	{
		UpdateLegs();
	}
}
