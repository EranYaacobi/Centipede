using System;
using UnityEngine;
using System.Collections;

public class NormalLegsCenter : MonoBehaviour
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
		var Legs = transform.GetComponentsInChildren<NormalLegMotor>();

		if (Legs.Length == 0)
		{
			enabled = false;
			return;
		}

		CurrentIntervalTime = 0;

		UpdateLegs();
	}

	private void UpdateLegs()
	{
		var Legs = transform.GetComponentsInChildren<NormalLegMotor>();

		foreach (var Leg in Legs)
		{
			Leg.BackSpringAnchor = BackSpringAnchor;
			Leg.FrontSpringAnchor = FrontSpringAnchor;
			Leg.Stiffness = Stiffness;
			Leg.Angle = (CurrentIntervalTime/CycleInterval)*360;
			Leg.MinimumLengthRatio = MinimumLengthRatio;
			Leg.MaximumLengthRatio = MaximumLengthRatio;
		}
	}

	public void Update()
	{
		CurrentIntervalTime += Input.GetAxis(Keys.Horizontal) * Time.deltaTime;
		//CurrentIntervalTime = (CurrentIntervalTime + 1 * Time.deltaTime + CycleInterval) % CycleInterval;
		UpdateLegs();
	}
}
