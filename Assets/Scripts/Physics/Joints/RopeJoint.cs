using System;
using System.Linq;
using UnityEngine;
using System.Collections;

/// <summary>
/// A rope joint, which connects two objects together.
/// The rope isn't really accurate, as it may pass between obstacles if there aren't enough links.
/// </summary>
public class RopeJoint : MonoBehaviour
{
	/// <summary>
	/// The body connected to the game object via the rope joint.
	/// </summary>
	public Rigidbody ConnectedBody;

	/// <summary>
	/// The anchor of the rope joint on the game object.
	/// </summary>
	public Vector3 Anchor;

	/// <summary>
	/// The anchor of the rope joint on the other game object.
	/// </summary>
	public Vector3 RemoteAnchor;

	/// <summary>
	/// The number of links of which the rope consists.
	/// Higher number will result in a more bendy row, at the expense of CPU power.
	/// However, too high a number will results in a rope that simply doesn't hold the object
	/// properly for some reason.
	/// </summary>
	public Int32 LinksCount;

	/// <summary>
	/// The rate between the initial length of the rope (or its length when LinksCount is changed),
	/// to the desired length.
	/// </summary>
	[Range(0.01F,1)]
	public Single DesiredLengthRate;

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
	/// The springs of which the rope consists.
	/// </summary>
	public HingeJoint[] Joints;

	public void Initialize(Rigidbody ConnectedBody, Vector3 Anchor, Vector3 RemoteAnchor, Int32 LinksCount, Single DesiredLengthRate, Single SpringForce, Single DampingRate, Single RopeTotalMass)
	{

		this.ConnectedBody = ConnectedBody;
		this.Anchor = Anchor;
		this.RemoteAnchor = RemoteAnchor;
		this.LinksCount = LinksCount;
		this.SpringForce = SpringForce;
		this.DampingRate = DampingRate;
		this.RopeTotalMass = RopeTotalMass;
		this.DesiredLengthRate = DesiredLengthRate;

		InitializeJoints();
	}

	void FixedUpdate()
	{
		if ((Joints == null) || (Joints.Length != LinksCount + 1))
			InitializeJoints();

		if (Joints != null)
		{
			var SpringValues = new JointSpring {spring = SpringForce, damper = DampingRate, targetPosition = 0};
			foreach (var Joint in Joints)
			{
				Joint.useSpring = true;
				Joint.spring = SpringValues;
			}
		}
	}

	private void InitializeJoints()
	{
		if (Joints != null)
		{
			DestroyJoints();
			return;
		}

		if (LinksCount == 0)
		{
			Debug.LogError("LinksCount can't be zero!");
			return;
		}

		Joints = new HingeJoint[LinksCount + 1];
		var Position = transform.TransformPoint(Anchor);
		var OtherPosition = ConnectedBody.transform.TransformPoint(RemoteAnchor);
		var DesiredPosition = Position + (OtherPosition - Position) * (1 - DesiredLengthRate);
		var PreviousPosition = transform.position;

		// Changing the position of the GameObject, so the created links will have the desired initial length.
		transform.position += DesiredPosition - Position;
		Position = transform.TransformPoint(Anchor);
		var Links = Enumerable.Range(0, LinksCount).Select(Index =>
		{
			var GameObject = new GameObject("Link");
			GameObject.transform.position = (Position*(LinksCount - Index) + OtherPosition*(1 + Index))/(LinksCount + 1);
			var RigidBody = GameObject.AddComponent<Rigidbody>();
			RigidBody.mass = RopeTotalMass / LinksCount;
			RigidBody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
			GameObject.transform.parent = gameObject.transform;

			return GameObject;
		}).ToList();

		var PreviousLink = gameObject;
		var CurrentLink = gameObject;
		var SpringValues = new JointSpring {spring = SpringForce, damper = DampingRate, targetPosition = 0};
		for (int i = 0; i < LinksCount + 1; i++)
		{
			if (i != LinksCount)
				CurrentLink = Links[i];
			else
			{
				// Performing a switch, as the SpringJoint contains only anchor, and not remote-anchor.
				PreviousLink = ConnectedBody.gameObject;
			}

			var Joint = PreviousLink.AddComponent<HingeJoint>();
			Joint.spring = SpringValues;
			Joint.anchor = Vector3.zero;
			Joint.axis = new Vector3(0, 0, 1);
			Joint.connectedBody = CurrentLink.rigidbody;
			Joints[i] = Joint;

			PreviousLink = CurrentLink;
		}
		Joints[0].anchor = Anchor;
		Joints[LinksCount].anchor = RemoteAnchor;

		// Restoring the positions of the GameObject, and putting the links in their appropriate location.
		transform.position = PreviousPosition;
		Position = transform.TransformPoint(Anchor);
		for (var Index = 0; Index < Links.Count; Index++)
		{
			var Link = Links[Index];
			Link.transform.position = (Position * (LinksCount - Index) + OtherPosition * (1 + Index)) / (LinksCount + 1);
		}
	}

	private void DestroyJoints()
	{
		if (Joints == null)
			return;

		foreach (var Joint in Joints)
		{
			if (Joint == null)
				continue;
			if ((Joint.gameObject != gameObject) && ((ConnectedBody == null) || (Joint.gameObject != ConnectedBody.gameObject)))
			{
				Destroy(Joint.gameObject);
			}
			else if (Joint.gameObject == ConnectedBody)
			{
				Destroy(Joint.connectedBody);
				Destroy(Joint);
			}
			else
			{
				Destroy(Joint);
			}
		}
		Joints = null;
	}

	private void OnDestroy()
	{
		DestroyJoints();
	}

	private void OnDrawGizmos()
	//private void OnDrawGizmosSelected()
	{
		const Single SphereRadius = 0.05F;
		const Single Depth = -1F;

		if (Joints != null)
		{
			foreach (var Joint in Joints)
			{
				if (Joint != null)
				{
					Gizmos.color = Color.yellow;
					var JointsPosition = Joint.transform.position + new Vector3(0, 0, Depth);
					if (Joint.rigidbody == ConnectedBody)
						JointsPosition = Joint.connectedBody.transform.position + new Vector3(0, 0, Depth);
					Gizmos.DrawSphere(JointsPosition, SphereRadius);
				}
			}
		}

		Gizmos.DrawSphere(transform.TransformPoint(Anchor) + new Vector3(0, 0, Depth), SphereRadius);

		if (ConnectedBody != null)
			Gizmos.DrawSphere(ConnectedBody.transform.TransformPoint(RemoteAnchor) + new Vector3(0, 0, Depth), SphereRadius);
	}
}