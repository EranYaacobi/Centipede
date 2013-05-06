using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using System.Collections;

public class CreateCentipede : MonoBehaviour
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

	void Awake()
	{
		Common.Assert(Legs.Length % 2 == 0);

		// Create the links, and their legs.
		var Links = Enumerable.Range(0, Legs.Length / 2).Select(Index =>
			{
				var BackLegPrefab = Legs[Index * 2];
				var FrontLegPrefab = Legs[Index * 2 + 1];
				var LegLink = CreateCentipedeLink(Index);

				LegLink.rigidbody.mass = LinkMass;
				LegLink.rigidbody.centerOfMass = CenterOfMass;

				CreateLeg(BackLegPrefab, Index * 2, LegLink, -new Vector3(LegLink.collider.bounds.size.x / 4, 0, 0)); 
				CreateLeg(FrontLegPrefab, Index * 2 + 1, LegLink, new Vector3(LegLink.collider.bounds.size.x / 4, 0, 0));

				return LegLink;
			}).ToList();

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

		var LegColliders = GetComponentsInChildren<LegMotor>().SelectMany(Leg => Leg.GetComponentsInChildren<Collider>()).ToList();
		// Ignore collisions between a centipede and its legs.
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
	}

	private void InitializeHingeJoint(HingeJoint HingeJoint, Rigidbody ConnectedBody)
	{
		HingeJoint.connectedBody = ConnectedBody;
		HingeJoint.axis = new Vector3(0, 0, 1);
		HingeJoint.anchor = Vector3.zero;
		HingeJoint.useLimits = true;
		HingeJoint.limits = new JointLimits { min = -LinkLimit, max = LinkLimit };
	}

	private GameObject CreateCentipedeLink(Int32 Index)
	{
		var Link = Instantiate(CentipedeLink,
							   transform.position + new Vector3(CentipedeLink.renderer.bounds.size.x * Index, 0, 0),
							   transform.rotation) as GameObject;
		Common.Assert(Link != null);
		Link.name = String.Format("{0}_Link", Index + 1);
		Link.transform.parent = transform;
		return Link;
	}

	private void CreateLeg(GameObject LegPrefab, Int32 Index, GameObject LegLink, Vector3 RelativePosition)
	{
		if (LegPrefab != null)
		{
			var Leg = Instantiate(LegPrefab,
			                      LegLink.transform.position + LegPrefab.transform.position + RelativePosition,
			                      LegLink.transform.rotation) as GameObject;
			Common.Assert(Leg != null);
			Leg.name = String.Format("{0}_{1}", Index + 1, LegPrefab.name);
			Leg.transform.parent = LegLink.transform;
		}
	}
}
