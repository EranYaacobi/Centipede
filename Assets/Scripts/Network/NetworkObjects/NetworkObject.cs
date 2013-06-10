using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A non-generic base class for an object that should be initialized in all clients.
/// It supports reporting to its parent about finishing initialization, including
/// the initialization of all children created by the NetworkObject.
/// </summary>
/// <remarks>
/// This base class should not be inherited directly (as it does absolutely nothing).
/// It exists solely for avoiding generics issues.
/// </remarks>
public abstract class NetworkObject : Photon.MonoBehaviour
{
	/// <summary>
	/// The parent of the NetworkObject.
	/// Null if there isn't any.
	/// </summary>
	protected NetworkObject Parent;

	/// <summary>
	/// A list of the initialized children.
	/// </summary>
	public List<NetworkObject> Children { get; protected set; }

	/// <summary>
	/// The number of children that the NetworkGameObject should create.
	/// </summary>
	protected abstract Int32 RequiredChildrenCount { get; }

	/// <summary>
	/// Causes a child gameobject to be disabled by its parent, until its parent is enabled.
	/// </summary>
	/// <param name="Child"></param>
	internal abstract void DisableChild(NetworkObject Child);
}

/// <summary>
/// A generic base class for an object that should be initialized in all clients.
/// It supports reporting to its parent about finishing initialization, including
/// the initialization of all children created by the NetworkObject.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NetworkObject<T> : NetworkObject where T : NetworkObjectData
{
	/// <summary>
	/// Indicates whether the NetworkObject should be set to disabled
	/// if it is not yet initialized when start is called on it.
	/// </summary>
	protected virtual Boolean DisableOnStartIfNotInitialized
	{
		get { return false; }
	}


	/// <summary>
	/// Indicates whether components where disabled due to the NetworkObject taking more
	/// than a frame to finish initializing.
	/// </summary>
	private Boolean DisabledComponents;

	/// <summary>
	/// The rigidbodies on the NetworkObject which were disabled, to avoid them
	/// being moved by the physics engine before initialization is completed.
	/// </summary>
	private List<Rigidbody> DisabledRigidBodies;

	/// <summary>
	/// The renderers on the NetworkObject which were disabled, to avoid them rendering
	/// the network object from being shown before it finishes initialization.
	/// </summary>
	private List<Renderer> DisabledRenderers;

	private void Start()
	{
		if ((Parent == null) && ((Children == null) || (Children.Count < RequiredChildrenCount)))
		{
			if (DisableOnStartIfNotInitialized)
				enabled = false;
			DisabledComponents = true;
			DisabledRigidBodies = gameObject.GetComponentsInChildren<Rigidbody>().Where(Rigidbody => !Rigidbody.isKinematic).ToList();
			DisabledRenderers = gameObject.GetComponentsInChildren<Renderer>().Where(Renderer => Renderer.enabled).ToList();
			foreach (var DisabledRigidBody in DisabledRigidBodies)
			{
				DisabledRigidBody.isKinematic = true;
				DisabledRigidBody.detectCollisions = false;
			}

			foreach (var DisabledRenderer in DisabledRenderers)
				DisabledRenderer.enabled = false;
		}
	}

	/// <summary>
	/// Causes a child gameobject to be disabled by its parent, until its parent is enabled.
	/// </summary>
	/// <param name="Child"></param>
	internal override void DisableChild(NetworkObject Child)
	{
		if (DisabledComponents)
		{
			if (!Child.rigidbody.isKinematic)
			{
				DisabledRigidBodies.Add(Child.rigidbody);
				Child.rigidbody.isKinematic = true;
				Child.rigidbody.detectCollisions = false;
			}

			if (Child.renderer.enabled)
			{
				DisabledRenderers.Add(Child.renderer);
				Child.renderer.enabled = false;
			}
		}
	}

	public void Initialize(T Data)
	{
		photonView.RPC("PerformInitialization", PhotonTargets.AllBuffered, Data);
	}

	/// <summary>
	/// Performs initialization of the NetworkObject, and report it to its parent if it has one.
	/// </summary>
	/// <param name="Data"></param>
	/// <returns></returns>
	[RPC]
	protected IEnumerator PerformInitialization(T Data)
	{
		Children = new List<NetworkObject>();

		if (Data.ParentViewID != NetworkObjectData.NullViewID)
			Parent = PhotonView.Find(Data.ParentViewID).GetComponent<NetworkObject>();

		if (Parent != null)
			Parent.DisableChild(this);

		CustomInitialization(Data);

		while (Children.Count < RequiredChildrenCount)
		{
			yield return new WaitForEndOfFrame();
		}

		PostCustomInitialization();

		if (DisabledComponents)
		{
			foreach (var DisabledRigidBody in DisabledRigidBodies)
			{
				DisabledRigidBody.isKinematic = false;
				DisabledRigidBody.detectCollisions = true;
			}

			foreach (var DisabledRenderer in DisabledRenderers)
				DisabledRenderer.enabled = true;
		}

		if (Parent != null)
			Parent.Children.Add(this);
	}

	/// <summary>
	/// Custom initialization for derived types.
	/// </summary>
	/// <param name="Data"></param>
	protected abstract void CustomInitialization(T Data);

	/// <summary>
	/// Custom initialization for derived types, after all children finished their initialization.
	/// </summary>
	protected virtual void PostCustomInitialization()
	{
	}
}

[PhotonSerializable]
[Serializable]
public abstract class NetworkObjectData
{
	public const Int32 NullViewID = -1;

	/// <summary>
	/// The link's parent.
	/// </summary>
	public readonly Int32 ParentViewID;

	public NetworkObjectData()
	{
	}

	public NetworkObjectData(PhotonView ParentView)
	{
		if (ParentView != null)
			ParentViewID = ParentView.viewID;
		else
			ParentViewID = NullViewID;
	}
}