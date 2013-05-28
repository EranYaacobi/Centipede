using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LinkNetworkObject : NetworkObject<LinkNetworkObjectData>
{
	/// <summary>
	/// The amount of legs that are missing (null prefab).
	/// </summary>
	protected Int32 MissingLegs;

	protected override int RequiredChildrenCount { get { return 2 - MissingLegs; } }

	protected override void CustomInitialization(LinkNetworkObjectData Data)
	{
		gameObject.name = String.Format("{0}_Link", Data.LinkIndex + 1);
		gameObject.transform.parent = PhotonView.Find(Data.ParentViewID).transform;

		if (PhotonNetwork.isMasterClient)
		{
			CreateLeg(Data.BackLegPrefab, Data.LinkIndex * 2, Data.Owner, gameObject, -new Vector3(gameObject.collider.bounds.size.x / 4, 0, 0));
			CreateLeg(Data.FrontLegPrefab, Data.LinkIndex * 2 + 1, Data.Owner, gameObject, new Vector3(gameObject.collider.bounds.size.x / 4, 0, 0));
		}
	}

	private void CreateLeg(String LegPrefab, Int32 Index, PhotonPlayer Owner, GameObject Link, Vector3 RelativePosition)
	{
		if (!String.IsNullOrEmpty(LegPrefab))
		{
			var Leg = PhotonNetwork.Instantiate(
				LegPrefab,
				Link.transform.position + RelativePosition,
				Link.transform.rotation, 0);
			Common.Assert(Leg != null);

			Leg.GetComponent<LegNetworkObject>().Initialize(new LegNetworkObjectData(photonView, Index, Owner));
		}
		else
		{
			MissingLegs += 1;
		}
	}
}

[Serializable]
public class LinkNetworkObjectData : NetworkObjectData
{
	/// <summary>
	/// The index of the link in the centipede.
	/// </summary>
	public readonly Int32 LinkIndex;

	/// <summary>
	/// The name of the back leg's prefab.
	/// </summary>
	public readonly String BackLegPrefab;

	/// <summary>
	/// The name of the front leg's prefab.
	/// </summary>
	public readonly String FrontLegPrefab;

	/// <summary>
	/// The owner of the link.
	/// </summary>
	public readonly PhotonPlayer Owner;

	public LinkNetworkObjectData()
	{
	}

	public LinkNetworkObjectData(PhotonView ParentView, Int32 LinkIndex, String BackLegPrefab, String FrontLegPrefab, PhotonPlayer Owner)
		: base(ParentView)
	{
		this.LinkIndex = LinkIndex;
		this.BackLegPrefab = BackLegPrefab;
		this.FrontLegPrefab = FrontLegPrefab;
		this.Owner = Owner;
	}
}