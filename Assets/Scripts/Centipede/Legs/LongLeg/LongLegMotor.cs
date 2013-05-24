using System;
using System.Linq;
using UnityEngine;
using System.Collections;

/// <summary>
/// A lengthening leg motor, handled by the LengtheningLegsCenter.
/// The public members of this class should not be set directly, but via the
/// LengtheningLegsCenter to make all legs synchronized.
/// </summary>
public class LongLegMotor : LegMotor
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
	/// The prismatic joint of which the leg consists.
	/// </summary>
	private BasicPrismaticJoint[] PrismaticJoints;

	/// <summary>
	/// The remote anchors of the joints.
	/// </summary>
	private Vector3[] Anchors;

	public override void Initialize(Single Mass)
	{
		base.Initialize(Mass);
		Sole = transform.GetChild(0).gameObject;
		Sole.rigidbody.mass = Mass;

		RestraintDistance = Sole.GetComponent<RestraintDistance>();
		RestraintDistance.enabled = true;

		var LegOffsetInLink = new Vector3(transform.localPosition.x, 0, 0);
		Anchors = new[] { BackJointAnchor + LegOffsetInLink, FrontJointAnchor + LegOffsetInLink };

		PrismaticJoints = Enumerable.Range(0, 2).Select(Index =>
		{
			var PrismaticJoint = Sole.AddComponent<BasicPrismaticJoint>();
			PrismaticJoint.Initialize(ConnectedBody, Vector3.zero, Anchors[Index], Flexibility, ForceConstant, MotorRetractingForce, 0, RetractedLength, MaximumLength, DampingRate, false);
			return PrismaticJoint;
		}).ToArray();
		
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
			}
		}

		if (Retracted)
			Sole.collider.isTrigger = true;		
	}

	private Boolean JointsRetracted()
	{
		return PrismaticJoints.All(PrismaticJoint => PrismaticJoint.CurrentLength <= RetractedLength + PrismaticJoint.InitialLength + Sole.collider.bounds.size.y);
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
		Sole.collider.isTrigger = false;
		RestraintDistance.enabled = false;

		var CurrentTime = Time.time;

		// Making the leg become longer.
		foreach (var PrismaticJoint in PrismaticJoints)
		{
			PrismaticJoint.MaxMotorForce = MotorLengtheningForce;
			PrismaticJoint.State = BasicPrismaticJoint.MotorState.Forward;
			PrismaticJoint.MotorSpeed = MotorLengtheningSpeed;
			PrismaticJoint.CenterOnStop = false;
			PrismaticJoint.UpperLimit = MaximumLength;
		}

		// Waiting for the specified time.
		yield return new WaitForSeconds(LengtheningTime);

		Sole.collider.isTrigger = true;

		// Retracting the leg.
		foreach (var PrismaticJoint in PrismaticJoints)
		{
			PrismaticJoint.MaxMotorForce = MotorRetractingForce;
			PrismaticJoint.State = BasicPrismaticJoint.MotorState.Backward;
			PrismaticJoint.MotorSpeed = MotorRetractingSpeed;
		}

		// Waiting until the leg is retracted.
		while (!JointsRetracted())
			yield return new WaitForFixedUpdate();

		foreach (var PrismaticJoint in PrismaticJoints)
			PrismaticJoint.State = BasicPrismaticJoint.MotorState.Stopped;	

		RestraintDistance.enabled = true;
		while (!RestraintDistance.InArea())
			yield return new WaitForFixedUpdate();

		var FinishTime = Time.time;

		var SleepTime = Mathf.Max(ReloadingTime - ((FinishTime - CurrentTime) - LengtheningTime), 0);

		yield return new WaitForSeconds(SleepTime);

		Retracted = true;
	}

	protected override void Move(Single Direction)
	{
	}
}
