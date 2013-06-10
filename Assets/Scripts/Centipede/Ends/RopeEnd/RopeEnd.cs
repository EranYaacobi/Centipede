using System;
using System.Linq;
using UnityEngine;
using System.Collections;

/// <summary>
/// An end that can be used to fire a rope in order to swing. It is handled by the RopeEndsCenter.
/// The public members of this class should not be set directly, but via the
/// RopeEndsCenter to make all ends synchronized.
/// </summary>
public class RopeEnd : End
{
	/// <summary>
	/// The game object that indicates the shooting position.
	/// </summary>
	public GameObject ShootingPosition;

	/// <summary>
	/// The rope's prefab.
	/// </summary>
	public GameObject RopePrefab;

	/// <summary>
	/// The force applied on the rope when shooting it.
	/// </summary>
	public Single ShootingForce;

	/// <summary>
	/// The rate between the initial length of the rope (or its length when LinksCount is changed),
	/// to the desired length.
	/// </summary>
	[Range(0.01F, 1)]
	public Single DesiredLengthRate;

	/// <summary>
	/// The length of each rope links.
	/// This is used to break the rope into links based on its length.
	/// </summary>
	public Single RopeLinkLength;

	/// <summary>
	/// The total mass of the rope (excluding its head).
	/// </summary>
	public Single RopeTotalMass;

	/// <summary>
	/// The force of the springs of which the rope consists.
	/// </summary>
	public Single RopeSpringForce;

	/// <summary>
	/// The damping of the springs if which the rope consists.
	/// </summary>
	public Single RopeDampingRate;

	/// <summary>
	/// The end's rope shooter.
	/// </summary>
	public GameObject RopeShooter { get; private set; }

	/// <summary>
	/// The end's rope.
	/// </summary>
	[HideInInspector]
	public GameObject Rope;

	/// <summary>
	/// The rope's script.
	/// </summary>
	private RopeNetworkObject RopeScript;

	public override void Initialize(Single Mass)
	{
		base.Initialize(Mass);
		RopeShooter = transform.GetChild(0).gameObject;
		RopeShooter.rigidbody.mass = Mass;
		RopeShooter.GetComponent<FixedJoint>().connectedBody = ConnectedBody;

		UpdateValues();
	}

	protected override void UpdateValues()
	{
		base.UpdateValues();
	}

	protected override void PerformAction()
	{
		if (Rope == null)
		{
			if (PhotonNetwork.isMasterClient)
			{
				Rope = PhotonNetwork.InstantiateSceneObject(
					RopePrefab.name,
					ShootingPosition.transform.position,
					gameObject.transform.rotation, 0, null);

				RopeScript = Rope.GetComponent<RopeNetworkObject>();
				RopeScript.Initialize(new RopeNetworkObjectData(photonView, ShootingForce, DesiredLengthRate, RopeLinkLength, RopeSpringForce, RopeDampingRate, RopeTotalMass, Owner));
			}
		}
		else
		{
			if (RopeScript == null)
				RopeScript = Rope.GetComponent<RopeNetworkObject>();
			if (RopeScript.Attached)
				RopeScript.DetachRope();
		}
	}

	private void OnDestroy()
	{
		if (Rope != null)
		{
			RopeScript.DetachRope();
		}
	}
}
