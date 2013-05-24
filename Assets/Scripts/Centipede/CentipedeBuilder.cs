using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using System.Collections;

public class CentipedeBuilder : Photon.MonoBehaviour
{
	/// <summary>
	/// The links of the centipede.
	/// </summary>
	public GameObject CentipedeLink;

	/// <summary>
	/// The mass of a link.
	/// </summary>
	public Single LinkMass;

	/// <summary>
	/// The limit of a link in angles (in each direction).
	/// </summary>
	public Single LinkLimit;

	/// <summary>
	/// The center of mass of a link, relative to its center.
	/// </summary>
	public Vector3 CenterOfMass;

	/// <summary>
	/// An array of the legs that should be created.
	/// </summary>
	public GameObject[] Legs;

	/// <summary>
	/// The owner of the centipede.
	/// </summary>
	public PhotonPlayer Owner;

	private void Start()
	{
		if (!PhotonNetwork.isMasterClient)
			return;

		Common.Assert(Legs.Length%2 == 0);

		CreateCentipede();
	}

	private void CreateCentipede()
	{
		// Create the links, and their legs.
		var Links = Enumerable.Range(0, Legs.Length / 2).Select(Index =>
			{
				var BackLegPrefab = Legs[Index * 2];
				var FrontLegPrefab = Legs[Index * 2 + 1];
				var LegLink = CreateLink(Index);

				CreateLeg(BackLegPrefab, Index * 2, LegLink, -new Vector3(LegLink.collider.bounds.size.x / 4, 0, 0));
				CreateLeg(FrontLegPrefab, Index * 2 + 1, LegLink, new Vector3(LegLink.collider.bounds.size.x / 4, 0, 0));

				return LegLink;
			}).ToList();

		var LinksNetworkViewIDs = Links.Select(Link => Link.GetPhotonView().viewID).ToArray();

		photonView.RPC("InitializeCentipede", PhotonTargets.AllBuffered, LinksNetworkViewIDs);
	}

	[RPC]
	private void InitializeCentipede(Int32[] LinksNetworkViewIDs)
	{
		var Links = LinksNetworkViewIDs.Select(LinkNetworkViewID => PhotonView.Find(LinkNetworkViewID).gameObject).ToList();

		// Connect the links.
		GameObject LastLink = null;
		foreach (var Link in Links)
		{
			if (LastLink != null)
			{
				var LastLinkHingeJoint = LastLink.AddComponent<HingeJoint>();
				var LinkHingeJoint = Link.AddComponent<HingeJoint>();

				InitializeHingeJoint(LastLinkHingeJoint, Link.rigidbody);
				InitializeHingeJoint(LinkHingeJoint, LastLink.rigidbody);
			}
			LastLink = Link;
		}

		// Ignore collisions between a centipede and its legs.
		var LegColliders = GetComponentsInChildren<LegMotor>().SelectMany(Leg => Leg.GetComponentsInChildren<Collider>()).ToList();
		foreach (var Link in Links)
		{
			foreach (var LegCollider in LegColliders)
				Physics.IgnoreCollision(Link.collider, LegCollider, true);
		}

		// Ignore collisions between legs.
		foreach (var FirstLegCollider in LegColliders)
		{
			foreach (var SecondLegCollider in LegColliders)
			{
				if (FirstLegCollider != SecondLegCollider)
					Physics.IgnoreCollision(FirstLegCollider, SecondLegCollider, true);
			}
		}

		// Enabling legs centers.
		foreach (var LegsCenter in GetComponents(typeof (LegsCenter)).Cast<MonoBehaviour>())
			LegsCenter.enabled = true;
	}

	private void InitializeHingeJoint(HingeJoint HingeJoint, Rigidbody ConnectedBody)
	{
		HingeJoint.connectedBody = ConnectedBody;
		HingeJoint.axis = new Vector3(0, 0, 1);
		HingeJoint.anchor = Vector3.zero;
		HingeJoint.useLimits = true;
		HingeJoint.limits = new JointLimits { min = -LinkLimit, max = LinkLimit };
	}

	private GameObject CreateLink(Int32 Index)
	{
		var Link = PhotonNetwork.Instantiate(
			CentipedeLink.name, 
			transform.position + new Vector3(CentipedeLink.renderer.bounds.size.x * Index, 0, 0),
			transform.rotation, 0);

		Common.Assert(Link != null);

		photonView.RPC("InitializeLink", PhotonTargets.AllBuffered, Link.GetPhotonView().viewID, Index);
		return Link;
	}

	[RPC]
	private void InitializeLink(Int32 NetworkViewID, Int32 Index)
	{
		var Link = PhotonView.Find(NetworkViewID).gameObject;
		Link.name = String.Format("{0}_Link", Index + 1);
		Link.transform.parent = transform;
		Link.rigidbody.mass = LinkMass;
		Link.rigidbody.centerOfMass = CenterOfMass;
	}

	private void CreateLeg(GameObject LegPrefab, Int32 Index, GameObject LegLink, Vector3 RelativePosition)
	{
		if (LegPrefab != null)
		{
			var Leg = PhotonNetwork.Instantiate(
				LegPrefab.name,
				LegLink.transform.position + LegPrefab.transform.position + RelativePosition,
				LegLink.transform.rotation, 0);
			Common.Assert(Leg != null);

			photonView.RPC("InitializeLeg", PhotonTargets.AllBuffered, LegLink.GetPhotonView().viewID, Leg.GetPhotonView().viewID, Index);
		}
	}

	[RPC]
	private void InitializeLeg(Int32 LinkNetworkViewID, Int32 LegNetworkViewID, Int32 Index)
	{
		var LegLink = PhotonView.Find(LinkNetworkViewID).gameObject;
		var Leg = PhotonView.Find(LegNetworkViewID).gameObject;

		Leg.name = String.Format("{0}_{1}", Index + 1, Leg.name.Replace("(Clone)", ""));
		Leg.transform.parent = LegLink.transform;

		// Setting input button.
		var LegMotor = Leg.GetComponent<LegMotor>();
		LegMotor.InputButton = String.Format(Keys.LegActionFormat, Index + 1);
		LegMotor.Owner = Owner;
		LegMotor.enabled = true;

		PlayerInput.GetPlayerInput(Owner).RegisterButton(LegMotor.InputButton);
	}
}