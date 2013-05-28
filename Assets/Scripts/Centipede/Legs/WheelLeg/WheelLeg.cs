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
	/// The graphics of the wheel.
	/// </summary>
	public Transform WheelTransform;

	/// <summary>
	/// The physics of the wheel.
	/// This includes not only to wheel (and as such, not a part of it), but the wheel's pivot.
	/// </summary>
	public WheelCollider WheelCollider;

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

	private Boolean retracted;
	/// <summary>
	/// Indicates whether the leg is retracted.
	/// </summary>
	public Boolean Retracted
	{
		get { return retracted; }
		set
		{
			retracted = value;
			WheelCollider.enabled = !retracted;
		}
	}

	/// <summary>
	/// The link to which the leg is connected.
	/// All force is applied on the link, and not the wheel, to make
	/// the movement move smooth.
	/// </summary>
	private GameObject ParentLink;

	/// <summary>
	/// The wheel connected to the leg.
	/// </summary>
	private GameObject Wheel;

	/// <summary>
	/// The RestraintRotation script that restricts the rotation of the wheel.
	/// </summary>
	private RestraintRotation RestraintRotation;

	/// <summary>
	/// The velocity of the wheel.
	/// </summary>
	private Vector3 WheelVelocity;

	/// <summary>
	/// The amount of ground speed that the wheel covered.
	/// Used to calculate by how much the wheel's graphic should be rotated.
	/// </summary>
	private Vector3 GroundSpeed;

	/// <summary>
	/// Indicates whether the body can actually drive, meaning that the wheel is touching the ground.
	/// </summary>
	public Boolean CanDrive;

	/// <summary>
	/// The throttle applied by the engine.
	/// </summary>
	private Single Throttle;

	/// <summary>
	/// The relative velocity of the wheel.
	/// </summary>
	private Vector3 RelativeVelocity;

	public override void Initialize(Single Mass)
	{
		base.Initialize(Mass);

		ParentLink = transform.parent.gameObject;
		Wheel = transform.GetChild(0).gameObject;
		Wheel.rigidbody.mass = Mass;

		RestraintRotation = GetComponentInChildren<RestraintRotation>();
		RestraintRotation.enabled = false;

		// Connecting the wheel to the parent link, with a hinge joint.
		var FixedJoint = ParentLink.AddComponent<FixedJoint>();
		FixedJoint.connectedBody = Wheel.rigidbody;
		FixedJoint.anchor = Vector3.zero;
		FixedJoint.axis = Vector3.forward;

		Retracted = true;
		CanDrive = false;

		UpdateValues();
	}


	protected override void UpdateValues()
	{
		base.UpdateValues();

		WheelCollider.suspensionDistance = Retracted ? RetractedSuspensionDistance : SuspensionDistance;
		WheelCollider.suspensionSpring = new JointSpring
		                                 	{
		                                 		spring = SuspensionSpring,
												damper = SuspensionDamper,
		                                 		targetPosition = SuspensionTargetPosition
		                                 	};

		UpdateWheelGraphics();
	}

	protected override void FixedUpdateValues()
	{
		base.FixedUpdateValues();

		RelativeVelocity = Wheel.transform.InverseTransformDirection(Wheel.rigidbody.velocity);
		CalculateState();
		FixRotationRestraints();

		if (Retracted)
			return;
		UpdateDrag();
		UpdateFriction();
	}

	protected override void PerformAction()
	{
		Retracted = !Retracted;
	}

	protected override void Move(Single Direction)
	{
		if (!Retracted)
			ApplyThrottle(Direction);
	}

	protected override void StopMoving()
	{
		base.StopMoving();

		Throttle = 0;
	}

	private void UpdateWheelGraphics()
	{
		WheelHit WheelHit;

		// First we get the velocity at the point where the wheel meets the ground, if the wheel is touching the ground
		if ((CanDrive) && (WheelCollider.GetGroundHit(out WheelHit)))
		{
			var AngleBetween = Vector3.Angle(WheelHit.normal, WheelCollider.transform.up) * Mathf.Deg2Rad;
			var WheelPositionFromGround = WheelHit.normal * WheelCollider.radius;
			var DistanceBetweenPositions = Mathf.Min(WheelCollider.suspensionDistance, WheelPositionFromGround.magnitude / Mathf.Cos(AngleBetween));
			WheelTransform.position = WheelHit.point + WheelCollider.transform.up * DistanceBetweenPositions;
			WheelVelocity = Wheel.rigidbody.GetPointVelocity(WheelHit.point);
			GroundSpeed = Wheel.transform.InverseTransformDirection(WheelVelocity);
		}
		else
		{
			// If the wheel is not touching the ground we set the position of the wheel graphics to
			// the wheel's transform position + the range of the suspension.
			WheelTransform.position = WheelCollider.transform.position - WheelCollider.transform.up * WheelCollider.suspensionDistance;

			WheelVelocity *= 0.9F * (1 - Throttle);
		}

		// Rotate the wheel based on the distance it passed during the frame.
		WheelTransform.Rotate(Vector3.back * (GroundSpeed.x / WheelCollider.radius) * Time.deltaTime * Mathf.Rad2Deg);
	}

	private void OnDrawGizmosSelected()
	{
		const Single Depth = -1F;

		if (!CanDrive)
			return;

		WheelHit WheelHit;
		if (!WheelCollider.GetGroundHit(out WheelHit)) 
			return;

		var AngleBetween = Vector3.Angle(WheelHit.normal, WheelCollider.transform.up) * Mathf.Deg2Rad;

		var WheelPositionFromGround = WheelHit.normal * WheelCollider.radius;
		var DistanceBetweenPositions = WheelPositionFromGround.magnitude / Mathf.Cos(AngleBetween);
		var WheelPositionOnSuspensionSpring = WheelCollider.transform.up * DistanceBetweenPositions;

		var WheelHitPoint = WheelHit.point + new Vector3(0, 0, Depth);
		var WheelColliderPosition = WheelCollider.transform.position + new Vector3(0, 0, Depth);

		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(WheelHitPoint, 0.05F);
		Gizmos.DrawLine(WheelHitPoint, WheelHitPoint + WheelPositionFromGround);
		Gizmos.DrawLine(WheelHitPoint, WheelHitPoint + WheelPositionOnSuspensionSpring);

		Gizmos.color = Color.green;
		Gizmos.DrawSphere(WheelColliderPosition, 0.05F);
		Gizmos.DrawLine(WheelColliderPosition, WheelColliderPosition + WheelCollider.transform.up);
	}

	private void CalculateState()
	{
		CanDrive = WheelCollider.isGrounded;
	}

	/// <summary>
	/// Fixes rotations restraint on the wheel, so it will be perpendicular to the ground
	/// when touching it, and perpendicular to the link when it doesn't.
	/// </summary>
	private void FixRotationRestraints()
	{
		WheelHit WheelHit;

		if ((CanDrive) && (WheelCollider.GetGroundHit(out WheelHit)))
		{
			var GroundAngle = Vector3.Angle(Vector3.right, WheelHit.normal) - 90;
			Wheel.transform.rotation = Quaternion.AngleAxis(GroundAngle, Vector3.forward);
		}
		else
		{
			RestraintRotation.Restraint();
		}
	}

	private void UpdateDrag()
	{
		if (!CanDrive)
			return;

		var Drag = new Vector3(-RelativeVelocity.x*Mathf.Abs(RelativeVelocity.x), 0, 0);

		// TODO: This is fairly inaccurate calculation of drag, which assumes that the air friction
		// here is the radius of the wheel.
		Wheel.rigidbody.AddForce(Drag * WheelCollider.radius * Wheel.rigidbody.mass);
	}

	private void ApplyThrottle(Single Direction)
	{
		if (!CanDrive)
			return;

		Throttle = Mathf.Sign(Direction);
		var Force = Throttle * MotorForce;

		// Checking whether we are accelerating of braking.
		if (Mathf.Sign(RelativeVelocity.x) == Throttle)
			Force *= MotorForce;
		else
			Force *= BrakeForce;

		Wheel.rigidbody.AddForce(Wheel.transform.right * Force, ForceMode.Force);
	}

	// Not necessarily needed (the usage here is irrelevant, yet we want to change the friction based on the ground's physics material
	private void UpdateFriction()
	{
		WheelHit WheelHit;
		if (!WheelCollider.GetGroundHit(out WheelHit))
			return;

		var TempFriction = WheelCollider.forwardFriction;
		TempFriction.stiffness = WheelHit.collider.material.staticFriction;
		WheelCollider.forwardFriction = TempFriction;

		TempFriction = WheelCollider.sidewaysFriction;
		TempFriction.stiffness = WheelHit.collider.material.staticFriction;
		WheelCollider.sidewaysFriction = TempFriction;
	}
}
