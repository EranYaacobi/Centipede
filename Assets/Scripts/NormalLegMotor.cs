using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// A normal leg motor, handled by the NormalLegsCenter.
/// The public members of this class should not be set directly, but via the
/// NormalLegsCenter to make all legs synchronized.
/// </summary>
public class NormalLegMotor : LegMotor
{
	/// <summary>
	/// The anchor of the back spring, relative to the body.
	/// </summary>
	public Vector3 BackSpringAnchor;

	/// <summary>
	/// The anchor of the front spring, relative to the body.
	/// </summary>
	public Vector3 FrontSpringAnchor;

	/// <summary>
	/// The stiffness of the springs.
	/// </summary>
	public Single Stiffness;

	/// <summary>
	/// The minimum rate between the length of the springs, to their initial length.
	/// </summary>
	public Single MinimumLengthRatio;

	/// <summary>
	/// The maximum rate between the length of the springs, to their initial length.
	/// </summary>
	public Single MaximumLengthRatio;

	/// <summary>
	/// The angle of the leg.
	/// </summary>
	public Single Angle;

	/// <summary>
	/// The motors of the soles from which the normal leg consists of.
	/// </summary>
	private NormalLegSoleMotor[] SolesMotors;

	/// <summary>
	/// Indicates whether the leg is retracted.
	/// </summary>
	public Boolean Retracted;

	protected override void Initialize()
	{
		SolesMotors = transform.GetComponentsInChildren<NormalLegSoleMotor>();
		foreach (var SoleMotor in SolesMotors)
			SoleMotor.ConnectedBody = ConnectedBody;
		Common.Assert(SolesMotors.Length == 2);
		UpdateSoles(false);
	}

	private void UpdateSoles(Boolean UpdateLengths)
	{
		for (int i = 0; i < SolesMotors.Length; i++)
		{
			var SoleMotor = SolesMotors[i];
			SoleMotor.BackSpringAnchor = BackSpringAnchor;
			SoleMotor.FrontSpringAnchor = FrontSpringAnchor;
			SoleMotor.Stiffness = Stiffness;

			if (UpdateLengths)
			{
				if (!Retracted)
				{
					var SoleAngle = Mathf.Deg2Rad * (Angle + 180 * i);
					var BackSpringVariedLengthRate = (MaximumLengthRatio - MinimumLengthRatio) * (1 + Mathf.Sin(SoleAngle + Mathf.PI / 2)) / 2;
					var FrontSpringVariedLengthRate = (MaximumLengthRatio - MinimumLengthRatio) * (1 + Mathf.Sin(SoleAngle)) / 2;
					SoleMotor.BackSpringEquilibriumLength = (MinimumLengthRatio + BackSpringVariedLengthRate) * SoleMotor.InitialLength;
					SoleMotor.FrontSpringEquilibriumLength = (MinimumLengthRatio + FrontSpringVariedLengthRate) * SoleMotor.InitialLength;
				}
				else
				{
					SoleMotor.BackSpringEquilibriumLength = MinimumLengthRatio*SoleMotor.InitialLength;
					SoleMotor.FrontSpringEquilibriumLength = MinimumLengthRatio*SoleMotor.InitialLength;
				}
			}
		}
	}

	protected override void UpdateValues()
	{
		base.UpdateValues();
		UpdateSoles(true);
	}

	protected override void PerformAction()
	{
		Retracted = !Retracted;
		UpdateValues();
	}

	protected override void Move(Single Direction)
	{
		UpdateValues();
	}
}