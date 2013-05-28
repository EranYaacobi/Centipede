using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using System.Collections;

public class CentipedeNetworkObject : NetworkObject<CentipedeNetworkObjectData>
{
	/// <summary>
	/// The normal value of timeScale.
	/// </summary>
	private static readonly Single NormalTimeScale = Time.timeScale;

	/// <summary>
	/// The normal value of fixedDeltaTime.
	/// </summary>
	private static readonly Single NormalFixedDeltaTime = Time.fixedDeltaTime;

	protected override int RequiredChildrenCount { get { return LegsPrefabs.Length/2; } }

	/// <summary>
	/// The prefab of the centipede's links.
	/// </summary>
	public GameObject CentipedeLink;

	/// <summary>
	/// An array of the legs that should be created.
	/// </summary>
	public GameObject[] LegsPrefabs;

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
	/// The slow-motion rate upon death.
	/// </summary>
	[Range(0, 1)]
	public Single DeathSlowMotionScale;

	/// <summary>
	/// The fast-motion rate after death.
	/// </summary>
	public Single AfterDeathFastMotionScale;
	
	/// <summary>
	/// The duration of the slow-motion upon death.
	/// </summary>
	public Single DeathSlowMotionDuration;

	/// <summary>
	/// The time to wait after death before respawning.
	/// </summary>
	public Single RespawningTime;

	/// <summary>
	/// The owner of the centipede.
	/// </summary>
	private PhotonPlayer Owner;

	/// <summary>
	/// The created links.
	/// </summary>
	private List<GameObject> Links;

	/// <summary>
	/// The joints connecting the links.
	/// </summary>
	private List<Joint> LinkJoints;

	/// <summary>
	/// The colliders of the created links.
	/// </summary>
	private List<Collider> LinkColliders;

	/// <summary>
	/// The colliders of the created legs.
	/// </summary>
	private List<Collider> LegColliders;

	/// <summary>
	/// A reference for the BreakJoints script.
	/// </summary>
	private BreakJoints BreakJoints;

	private void Awake()
	{
		enabled = false;
	}

	protected override void CustomInitialization(CentipedeNetworkObjectData Data)
	{
		Common.Assert(LegsPrefabs.Length % 2 == 0);
		
		Owner = Data.Owner;
		BreakJoints = GetComponent<BreakJoints>();

		Time.timeScale = 0;

		gameObject.GetComponent<PlayerInput>().Initialize(Owner);
		if (Owner == PhotonNetwork.player)
			Camera.mainCamera.GetComponent<LookOnGameObject>().GameObject = gameObject;

		if (PhotonNetwork.isMasterClient)
			Enumerable.Range(0, LegsPrefabs.Length/2).Select<Int32, GameObject>(CreateLink).ToList();
	}

	/// <summary>
	/// Creates a centipede link.
	/// This function should be called only on the MasterClient.
	/// </summary>
	/// <param name="LinkIndex"></param>
	/// <returns></returns>
	private GameObject CreateLink(Int32 LinkIndex)
	{
		var Link = PhotonNetwork.Instantiate(
			CentipedeLink.name,
			transform.position + new Vector3((CentipedeLink.renderer.bounds.size.x + 0.01F) * LinkIndex, 0, 0),
			transform.rotation, 0);

		Common.Assert(Link != null);

		var BackLegPrefab = String.Empty;
		if (LegsPrefabs[LinkIndex * 2] != null)
			BackLegPrefab = LegsPrefabs[LinkIndex * 2].name;

		var FrontLegPrefab = String.Empty;
		if (LegsPrefabs[LinkIndex * 2 + 1] != null)
			FrontLegPrefab = LegsPrefabs[LinkIndex * 2 + 1].name;

		Link.GetComponent<LinkNetworkObject>().Initialize(new LinkNetworkObjectData(photonView, LinkIndex, BackLegPrefab, FrontLegPrefab, Owner));

		return Link;
	}

	protected override void PostCustomInitialization()
	{
		base.PostCustomInitialization();

		Links = Children.Select(Child => Child.gameObject).ToList();
		LinkJoints = new List<Joint>();

		// Connect the links, and set their mass.
		GameObject LastLink = null;
		foreach (var Link in Links)
		{
			Link.rigidbody.mass = LinkMass;
			Link.rigidbody.centerOfMass = CenterOfMass;

			if (LastLink != null)
			{
				var LastLinkHingeJoint = LastLink.AddComponent<HingeJoint>();
				var LinkHingeJoint = Link.AddComponent<HingeJoint>();

				InitializeHingeJoint(LastLinkHingeJoint, Link.rigidbody);
				InitializeHingeJoint(LinkHingeJoint, LastLink.rigidbody);
			}
			LastLink = Link;
		}

		// Get colliders.
		LegColliders = GetComponentsInChildren<Leg>().SelectMany(Leg => Leg.GetComponentsInChildren<Collider>()).ToList();
		LinkColliders = Links.Select(Link => Link.collider).ToList();

		IgnoreCollisions();

		// Enabling legs centers.
		foreach (var LegsCenter in GetComponents(typeof(LegsCenter)).Cast<MonoBehaviour>())
			LegsCenter.enabled = true;

		enabled = true;
		Time.timeScale = NormalTimeScale;
	}

	private void InitializeHingeJoint(HingeJoint HingeJoint, Rigidbody ConnectedBody)
	{
		HingeJoint.connectedBody = ConnectedBody;
		HingeJoint.axis = new Vector3(0, 0, 1);
		HingeJoint.anchor = Vector3.zero;
		HingeJoint.useLimits = true;
		HingeJoint.limits = new JointLimits { min = -LinkLimit, max = LinkLimit };

		BreakJoints.MonitorJoint(HingeJoint);

		LinkJoints.Add(HingeJoint);
	}

	private void IgnoreCollisions()
	{
		var EnabledLinkColliders = LinkColliders.Where(LinkCollider => LinkCollider.enabled).ToList();
		var EnabledLegColliders = LegColliders.Where(LegCollider => LegCollider.enabled).ToList();

		// Ignore collisions between a centipede and its legs.
		foreach (var LinkCollider in EnabledLinkColliders)
		{
			foreach (var LegCollider in EnabledLegColliders)
				Physics.IgnoreCollision(LinkCollider, LegCollider, true);
		}

		// Ignore collisions between legs.
		foreach (var FirstLegCollider in EnabledLegColliders)
		{
			foreach (var SecondLegCollider in EnabledLegColliders)
			{
				if (FirstLegCollider != SecondLegCollider)
					Physics.IgnoreCollision(FirstLegCollider, SecondLegCollider, true);
			}
		}
	}

	private void Update()
	{
		IgnoreCollisions();

		var JointsPerLink = Links.ToDictionary(Link => Link, Link => 0);
		foreach (var LinkJoint in LinkJoints)
		{
			if (LinkJoint != null)
			{
				JointsPerLink[LinkJoint.gameObject] += 1;
				JointsPerLink[LinkJoint.connectedBody.gameObject] += 1;
			}
		}

		var Broken = false;
		for (int Index = 0; Index < Links.Count - 1; Index++)
		{
			var Link = Links[Index];
			
			if (JointsPerLink[Link] == 0)
			{
				Broken = true;
				break;
			}

			var NextLink = Links[Index + 1];
			JointsPerLink[NextLink] -= JointsPerLink[Link];
		}

		if (Broken)
			StartCoroutine(Respawn());
	}

	private IEnumerator Respawn()
	{
		enabled = false;
		if (PhotonNetwork.player == Owner)
		{
			// Performing slow-motion.
			Time.timeScale = NormalTimeScale*DeathSlowMotionScale;
			Time.fixedDeltaTime = NormalFixedDeltaTime*DeathSlowMotionScale;

			yield return new WaitForSeconds(DeathSlowMotionDuration*DeathSlowMotionScale);

			// Restoring time.
			Time.fixedDeltaTime = NormalFixedDeltaTime;
		}
		else
		{
			yield return new WaitForSeconds(DeathSlowMotionDuration);
		}
		GetComponent<PlayerInput>().enabled = false;

		var AfterDeathFastMotionDuration = 0F;
		if (PhotonNetwork.player == Owner)
		{
			// Performing fast-motion.
			Time.timeScale = NormalTimeScale * AfterDeathFastMotionScale;

			var FallbackTime = DeathSlowMotionDuration*(1 - DeathSlowMotionScale);
			AfterDeathFastMotionDuration = (AfterDeathFastMotionScale - 1)/FallbackTime;
			yield return new WaitForSeconds(AfterDeathFastMotionDuration * AfterDeathFastMotionScale);

			// Restoring time.
			Time.timeScale = NormalTimeScale;
		}

		// Waiting for respawning.
		yield return new WaitForSeconds(RespawningTime - AfterDeathFastMotionDuration);

		if (PhotonNetwork.isMasterClient)
			(FindObjectOfType(typeof(SpawnPlayers)) as SpawnPlayers).Respawn(Owner, photonView.viewID);
	}

	private void OnGUI()
	{
		if (PhotonNetwork.player == Owner)
			GUI.TextField(new Rect(10, 10, 20, 20), GetComponentsInChildren<HingeJoint>().Length.ToString());
	}
}

[Serializable]
public class CentipedeNetworkObjectData : NetworkObjectData
{
	/// <summary>
	/// The owner of the leg.
	/// </summary>
	public readonly PhotonPlayer Owner;

	public CentipedeNetworkObjectData()
	{
	}

	public CentipedeNetworkObjectData(PhotonView ParentView, PhotonPlayer Owner)
		: base(ParentView)
	{
		this.Owner = Owner;
	}
}