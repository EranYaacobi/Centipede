using System;
using System.Linq;
using UnityEngine;
using System.Collections;

/// <summary>
/// A lengthening leg, handled by the LengtheningLegsCenter.
/// The public members of this class should not be set directly, but via the
/// LengtheningLegsCenter to make all legs synchronized.
/// </summary>
public class LongLeg : Leg
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
	/// The force of the motor when lengthening.
	/// </summary>
	public Single MotorLengtheningForce;

	/// <summary>
	/// The speed of the motor when retracting.
	/// </summary>
	public Single MotorRetractingForce;

	/// <summary>
	/// The speed of the motor when lengthening.
	/// </summary>
	public Single MotorLengtheningSpeed;

	/// <summary>
	/// The speed of the motor when retracting.
	/// </summary>
	public Single MotorRetractingSpeed;

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

	/// <summary>
	/// Indicates whether the leg is retracted.
	/// </summary>
	public Boolean Retracted;

	/// <summary>
	/// The leg's sole.
	/// </summary>
	private GameObject Sole;

	/// <summary>
	/// The RestraintDistance script that restraints the distance of the sole from its origin.
	/// </summary>
	private RestraintDistance RestraintDistance;

	/// <summary>
	/// The RestraintRotation script that restraints the rotation of the sole to its parent.
	/// </summary>
	private RestraintRotation RestraintRotation;

	/// <summary>
	/// The prismatic joint of which the leg consists.
	/// </summary>
	private BasicPrismaticJoint[] PrismaticJoints;

	/// <summary>
	/// The remote anchors of the joints.
	/// </summary>
	private Vector3[] Anchors;

	/// <summary>
	/// Stores the sole's height.
	/// This is used because the collider's bound is zeroed when the collider is disabled.
	/// </summary>
	private Single SoleHeight;

	public override void Initialize(Single Mass)
	{
		base.Initialize(Mass);
		Sole = transform.GetChild(0).gameObject;
		Sole.rigidbody.mass = Mass;

		RestraintDistance = Sole.GetComponent<RestraintDistance>();
		RestraintDistance.enabled = true;
		RestraintRotation = Sole.GetComponent<RestraintRotation>();

		var LegOffsetInLink = new Vector3(transform.localPosition.x, 0, 0);
		Anchors = new[] { BackJointAnchor + LegOffsetInLink, FrontJointAnchor + LegOffsetInLink };

		PrismaticJoints = Enumerable.Range(0, 2).Select(Index =>
		{
			var PrismaticJoint = Sole.AddComponent<BasicPrismaticJoint>();
			PrismaticJoint.Initialize(ConnectedBody, Vector3.zero, Anchors[Index], Flexibility, ForceConstant, MotorRetractingForce, 0, RetractedLength, MaximumLength, DampingRate, true, true);
			return PrismaticJoint;
		}).ToArray();

		SoleHeight = Sole.collider.bounds.size.y;

		Sole.rigidbody.useGravity = false;
		
		UpdateValues();
	}


	protected override void UpdateValues()
	{
		base.UpdateValues();

		var LegOffsetInLink = new Vector3(transform.localPosition.x, 0, 0);
		Anchors[0] = BackJointAnchor + LegOffsetInLink;
		Anchors[1] = FrontJointAnchor + LegOffsetInLink;

		for (int i = 0; i < PrismaticJoints.Length; i++)
		{
			var PrismaticJoint = PrismaticJoints[i];

			PrismaticJoint.Anchor = Vector3.zero;
			PrismaticJoint.RemoteAnchor = Anchors[i];
			PrismaticJoint.LowerLimit = RetractedLength;
			PrismaticJoint.Flexibility = Flexibility;
			PrismaticJoint.ForceConstant = ForceConstant;
			PrismaticJoint.DampingRate = DampingRate;

			if (Retracted)
			{
				PrismaticJoint.MaxMotorForce = MotorRetractingForce;
				PrismaticJoint.UpperLimit = RetractedLength;
				PrismaticJoint.CenterOnStop = true;
				PrismaticJoint.DisableUpwardMovement = true;
			}
		}

		if (Retracted)
			Sole.collider.enabled = false;		
	}

	private Boolean JointsRetracted()
	{
		return PrismaticJoints.All(PrismaticJoint => PrismaticJoint.CurrentLength <= RetractedLength + PrismaticJoint.InitialLength + SoleHeight);
	}

	protected override bool CheckActionInput()
	{
		return PlayerInput.GetPlayerInput(Owner).ButtonDown(InputButton);
	}

	protected override void PerformAction()
	{
		if ((Retracted) && (RestraintDistance.InArea()))
		{
			Retracted = false;
			StartCoroutine(Lengthen());
		}
	}

	private IEnumerator Lengthen()
	{
		var PreviousConstraints = Sole.rigidbody.constraints;

		Sole.collider.enabled = true;
		Sole.rigidbody.useGravity = true;
		RestraintDistance.enabled = false;
		RestraintRotation.enabled = false;
		Sole.rigidbody.freezeRotation = true;

		// Making the leg become longer.
		foreach (var PrismaticJoint in PrismaticJoints)
		{
			PrismaticJoint.MaxMotorForce = MotorLengtheningForce;
			PrismaticJoint.State = BasicPrismaticJoint.MotorState.Forward;
			PrismaticJoint.MotorSpeed = MotorLengtheningSpeed;
			PrismaticJoint.CenterOnStop = false;
			PrismaticJoint.UpperLimit = MaximumLength;
		}

		yield return new WaitForSeconds(0.1F);
		// Enabling the leg to move in any direction.
		foreach (var PrismaticJoint in PrismaticJoints)
			PrismaticJoint.DisableUpwardMovement = false;

		// Waiting for the specified time.
		yield return new WaitForSeconds(LengtheningTime - 0.1F);

		Sole.collider.enabled = false;
		Sole.rigidbody.freezeRotation = false;
		Sole.rigidbody.constraints = PreviousConstraints;
		RestraintRotation.enabled = true;

		var CurrentTime = Time.time;

		// Retracting the leg.
		foreach (var PrismaticJoint in PrismaticJoints)
		{
			PrismaticJoint.MaxMotorForce = MotorRetractingForce;
			PrismaticJoint.State = BasicPrismaticJoint.MotorState.Backward;
			PrismaticJoint.MotorSpeed = MotorRetractingSpeed;
			PrismaticJoint.DisableUpwardMovement = true;
		}

		Sole.rigidbody.useGravity = false;

		// Waiting until the leg is in its restrained area, and the joints are retracted.
		while ((!JointsRetracted()) || (!RestraintDistance.InArea()))
			yield return new WaitForFixedUpdate();

		foreach (var PrismaticJoint in PrismaticJoints)
		{
			PrismaticJoint.State = BasicPrismaticJoint.MotorState.Stopped;
			PrismaticJoint.MaxMotorForce = MotorRetractingForce;
			PrismaticJoint.UpperLimit = RetractedLength;
			PrismaticJoint.CenterOnStop = true;
			PrismaticJoint.DisableUpwardMovement = true;
		}

		RestraintDistance.enabled = true;

		var ActualReloadingTime = Time.time - CurrentTime;

		var SleepTime = Mathf.Max(ReloadingTime - ActualReloadingTime, 0);

		yield return new WaitForSeconds(SleepTime);

		Retracted = true;
	}

	protected override void Move(Single Direction)
	{
	}
}
