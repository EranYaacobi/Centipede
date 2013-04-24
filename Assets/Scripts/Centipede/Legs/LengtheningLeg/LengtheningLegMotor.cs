using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// A lengthening leg motor, handled by the LengtheningLegsCenter.
/// The public members of this class should not be set directly, but via the
/// LengtheningLegsCenter to make all legs synchronized.
/// </summary>
public class LengtheningLegMotor : LegMotor
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

	/// <summary>
	/// The script that restrains the leg.
	/// </summary>
	private RestraintRotation RestraintRotation;

	public override void Initialize()
	{
		base.Initialize();
		var Sole = transform.GetChild(0).gameObject;

		Anchors[0] = BackJointAnchor;
		Anchors[1] = FrontJointAnchor;

		RestraintRotation = GetComponent<RestraintRotation>();

		for (int i = 0; i < PrismaticJoints.Length; i++)
		{
			PrismaticJoints[i] = Sole.AddComponent<BasicPrismaticJoint>();
			PrismaticJoints[i].Initialize(ConnectedBody, Anchors[i], Anchors[i], Flexibility, ForceConstant, MotorRetractingForce, 0, RetractedLength, MaximumLength, Damping);
		}
		
		Retracted = true;
		UpdateValues();
	}


	protected override void UpdateValues()
	{
		base.UpdateValues();

		Anchors[0] = BackJointAnchor;
		Anchors[1] = FrontJointAnchor;

		var TempRetracted = Retracted;
		for (int i = 0; i < PrismaticJoints.Length; i++)
		{
			var PrismaticJoint = PrismaticJoints[i];

			PrismaticJoint.Anchor = Anchors[i];
			PrismaticJoint.RemoteAnchor = Anchors[i];
			PrismaticJoint.LowerLimit = RetractedLength;
			PrismaticJoint.UpperLimit = MaximumLength;
			PrismaticJoint.Flexibility = Flexibility;
			PrismaticJoint.ForceConstant = ForceConstant;

			if (TempRetracted)
			{
				RestraintRotation.ReverseRestraint = false;
				PrismaticJoint.MaxMotorForce = MotorRetractingForce;
				PrismaticJoint.State = BasicPrismaticJoint.MotorState.Backward;
				PrismaticJoint.MotorSpeed = MotorRetractingSpeed;
				if (PrismaticJoint.CurrentLength <= RetractedLength + PrismaticJoint.InitialLength)
					PrismaticJoint.State = BasicPrismaticJoint.MotorState.Stopped;

			}
			else
			{
				RestraintRotation.ReverseRestraint = true;
				PrismaticJoint.MaxMotorForce = MotorLengtheningForce;
				PrismaticJoint.State = BasicPrismaticJoint.MotorState.Forward;
				PrismaticJoint.MotorSpeed = MotorLengtheningSpeed;
				if (PrismaticJoint.CurrentLength >= MaximumLength + PrismaticJoint.InitialLength)
				{
					PrismaticJoint.State = BasicPrismaticJoint.MotorState.Backward;
					TempRetracted = true;
				}
			}

			PrismaticJoint.Damping = Damping;
		}

		Retracted = TempRetracted;
	}

	protected override void PerformAction()
	{
		if (PrismaticJoints[0].CurrentLength <= RetractedLength + PrismaticJoints[0].InitialLength + 0.1)
			Retracted = false;
	}

	protected override void Move(Single Direction)
	{
	}
}
