using System;
using UnityEngine;
using System.Collections;

public abstract class LegMotor : MonoBehaviour
{
	/// <summary>
	/// The body to which the leg is connected.
	/// If nothing is assigned, the script assumes that the connected body is
	/// the parent.
	/// </summary>
	public Rigidbody ConnectedBody;

	/// <summary>
	/// The anchor of the leg.
	/// </summary>
	public Vector3 LegAnchor;

	/// <summary>
	/// The input button which represents the leg action.
	/// </summary>
	public String InputButton;

	/// <summary>
	/// Stores the last value of the horizontal axis.
	/// </summary>
	private Single LastHorizontalMovement = 0;

	/// <summary>
	/// Inidicates whether the script was initialized;
	/// </summary>
	private Boolean Initialized;

	private void Start()
	{
		LastHorizontalMovement = 0;
		if (ConnectedBody == null)
			ConnectedBody = transform.parent.gameObject.rigidbody;
	}

	/// <summary>
	/// Custom initialization for inherited class.
	/// Derived implementations should set the mass of the leg.
	/// </summary>
	public virtual void Initialize(Single Mass)
	{
		Initialized = true;
	}

	private void Update()
	{
		if (Initialized)
		{
			UpdateValues();

			if (Input.GetButtonUp(InputButton))
				PerformAction();
		}
	}

	private void FixedUpdate()
	{
		if (Initialized)
		{
			FixedUpdateValues();

			var Horizontal = Input.GetAxis(Keys.Horizontal);
			if (Horizontal != 0)
			{
				Move(Horizontal);
			}
			else
			{
				if (LastHorizontalMovement != 0)
					StopMoving();
			}
			LastHorizontalMovement = Horizontal;
		}
	}

	/// <summary>
	/// Performs custom value updates, during the Update phase.
	/// </summary>
	protected virtual void UpdateValues()
	{
	}

	/// <summary>
	/// Performs custom value updates, during the FixedUpdate phase.
	/// </summary>
	protected virtual void FixedUpdateValues()
	{
	}

	/// <summary>
	/// Performs custom action when the leg's action button is pressed.
	/// </summary>
	protected abstract void PerformAction();

	/// <summary>
	/// Performs custom movement, in the given direction.
	/// </summary>
	/// <param name="Direction"></param>
	protected abstract void Move(Single Direction);

	/// <summary>
	/// Performs custom action upon stopping.
	/// </summary>
	protected virtual void StopMoving()
	{
	}
}