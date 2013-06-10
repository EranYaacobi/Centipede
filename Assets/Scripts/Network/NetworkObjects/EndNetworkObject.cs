using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EndNetworkObject : NetworkObject<EndNetworkObjectData>
{
	protected override int RequiredChildrenCount { get { return 0; } }

	protected override void CustomInitialization(EndNetworkObjectData Data)
	{
		var Link = PhotonView.Find(Data.ParentViewID).gameObject;

		gameObject.name = String.Format("{0}{1}", Data.EndPosition, gameObject.name.Replace("(Clone)", ""));
		gameObject.transform.parent = Link.transform;

		// Setting input button.
		var End = gameObject.GetComponent<End>();
		End.InputButton = String.Format(Keys.EndActionFormat, Data.EndPosition);
		End.Owner = Data.Owner;
		End.enabled = true;
		End.EndPosition = Data.EndPosition;

		PlayerInput.GetPlayerInput(Data.Owner).RegisterButton(End.InputButton);
	}
}

[Serializable]
public class EndNetworkObjectData : NetworkObjectData
{
	/// <summary>
	/// Indicates whether this is a front or back end.
	/// </summary>
	public readonly EndPosition EndPosition;

	/// <summary>
	/// The owner of the end.
	/// </summary>
	public readonly PhotonPlayer Owner;

	public EndNetworkObjectData()
	{
	}

	public EndNetworkObjectData(PhotonView ParentView, EndPosition EndPosition, PhotonPlayer Owner)
		: base(ParentView)
	{
		this.EndPosition = EndPosition;
		this.Owner = Owner;
	}
}