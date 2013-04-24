using System;
using UnityEngine;
using System.Collections;

public class LengtheningLegsCenter : MonoBehaviour
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
	/// The damping of the motor.
	/// </summary>
	public Single Damping;

	// Use this for initialization
	void Start()
	{
		var Legs = transform.GetComponentsInChildren<LengtheningLegMotor>();

		if (Legs.Length == 0)
		{
			enabled = false;
			return;
		}

		UpdateLegs();

		foreach (var Leg in Legs)
		{
			Leg.Retracted = true;
			Leg.Initialize();
		}
	}

	private void UpdateLegs()
	{
		var Legs = transform.GetComponentsInChildren<LengtheningLegMotor>();

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
		}
	}

	public void Update()
	{
		UpdateLegs();
	}
}
