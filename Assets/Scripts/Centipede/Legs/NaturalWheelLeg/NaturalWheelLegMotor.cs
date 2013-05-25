using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// A wheel leg motor, handled by the WheelLegsCenter.
/// The public members of this class should not be set directly, but via the
/// WheelLegsCenter to make all legs synchronized.
/// </summary>
public class NaturalWheelLegMotor : LegMotor
{
	/// <summary>
	/// The radius of the wheel.
	/// </summary>
	private const Single WheelRadius = 0.2F;

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

	private Vector3 backSuspensionJointAnchor;
	/// <summary>
	/// The anchor of the back suspension joint, relative to the body.
	/// </summary>
	public Vector3 BackSuspensionJointAnchor
	{
		get { return backSuspensionJointAnchor + InitialTranform; }
		set { backSuspensionJointAnchor = value; }
	}

	private Vector3 frontSuspensionJointAnchor;
	/// <summary>
	/// The anchor of the front suspension joint, relative to the body.
	/// </summary>
	public Vector3 FrontSuspensionJointAnchor
	{
		get { return frontSuspensionJointAnchor + InitialTranform; }
		set { frontSuspensionJointAnchor = value; }
	}

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

	/// <summary>
	/// Indicates whether the leg is retracted.
	/// </summary>
	public Boolean Retracted;

	/// <summary>
	/// The wheel connected to the leg.
	/// </summary>
	private GameObject Wheel;

	/// <summary>
	/// The back joint created by the script.
	/// </summary>
	private BasicPrismaticJoint BackSuspensionJoint;

	/// <summary>
	/// The front joint created by the script.
	/// </summary>
	private BasicPrismaticJoint FrontSuspensionJoint;

	/// <summary>
	/// The initial transform of the wheel.
	/// This is used to adjust the suspension joints' remote anchors.
	/// </summary>
	private Vector3 InitialTranform;

	public override void Initialize(Single Mass)
	{
		base.Initialize(Mass);

		InitialTranform = transform.localPosition;

		Wheel = transform.GetChild(0).gameObject;
		Wheel.rigidbody.mass = Mass;

		BackSuspensionJoint = Wheel.AddComponent<BasicPrismaticJoint>();
		BackSuspensionJoint.Initialize(ConnectedBody, LegAnchor, BackSuspensionJointAnchor, 1, SuspensionJointsForceConstant, SuspensionJointsMaxMotorForce, 0, SuspensionLowerLimit, SuspensionUpperLimit, SuspensionDampingRate, true);

		FrontSuspensionJoint = Wheel.AddComponent<BasicPrismaticJoint>();
		FrontSuspensionJoint.Initialize(ConnectedBody, LegAnchor, FrontSuspensionJointAnchor, 1, SuspensionJointsForceConstant, SuspensionJointsMaxMotorForce, 0, SuspensionLowerLimit, SuspensionUpperLimit, SuspensionDampingRate, true);

		Retracted = true;

		UpdateValues();
	}

	protected override void UpdateValues()
	{
		base.UpdateValues();

		BackSuspensionJoint.Anchor = LegAnchor;
		BackSuspensionJoint.RemoteAnchor = BackSuspensionJointAnchor;
		BackSuspensionJoint.ForceConstant = SuspensionJointsForceConstant;
		BackSuspensionJoint.MaxMotorForce = SuspensionJointsMaxMotorForce;
		BackSuspensionJoint.DampingRate = SuspensionDampingRate;

		FrontSuspensionJoint.Anchor = LegAnchor;
		FrontSuspensionJoint.RemoteAnchor = FrontSuspensionJointAnchor;
		FrontSuspensionJoint.ForceConstant = SuspensionJointsForceConstant;
		FrontSuspensionJoint.MaxMotorForce = SuspensionJointsMaxMotorForce;
		FrontSuspensionJoint.DampingRate = SuspensionDampingRate;

		Wheel.rigidbody.maxAngularVelocity = MaximumAngularVelocity;

		// Using isTrigger and not enabled, because of a bug in Unity.
		//Wheel.collider.enabled = !Retracted;
		Wheel.collider.isTrigger = Retracted;

		if (Retracted)
		{
			BackSuspensionJoint.LowerLimit = SuspensionRetractedLength;
			BackSuspensionJoint.UpperLimit = SuspensionRetractedLength;
			BackSuspensionJoint.LowerLimit = SuspensionRetractedLength;
			FrontSuspensionJoint.UpperLimit = SuspensionRetractedLength;
		}
		else
		{
			BackSuspensionJoint.LowerLimit = SuspensionLowerLimit;
			BackSuspensionJoint.UpperLimit = SuspensionUpperLimit;
			FrontSuspensionJoint.LowerLimit = SuspensionLowerLimit;
			FrontSuspensionJoint.UpperLimit = SuspensionUpperLimit;
		}
	}

	protected override void PerformAction()
	{
		Retracted = !Retracted;
	}

	protected override void Move(Single Direction)
	{
		if (!Retracted)
		{
			var CurrentAngularVelocity = Wheel.rigidbody.angularVelocity.z;
			// Checking whether we are accelerating of braking.
			var Force = -Mathf.Sign(Direction);
			if (Mathf.Sign(Wheel.rigidbody.angularVelocity.z) == Force)
			{
				Force *= MotorTorque;

				var CurrentVelocity = Wheel.rigidbody.velocity.magnitude;
				var RotationsProgress = CurrentAngularVelocity * WheelRadius;
				if (Mathf.Abs(CurrentVelocity) < Mathf.Abs(RotationsProgress * (1 - MaximumSlipRate)))
					Force *= 0;
			}
			else
				Force *= BrakeTorque;

			Wheel.rigidbody.AddTorque(Vector3.forward * Force, ForceMode.Force);
			ConnectedBody.AddTorque(-Vector3.forward * Force, ForceMode.Force);
		}
	}

	private void OnGUI()
	{
		if (transform.parent.parent.transform.GetComponentsInChildren<NaturalWheelLegMotor>()[0] != this)
			return;
		var Position = Wheel.transform.position - Vector3.up;
		var Percents = Mathf.Abs(Wheel.rigidbody.angularVelocity.z)/MaximumAngularVelocity;
		var GUIPosition = Camera.mainCamera.WorldToScreenPoint(Position);

		GUI.HorizontalSlider(new Rect(GUIPosition.x, GUIPosition.y, 200, 20), Percents, 0, 1);
	}
}