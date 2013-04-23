using System;
using UnityEngine;
using System.Collections;

public class CreateCentipede : MonoBehaviour
{
	/// <summary>
	/// The links of the centipede.
	/// </summary>
	public GameObject CentipedeLink;

	/// <summary>
	/// The number of links between leg-links.
	/// </summary>
	public Int32 LinksBetweenLegs;

	/// <summary>
	/// An array of the legs that should be created.
	/// </summary>
	public GameObject[] Legs;

	private Int32 LinksCount = 0;

	void Awake()
	{
		var CentipedeLinkPosition = new Vector3(0, 0, 0);
		GameObject LastCentipedeLink = null;
		for (int i = 0; i < Legs.Length; i++)
		{
			var LegPrefab = Legs[i];
			var LegLink = CreateCentipedeLink(ref CentipedeLinkPosition, LastCentipedeLink);

			var Leg = Instantiate(LegPrefab, 
								  LegLink.transform.position + LegPrefab.transform.position, 
								  LegLink.transform.rotation) as GameObject;
			Common.Assert(Leg != null);
			Leg.name = String.Format("{0}_{1}", LegPrefab.name, i + 1);
			Leg.transform.parent = LegLink.transform;
			LastCentipedeLink = LegLink;

			if (i == Legs.Length - 1)
			{
				Destroy(LegLink.hingeJoint);
				break;
			}

			for (int j = 0; j < LinksBetweenLegs; j++)
				LastCentipedeLink = CreateCentipedeLink(ref CentipedeLinkPosition, LastCentipedeLink);
		}

		Common.Assert(LastCentipedeLink != null);
	}

	private GameObject CreateCentipedeLink(ref Vector3 Position, GameObject LastCentipedeLink)
	{
		var CurrentCentipedeLink = Instantiate(CentipedeLink, 
											   transform.position + Position,
											   transform.rotation) as GameObject;
		Common.Assert(CurrentCentipedeLink != null);
		CurrentCentipedeLink.name = String.Format("CentipedeLink_{0}", LinksCount + 1);
		LinksCount += 1;
		CurrentCentipedeLink.transform.parent = transform;
		if (LastCentipedeLink != null)
			LastCentipedeLink.hingeJoint.connectedBody = CurrentCentipedeLink.rigidbody;

		Position = new Vector3(Position.x + CurrentCentipedeLink.renderer.bounds.size.x, Position.y, Position.z);
		return CurrentCentipedeLink;
	}
}
