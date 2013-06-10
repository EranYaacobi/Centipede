using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LegNetworkObject : NetworkObject<LegNetworkObjectData>
{
	protected override int RequiredChildrenCount { get { return 0; } }

	protected override void CustomInitialization(LegNetworkObjectData Data)
	{
		var Link = PhotonView.Find(Data.ParentViewID).gameObject;

		gameObject.name = String.Format("{0}_{1}", Data.LegIndex + 1, gameObject.name.Replace("(Clone)", ""));
		gameObject.transform.parent = Link.transform;

		// Setting input button.
		var Leg = gameObject.GetComponent<Leg>();
		Leg.InputButton = String.Format(Keys.LegActionFormat, Data.LegIndex + 1);
		Leg.Owner = Data.Owner;
		Leg.enabled = true;

		PlayerInput.GetPlayerInput(Data.Owner).RegisterButton(Leg.InputButton);
	}
}

[Serializable]
public class LegNetworkObjectData : NetworkObjectData
{
	/// <summary>
	/// The index of the leg in the centipede.
	/// </summary>
	public readonly Int32 LegIndex;

	/// <summary>
	/// The owner of the leg.
	/// </summary>
	public readonly PhotonPlayer Owner;

	public LegNetworkObjectData()
	{
	}

	public LegNetworkObjectData(PhotonView ParentView, Int32 LegIndex, PhotonPlayer Owner)
		: base(ParentView)
	{
		this.LegIndex = LegIndex;
		this.Owner = Owner;
	}
}