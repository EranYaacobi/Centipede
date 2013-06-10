using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LinkNetworkObject : NetworkObject<LinkNetworkObjectData>
{
	/// <summary>
	/// The amount of parts that are missing (null prefab).
	/// </summary>
	private Int32 MissingParts;

	protected override int RequiredChildrenCount { get { return 3 - MissingParts; } }

	protected override void CustomInitialization(LinkNetworkObjectData Data)
	{
		gameObject.name = String.Format("{0}_Link", Data.LinkIndex + 1);
		gameObject.transform.parent = PhotonView.Find(Data.ParentViewID).transform;

		if (PhotonNetwork.isMasterClient)
		{
			CreateLeg(Data.BackLegPrefab, Data.LinkIndex * 2, Data.Owner, -new Vector3(gameObject.collider.bounds.size.x / 4, 0, 0));
			CreateLeg(Data.FrontLegPrefab, Data.LinkIndex * 2 + 1, Data.Owner, new Vector3(gameObject.collider.bounds.size.x / 4, 0, 0));
			CreateEnd(Data.EndPrefab, Data.EndPosition, Data.Owner);
		}
	}

	private void CreateLeg(String LegPrefab, Int32 Index, PhotonPlayer Owner, Vector3 RelativePosition)
	{
		if (!String.IsNullOrEmpty(LegPrefab))
		{
			var Leg = PhotonNetwork.InstantiateSceneObject(
				LegPrefab,
				gameObject.transform.position + RelativePosition,
				gameObject.transform.rotation, 0, null);
			Common.Assert(Leg != null);

			Leg.GetComponent<LegNetworkObject>().Initialize(new LegNetworkObjectData(photonView, Index, Owner));
		}
		else
		{
			photonView.RPC("AddMissingPart", PhotonTargets.AllBuffered);
		}
	}

	private void CreateEnd(String EndPrefab, EndPosition EndPosition, PhotonPlayer Owner)
	{
		if (!String.IsNullOrEmpty(EndPrefab))
		{
			var End = PhotonNetwork.InstantiateSceneObject(
				EndPrefab,
				gameObject.transform.position,
				gameObject.transform.rotation, 0, null);
			Common.Assert(End != null);

			End.GetComponent<EndNetworkObject>().Initialize(new EndNetworkObjectData(photonView, EndPosition, Owner));
		}
		else
		{
			photonView.RPC("AddMissingPart", PhotonTargets.AllBuffered);
		}
	}

	[RPC]
	private void AddMissingPart()
	{
		MissingParts += 1;
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
	/// Indicates whether the end connected to the link (if there is such), is in the front or back of the centipede.
	/// </summary>
	public readonly EndPosition EndPosition;

	/// <summary>
	/// The name of the back leg's prefab.
	/// </summary>
	public readonly String BackLegPrefab;

	/// <summary>
	/// The name of the front leg's prefab.
	/// </summary>
	public readonly String FrontLegPrefab;
	
	/// <summary>
	/// The name of the end's prefab.
	/// </summary>
	public readonly String EndPrefab;

	/// <summary>
	/// The owner of the link.
	/// </summary>
	public readonly PhotonPlayer Owner;

	public LinkNetworkObjectData()
	{
	}

	public LinkNetworkObjectData(PhotonView ParentView, Int32 LinkIndex, EndPosition EndPosition, String BackLegPrefab, String FrontLegPrefab, String EndPrefab, PhotonPlayer Owner)
		: base(ParentView)
	{
		this.LinkIndex = LinkIndex;
		this.EndPosition = EndPosition;
		this.BackLegPrefab = BackLegPrefab;
		this.FrontLegPrefab = FrontLegPrefab;
		this.EndPrefab = EndPrefab;
		this.Owner = Owner;
	}
}