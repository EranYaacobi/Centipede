using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class RopeNetworkObject : NetworkObject<RopeNetworkObjectData>
{
	protected override int RequiredChildrenCount { get { return 0; } }

	/// <summary>
	/// The RopeEnd that fired this rope.
	/// </summary>
	private RopeEnd RopeEnd;

	/// <summary>
	/// Indicates whether the Rope is attached to something.
	/// </summary>
	public Boolean Attached { get; private set; }

	/// <summary>
	/// The initialization data.
	/// </summary>
	private RopeNetworkObjectData Data;

	/// <summary>
	/// The line-renderer that renders the rope.
	/// </summary>
	private LineRenderer LineRenderer;

	/// <summary>
	/// The gameobject that is used to show the rope properly.
	/// </summary>
	private GameObject RopeGraphics;

	/// <summary>
	/// The game-object the rope attached itself to.
	/// </summary>
	private GameObject AttachedGameObject;

	/// <summary>
	/// The fixed-joint added to the attached game object, to make it equal in the rope connection.
	/// </summary>
	private FixedJoint AttachedGameObjectFixedJoint;

	/// <summary>
	/// The anchor of the rope on the game object to which it attached.
	/// </summary>
	private Vector3 AttachedGameObjectAnchor;

	/// <summary>
	/// The rotation of the anchor of the rope on the game object to which it attached.
	/// </summary>
	private Vector3 AttachedGameObjectAnchorRotation;

	/// <summary>
	/// Indicates whether the rope was destroyed.
	/// </summary>
	private Boolean Destroyed;

	protected override void CustomInitialization(RopeNetworkObjectData Data)
	{
		this.Data = Data;
		RopeEnd = PhotonView.Find(Data.ParentViewID).GetComponent<RopeEnd>();
		RopeGraphics = transform.GetChild(0).gameObject;

		LineRenderer = GetComponent<LineRenderer>();

		gameObject.name = "Rope";
		RopeEnd.Rope = gameObject;
		rigidbody.freezeRotation = true;
		RopeGraphics.rigidbody.freezeRotation = true;
		RopeGraphics.transform.parent = null;

		gameObject.rigidbody.velocity = RopeEnd.RopeShooter.rigidbody.velocity;
		gameObject.rigidbody.AddForce(-Data.ShootingForce * gameObject.transform.right, ForceMode.Impulse);
	}

	private void Update()
	{
		if (!Attached)
		{
			if (!Destroyed)
			{
				transform.rotation = Quaternion.LookRotation(Vector3.forward, new Vector3(rigidbody.velocity.y, -rigidbody.velocity.x, 0));
				RopeGraphics.transform.position = transform.position;
				RopeGraphics.transform.rotation = transform.rotation;
			}
		}
		else
		{
			if (AttachedGameObject != null)
			{
				RopeGraphics.transform.position = AttachedGameObject.transform.TransformPoint(AttachedGameObjectAnchor);
				RopeGraphics.transform.rotation = Quaternion.Euler(AttachedGameObjectAnchorRotation + AttachedGameObject.transform.rotation.eulerAngles);
			}
			else
			{
				if (PhotonNetwork.isMasterClient)
					DetachRope();
				else
					transform.position = RopeGraphics.transform.position;
			}
		}

		if (LineRenderer != null)
		{
			LineRenderer.SetPosition(0, RopeGraphics.transform.position - Vector3.forward);
			LineRenderer.SetPosition(1, RopeEnd.RopeShooter.transform.position - Vector3.forward);
		}
	}

	private void OnCollisionEnter(Collision CollisionInfo)
	{
		if (!PhotonNetwork.isMasterClient)
			return;

		if (Attached)
			return;

		if (CollisionInfo.gameObject == RopeEnd.RopeShooter)
			return;

		// Getting the closest photon-view for the object.
		// Notice that this code assumes that there isn't a situation in which there is a parent that has
		// PhotonView, and several decendants with rigidbodies\collidres, without intervening PhotonViews.
		var ViewId = -1;
		var ColliderParent = CollisionInfo.gameObject.transform;
		PhotonView ParentPhotonView = null;
		while (ColliderParent != null)
		{
			ParentPhotonView = ColliderParent.GetComponent<PhotonView>();
			if (ParentPhotonView != null)
				break;
			ColliderParent = ColliderParent.transform.parent;
		}
		if (ParentPhotonView != null)
			ViewId = ParentPhotonView.viewID;

		photonView.RPC("SimulateAttachRope", PhotonTargets.OthersBuffered, ViewId, transform.position, transform.rotation);

		AttachedGameObject = CollisionInfo.gameObject;

		AttachedGameObjectAnchor = AttachedGameObject.transform.InverseTransformPoint(transform.position);
		AttachedGameObjectAnchorRotation = transform.rotation.eulerAngles - AttachedGameObject.transform.rotation.eulerAngles;

		if (AttachedGameObject.GetComponent<Rigidbody>() != null)
		{
			AttachedGameObjectFixedJoint = AttachedGameObject.AddComponent<FixedJoint>();
			AttachedGameObjectFixedJoint.connectedBody = rigidbody;
		}
		else
		{
			rigidbody.isKinematic = true;
		}

		CreateRope();
	}

	/// <summary>
	/// Creates the connecting rope.
	/// </summary>
	private void CreateRope()
	{
		var RopeJoint = gameObject.AddComponent<RopeJoint>();
		var Position = transform.TransformPoint(RopeJoint.Anchor);
		var ConnectedPosition = RopeEnd.transform.TransformPoint(RopeJoint.RemoteAnchor);
		RopeJoint.Initialize(RopeEnd.RopeShooter.rigidbody, Vector3.zero, Vector3.zero, (Int32)Mathf.Max(2, Vector3.Distance(Position, ConnectedPosition) / Data.LinkLength), Data.DesiredLengthRate, Data.SpringForce, Data.DampingRate, Data.RopeTotalMass);
		collider.enabled = false;
		RopeGraphics.GetComponent<NetworkRigidbody>().enabled = true;

		Attached = true;
	}

	/// <summary>
	/// Starts simulation of a rope in clients.
	/// </summary>
	/// <param name="ViewId"></param>
	/// <param name="RopePosition"></param>
	/// <param name="RopeRotation"></param>
	[RPC]
	private void SimulateAttachRope(Int32 ViewId, Vector3 RopePosition, Quaternion RopeRotation)
	{
		transform.position = RopePosition;
		transform.rotation = RopeRotation;
		RopeGraphics.transform.position = RopePosition;
		RopeGraphics.transform.rotation = RopeRotation;
		RopeGraphics.GetComponentInChildren<NetworkRigidbody>().ClearBuffers();
		
		CreateRope();

		if (ViewId != -1)
		{
			var PassedGameObject = PhotonView.Find(ViewId).gameObject;;
			AttachedGameObject = PassedGameObject.GetComponentInChildren<Rigidbody>().gameObject ??
			                     PassedGameObject.GetComponentInChildren<Collider>().gameObject;

			AttachedGameObjectAnchor = AttachedGameObject.transform.InverseTransformPoint(transform.position);
			AttachedGameObjectAnchorRotation = transform.rotation.eulerAngles - AttachedGameObject.transform.rotation.eulerAngles;
		}
		if ((AttachedGameObject != null) && (AttachedGameObject.GetComponent<Rigidbody>() != null))
		{
			AttachedGameObjectFixedJoint = AttachedGameObject.AddComponent<FixedJoint>();
			AttachedGameObjectFixedJoint.connectedBody = rigidbody;
			RopeGraphics.GetComponent<NetworkRigidbody>().enabled = false;
		}
		else
		{
			rigidbody.isKinematic = true;
		}
	}

	
	/// <summary>
	/// Detaches the rope from the RopeEnd.
	/// </summary>
	[RPC]
	public void DetachRope()
	{
		if (Destroyed)
			return;
		Destroyed = true;

		if (PhotonNetwork.isMasterClient)
			photonView.RPC("DetachRope", PhotonTargets.OthersBuffered);

		if (AttachedGameObjectFixedJoint != null)
			Destroy(AttachedGameObjectFixedJoint);

		RopeGraphics.transform.parent = gameObject.transform;
		rigidbody.isKinematic = false;
		rigidbody.freezeRotation = false;
		RopeGraphics.rigidbody.freezeRotation = false;
		RopeGraphics.rigidbody.isKinematic = false;
		RopeGraphics.GetComponent<NetworkRigidbody>().enabled = false;
		Destroy(GetComponent<LineRenderer>());
		Destroy(GetComponent<RopeJoint>());
		Destroy(constantForce);
		if (PhotonNetwork.isMasterClient)
			StartCoroutine(DestroyRope());
		RopeEnd.Rope = null;
		AttachedGameObject = null;
		Attached = false;
	}

	private IEnumerator DestroyRope()
	{
		yield return new WaitForSeconds(2);

		PhotonNetwork.Destroy(gameObject);
	}
}

[Serializable]
public class RopeNetworkObjectData : NetworkObjectData
{
	/// <summary>
	/// The force with which the rope is shot.
	/// </summary>
	public readonly Single ShootingForce;

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
	public Single LinkLength;

	/// <summary>
	/// The force of the springs of which the rope consists.
	/// </summary>
	public Single SpringForce;

	/// <summary>
	/// The damping of the springs if which the rope consists.
	/// </summary>
	public Single DampingRate;

	/// <summary>
	/// The total mass of the rope.
	/// </summary>
	public Single RopeTotalMass;

	/// <summary>
	/// The owner of the leg.
	/// </summary>
	public readonly PhotonPlayer Owner;

	public RopeNetworkObjectData()
	{
	}

	public RopeNetworkObjectData(PhotonView ParentView, Single ShootingForce, Single DesiredLengthRate, Single LinkLength, Single SpringForce, Single DampingRate, Single RopeTotalMass, PhotonPlayer Owner)
		: base(ParentView)
	{
		this.ShootingForce = ShootingForce;
		this.DesiredLengthRate = DesiredLengthRate;
		this.LinkLength = LinkLength;
		this.SpringForce = SpringForce;	
		this.DampingRate = DampingRate;
		this.RopeTotalMass = RopeTotalMass;
		this.Owner = Owner;
	}
}