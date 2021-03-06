using System;
using UnityEngine;
using System.Collections;

public abstract class Leg : Photon.MonoBehaviour
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
	/// The owner of the centipede.
	/// </summary>
	public PhotonPlayer Owner;

	/// <summary>
	/// Stores the current value of the horizontal axis.
	/// </summary>
	private Single HorizontalMovement;

	/// <summary>
	/// Stores the last value of the horizontal axis.
	/// </summary>
	private Single LastHorizontalMovement;

	/// <summary>
	/// Inidicates whether the script was initialized;
	/// </summary>
	private Boolean Initialized;

	private void Awake()
	{
		enabled = false;
		foreach (var Collider in GetComponentsInChildren<Collider>())
			Collider.enabled = false;
	}

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

			if (CheckActionInput())
				PerformAction();
		}
	}

	private void FixedUpdate()
	{
		if (Initialized)
		{
			FixedUpdateValues();

			HorizontalMovement = PlayerInput.GetPlayerInput(Owner).GetHorizontalAxis();

			if (HorizontalMovement != 0)
			{
				Move(HorizontalMovement);
			}
			else
			{
				if (LastHorizontalMovement != 0)
					StopMoving();
			}

			LastHorizontalMovement = HorizontalMovement;
		}
	}

	/// <summary>
	/// Checks if the input implies that an action should be performed.
	/// </summary>
	/// <returns></returns>
	protected virtual Boolean CheckActionInput()
	{
		return PlayerInput.GetPlayerInput(Owner).GetButtonState(InputButton) == PlayerInput.ButtonState.Pressed;
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

	protected virtual void OnPhotonSerializeView(PhotonStream Stream, PhotonMessageInfo Info)
	{
	}
}
