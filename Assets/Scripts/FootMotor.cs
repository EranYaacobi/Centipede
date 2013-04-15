using System;
using UnityEngine;
using System.Collections;

public abstract class FootMotor : MonoBehaviour
{
	/// <summary>
	/// The body to which the foot is connected.
	/// If nothing is assigned, the script assumes that the connected body is
	/// the parent.
	/// </summary>
	public Rigidbody ConnectedBody;

	/// <summary>
	/// The anchor of the foot.
	/// </summary>
	public Vector3 FootAnchor;

	/// <summary>
	/// The input button which represents the leg action.
	/// </summary>
	public String InputButton;

	private void Start()
	{
		if (ConnectedBody == null)
			ConnectedBody = transform.parent.gameObject.rigidbody;
		Initialize();
	}

	/// <summary>
	/// Custom initialization for inherited class.
	/// Called at start.
	/// </summary>
	protected abstract void Initialize();

	private void Update()
	{
		UpdateValues();
		
		if (Input.GetButtonUp(InputButton))
			PerformAction();

		var Horizontal = Input.GetAxis(Keys.Horizontal);
		if (Horizontal != 0)
			Move(Horizontal);
	}

	/// <summary>
	/// Performs custom value updates, in order to allow changing values in debug mode using the IDE.
	/// </summary>
	protected virtual void UpdateValues()
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
}