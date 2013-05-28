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
}

/// <summary>
/// A generic base class for an object that should be initialized in all clients.
/// It supports reporting to its parent about finishing initialization, including
/// the initialization of all children created by the NetworkObject.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NetworkObject<T> : NetworkObject where T : NetworkObjectData
{
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


		CustomInitialization(Data);

		while (Children.Count < RequiredChildrenCount)
		{
			yield return new WaitForEndOfFrame();
		}

		PostCustomInitialization();

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