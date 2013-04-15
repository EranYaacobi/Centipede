using System;
using UnityEngine;
using System.Collections;

public class NormalFeetCenter : MonoBehaviour
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
	/// The amount of time it takes the motor to complete a full circle.
	/// </summary>
	public Single CycleInterval;

	/// <summary>
	/// The minimum rate between the length of the springs, to their initial length.
	/// </summary>
	public Single MinimumLengthRatio;

	/// <summary>
	/// The maximum rate between the length of the springs, to their initial length.
	/// </summary>
	public Single MaximumLengthRatio;

	/// <summary>
	/// The position in the current cycle.
	/// </summary>
	private Single CurrentIntervalTime;

	// Use this for initialization
	void Start()
	{
		var Feet = transform.GetComponentsInChildren<NormalFootMotor>();

		if (Feet.Length == 0)
		{
			enabled = false;
			return;
		}

		CurrentIntervalTime = 0;

		UpdateFeet(false);
	}

	private void UpdateFeet(Boolean UpdateLengths)
	{
		var Feet = transform.GetComponentsInChildren<NormalFootMotor>();

		for (int i = 0; i < Feet.Length; i++)
		{
			var Foot = Feet[i];
			Foot.BackSpringAnchor = BackSpringAnchor;
			Foot.FrontSpringAnchor = FrontSpringAnchor;
			Foot.Stiffness = Stiffness;

			if (UpdateLengths)
			{
				if (!Foot.Retracted)
				{
					var FeetCycleIntervalTime = ((i*CycleInterval)/Feet.Length + CurrentIntervalTime)%CycleInterval;
					var IntervalAngle = Mathf.PI*2*FeetCycleIntervalTime/CycleInterval;
					var BackSpringAngleLengthRate = (1 + Mathf.Sin(IntervalAngle))/2;
					var FrontSpringAngleLengthRate = (1 + Mathf.Sin(IntervalAngle + Mathf.PI / 2))/2;
					Foot.BackSpringEquilibriumLength = (MinimumLengthRatio +
					                                    (MaximumLengthRatio - MinimumLengthRatio)*BackSpringAngleLengthRate)*
					                                   Foot.InitialLength;
					Foot.FrontSpringEquilibriumLength = (MinimumLengthRatio +
					                                     (MaximumLengthRatio - MinimumLengthRatio)*FrontSpringAngleLengthRate)*
					                                    Foot.InitialLength;
				}
				else
				{
					Foot.BackSpringEquilibriumLength = MinimumLengthRatio*Foot.InitialLength;
					Foot.FrontSpringEquilibriumLength = MinimumLengthRatio*Foot.InitialLength;
				}
			}
		}
	}

	public void Update()
	{
		CurrentIntervalTime += Input.GetAxis(Keys.Horizontal) * Time.deltaTime;
		UpdateFeet(true);
	}
}
