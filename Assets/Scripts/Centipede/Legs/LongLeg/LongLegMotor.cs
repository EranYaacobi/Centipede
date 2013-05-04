using System;
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
	/// The damping of the motor.
	/// Ranges from 0 (no damping) to 1 (critial damping).
	/// </summary>
	[Range(0, 1)]
	public Single DampingRate;

	/// <summary>
	/// Indicates whether the desired length should be set to the center, when the motor stops.
	/// </summary>
	public Boolean CenterOnStop;

	/// <summary>
	/// Indicates whether the leg is retracted.
	/// </summary>
	public Boolean Retracted;

	/// <summary>
	/// The prismatic joint of which the leg consists.
	/// </summary>
	private readonly BasicPrismaticJoint[] PrismaticJoints = new BasicPrismaticJoint[2];

	/// <summary>
	/// The remote anchors of the joints.
	/// </summary>
	private readonly Vector3[] Anchors = new Vector3[2];

	public override void Initialize()
	{
		base.Initialize();
		var Sole = transform.GetChild(0).gameObject;

		Anchors[0] = BackJointAnchor;
		Anchors[1] = FrontJointAnchor;

		for (int i = 0; i < PrismaticJoints.Length; i++)
		{
			PrismaticJoints[i] = Sole.AddComponent<BasicPrismaticJoint>();
			PrismaticJoints[i].Initialize(ConnectedBody, Anchors[i], Anchors[i], Flexibility, ForceConstant, MotorRetractingForce, 0, RetractedLength, MaximumLength, DampingRate, CenterOnStop);
		}
		
		Retracted = true;
		UpdateValues();
	}


	protected override void UpdateValues()
	{
		base.UpdateValues();

		Anchors[0] = BackJointAnchor;
		Anchors[1] = FrontJointAnchor;

		for (int i = 0; i < PrismaticJoints.Length; i++)
		{
			var PrismaticJoint = PrismaticJoints[i];

			PrismaticJoint.Anchor = Vector3.zero;
			PrismaticJoint.RemoteAnchor = Anchors[i];
			PrismaticJoint.LowerLimit = RetractedLength;
			PrismaticJoint.UpperLimit = MaximumLength;
			PrismaticJoint.Flexibility = Flexibility;
			PrismaticJoint.ForceConstant = ForceConstant;
			PrismaticJoint.DampingRate = DampingRate;
			PrismaticJoint.CenterOnStop = CenterOnStop;
		}
	}

	protected override void PerformAction()
	{
		if (PrismaticJoints[0].CurrentLength <= RetractedLength + PrismaticJoints[0].InitialLength + 0.1)
		{
			Retracted = false;
			StartCoroutine(Lengthen());
		}
	}

	private IEnumerator Lengthen()
	{
		// Making the become longer.
		foreach (var PrismaticJoint in PrismaticJoints)
		{
			PrismaticJoint.MaxMotorForce = MotorLengtheningForce;
			PrismaticJoint.State = BasicPrismaticJoint.MotorState.Forward;
			PrismaticJoint.MotorSpeed = MotorLengtheningSpeed;
		}

		// Waiting for the specified time.
		yield return new WaitForSeconds(LengtheningTime);

		// Retracting the leg.
		foreach (var PrismaticJoint in PrismaticJoints)
		{
			PrismaticJoint.MaxMotorForce = MotorRetractingForce;
			PrismaticJoint.State = BasicPrismaticJoint.MotorState.Backward;
			PrismaticJoint.MotorSpeed = MotorRetractingSpeed;
		}

		// Waiting until the leg is retracted.
		var JointsRunning = PrismaticJoints.Length;
		while (true)
		{
			foreach (var PrismaticJoint in PrismaticJoints)
			{
				if (PrismaticJoint.State != BasicPrismaticJoint.MotorState.Stopped)
				{
					if (PrismaticJoint.CurrentLength <= RetractedLength + PrismaticJoint.InitialLength)
					{
						PrismaticJoint.State = BasicPrismaticJoint.MotorState.Stopped;
						JointsRunning -= 1;
					}
				}
			}

			if (JointsRunning == 0)
				break;

			yield return new WaitForFixedUpdate();
		}

		Retracted = true;
	}

	protected override void Move(Single Direction)
	{
	}
}
