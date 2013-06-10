using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// A wheel leg, handled by the WheelLegsCenter.
/// The public members of this class should not be set directly, but via the
/// WheelLegsCenter to make all legs synchronized.
/// </summary>
public class WheelLeg : Leg
{
	/// <summary>
	/// The radius of the wheel.
	/// </summary>
	public const Single WheelRadius = 0.2F;

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
	/// The minimum slip rate after which the motor applies less force.
	/// </summary>
	[Range(0, 1)]
	public Single MinimumSlipRate;

	/// <summary>
	/// The maximum slip rate after which the motor shuts down.
	/// </summary>
	[Range(0, 1)]
	public Single MaximumSlipRate;

	/// <summary>
	/// The minimum required angular velocity for checking if the wheel is slipping.
	/// </summary>
	public Single MinimumSlipAngularVelocity;

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
	/// Indicates whether the wheel is currently touching anything.
	/// </summary>
	private Boolean CollidingSomething;

	public override void Initialize(Single Mass)
	{
		base.Initialize(Mass);

		Wheel = transform.GetChild(0).gameObject;
		Wheel.rigidbody.mass = Mass;

		var LegOffsetInLink = new Vector3(transform.localPosition.x, 0, 0);
		BackSuspensionJoint = Wheel.AddComponent<BasicPrismaticJoint>();
		BackSuspensionJoint.Initialize(ConnectedBody, LegAnchor, BackSuspensionJointAnchor + LegOffsetInLink, 1, SuspensionJointsForceConstant, SuspensionJointsMaxMotorForce, 0, SuspensionLowerLimit, SuspensionUpperLimit, SuspensionDampingRate, true, true);

		FrontSuspensionJoint = Wheel.AddComponent<BasicPrismaticJoint>();
		FrontSuspensionJoint.Initialize(ConnectedBody, LegAnchor, FrontSuspensionJointAnchor + LegOffsetInLink, 1, SuspensionJointsForceConstant, SuspensionJointsMaxMotorForce, 0, SuspensionLowerLimit, SuspensionUpperLimit, SuspensionDampingRate, true, true);

		Retracted = true;

		UpdateValues();
	}

	protected override void UpdateValues()
	{
		base.UpdateValues();

		var LegOffsetInLink = new Vector3(transform.localPosition.x, 0, 0);
		BackSuspensionJoint.Anchor = LegAnchor;
		BackSuspensionJoint.RemoteAnchor = BackSuspensionJointAnchor + LegOffsetInLink;
		BackSuspensionJoint.ForceConstant = SuspensionJointsForceConstant;
		BackSuspensionJoint.MaxMotorForce = SuspensionJointsMaxMotorForce;
		BackSuspensionJoint.DampingRate = SuspensionDampingRate;

		FrontSuspensionJoint.Anchor = LegAnchor;
		FrontSuspensionJoint.RemoteAnchor = FrontSuspensionJointAnchor + LegOffsetInLink;
		FrontSuspensionJoint.ForceConstant = SuspensionJointsForceConstant;
		FrontSuspensionJoint.MaxMotorForce = SuspensionJointsMaxMotorForce;
		FrontSuspensionJoint.DampingRate = SuspensionDampingRate;

		Wheel.rigidbody.maxAngularVelocity = MaximumAngularVelocity;

		// Using isTrigger and not enabled, because of a bug in Unity.
		//Wheel.collider.enabled = !Retracted;
		Wheel.collider.isTrigger = Retracted;

		if (Retracted)
		{
			BackSuspensionJoint.LowerLimit = Mathf.Lerp(BackSuspensionJoint.LowerLimit, SuspensionRetractedLength, 10 * Time.deltaTime);
			BackSuspensionJoint.UpperLimit = Mathf.Lerp(BackSuspensionJoint.UpperLimit, SuspensionRetractedLength, 10 * Time.deltaTime);
			FrontSuspensionJoint.LowerLimit = Mathf.Lerp(FrontSuspensionJoint.LowerLimit, SuspensionRetractedLength, 10 * Time.deltaTime);
			FrontSuspensionJoint.UpperLimit = Mathf.Lerp(FrontSuspensionJoint.UpperLimit, SuspensionRetractedLength, 10 * Time.deltaTime);

			BackSuspensionJoint.RemoteAnchor += 0.15F * Vector3.up;
			FrontSuspensionJoint.RemoteAnchor += 0.15F * Vector3.up;
		}
		else
		{
			BackSuspensionJoint.LowerLimit = Mathf.Lerp(BackSuspensionJoint.LowerLimit, SuspensionLowerLimit, 10 * Time.deltaTime);
			BackSuspensionJoint.UpperLimit = Mathf.Lerp(BackSuspensionJoint.UpperLimit, SuspensionUpperLimit, 10 * Time.deltaTime);
			FrontSuspensionJoint.LowerLimit = Mathf.Lerp(FrontSuspensionJoint.LowerLimit, SuspensionLowerLimit, 10 * Time.deltaTime);
			FrontSuspensionJoint.UpperLimit = Mathf.Lerp(FrontSuspensionJoint.UpperLimit, SuspensionUpperLimit, 10 * Time.deltaTime);
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

				if (CollidingSomething)
				{
					if (MinimumSlipAngularVelocity < Mathf.Abs(CurrentAngularVelocity))
					{
						var CurrentVelocity = Mathf.Abs(Wheel.rigidbody.velocity.magnitude);
						var WheelVelocity = Mathf.Abs(CurrentAngularVelocity*WheelRadius);
						var MinimumSlipVelocity = WheelVelocity*(1 - MinimumSlipRate);

						if (CurrentVelocity < MinimumSlipVelocity)
						{
							// The wheel is slipping a little
							var MaximumSlipVelocity = WheelVelocity*(1 - MaximumSlipRate);
							if (CurrentVelocity < MaximumSlipVelocity)
							{
								// The wheel is slipping to much.
								// Shuting down engine.
								Force = 0;
							}
							else
							{
								Force *= (CurrentVelocity - MaximumSlipVelocity)/(MinimumSlipVelocity - MaximumSlipVelocity);
							}
						}
					}
				}
			}
			else
				Force *= BrakeTorque;

			Wheel.rigidbody.AddTorque(Vector3.forward * Force, ForceMode.Force);
			ConnectedBody.AddTorque(-Vector3.forward * Force, ForceMode.Force);
		}
	}

	protected void OnCollisionEnter(Collision Collision)
	{
		CollidingSomething = true;
	}

	protected void OnCollisionStay(Collision Collision)
	{
		CollidingSomething = true;
	}

	protected void OnCollisionExit(Collision Collision)
	{
		CollidingSomething = true;
	}
}