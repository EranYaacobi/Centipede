using System;
using UnityEngine;
using System.Collections;

public abstract class End : Photon.MonoBehaviour
{
	/// <summary>
	/// The body to which the end is connected.
	/// If nothing is assigned, the script assumes that the connected body is
	/// the parent.
	/// </summary>
	public Rigidbody ConnectedBody;

	/// <summary>
	/// The position of the end in the centipede.
	/// </summary>
	public EndPosition EndPosition;

	/// <summary>
	/// The anchor of the end.
	/// </summary>
	public Vector3 EndAnchor;

	/// <summary>
	/// The input button which represents the end action.
	/// </summary>
	public String InputButton;

	/// <summary>
	/// The owner of the centipede.
	/// </summary>
	public PhotonPlayer Owner;

	/// <summary>
	/// Inidicates whether the script was initialized;
	/// </summary>
	private Boolean Initialized;

	private void Awake()
	{
		enabled = false;
	}

	private void Start()
	{
		if (ConnectedBody == null)
			ConnectedBody = transform.parent.gameObject.rigidbody;
	}

	/// <summary>
	/// Custom initialization for inherited class.
	/// Derived implementations should set the mass of the end.
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
	/// Performs custom action when the end's action button is pressed.
	/// </summary>
	protected abstract void PerformAction();

	protected virtual void OnPhotonSerializeView(PhotonStream Stream, PhotonMessageInfo Info)
	{
	}
}

/// <summary>
/// The position of an end in a centipede.
/// </summary>
public enum EndPosition
{
	Back,
	Front
}