using System;
using System.Linq;
using UnityEngine;
using System.Collections;

/// <summary>
/// An end that can be used to ram enemies. It is handled by the RamEndsCenter.
/// The public members of this class should not be set directly, but via the
/// RamEndsCenter to make all ends synchronized.
/// </summary>
public class RamEnd : End
{
	/// <summary>
	/// The end's ram.
	/// </summary>
	private GameObject Ram;

	public override void Initialize(Single Mass)
	{
		base.Initialize(Mass);
		Ram = transform.GetChild(0).gameObject;
		Ram.rigidbody.mass = Mass;
		Ram.GetComponent<FixedJoint>().connectedBody = ConnectedBody;

		UpdateValues();
	}

	protected override bool CheckActionInput()
	{
		return false;
	}

	protected override void PerformAction()
	{
	}
}
