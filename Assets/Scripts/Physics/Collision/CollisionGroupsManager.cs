using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CollisionGroupsManager : MonoBehaviour
{
	/// <summary>
	/// The singleton instance of the class.
	/// </summary>
	private static CollisionGroupsManager Manager;

	/// <summary>
	/// The configuration of each collision group.
	/// </summary>
	public List<CollisionGroupConfiguration> GroupsConfigurations = CreateInitialGroups();

	/// <summary>
	/// The colliders registered to the CollisionGroupsManager, for each groups.
	/// </summary>
	private readonly Dictionary<CollisionGroups, List<Collider>> RegisteredColliders = new Dictionary<CollisionGroups, List<Collider>>();

	private 

	void Start()
	{
		Manager = this;

		// This code exists in case any of the groups were deleted.
		foreach (var CollisionGroup in Enum.GetValues(typeof(CollisionGroups)).Cast<CollisionGroups>())
		{
			if (!GroupsConfigurations.Select(GroupsConfiguration => GroupsConfiguration.CollisionGroup).Contains(CollisionGroup))
				GroupsConfigurations.Add(new CollisionGroupConfiguration(CollisionGroup, new List<CollisionGroups>()));
		}
	}

	// Update is called once per frame
	void Update()
	{
		// clean up any dead objects
		foreach (var CollisionGroupColliders in RegisteredColliders.Values)
		{
			for (int i = CollisionGroupColliders.Count - 1; i >= 0; i--)
			{
				if (CollisionGroupColliders[i] == null)
					CollisionGroupColliders.RemoveAt(i);
			}
		}
	}

	public static void AddCollider(Collider Collider, CollisionGroups CollisionGroup)
	{
		if (!Manager.RegisteredColliders.Keys.Contains(CollisionGroup))
			Manager.RegisteredColliders.Add(CollisionGroup, new List<Collider>());
		Manager.RegisteredColliders[CollisionGroup].Add(Collider);

		var IgnoredGroups = Manager.GroupsConfigurations.Where(Group => Group.CollisionGroup == CollisionGroup).SelectMany(Group => Group.IgnoredGroups).Distinct();
		foreach (var IgnoredCollider in Manager.RegisteredColliders.Where(Pair => IgnoredGroups.Contains(Pair.Key)).SelectMany(Pair => Pair.Value))
		{
			if (Collider.gameObject != IgnoredCollider.gameObject)
				Physics.IgnoreCollision(Collider, IgnoredCollider, true);
		}
	}

	private static List<CollisionGroupConfiguration> CreateInitialGroups()
	{
		return Enum.GetValues(typeof(CollisionGroups)).Cast<CollisionGroups>().Select(CollisionGroup => new CollisionGroupConfiguration(CollisionGroup, new List<CollisionGroups>())).ToList();
	}
}

[Serializable]
public class CollisionGroupConfiguration
{
	/// <summary>
	/// The group to which the ignored groups will be applied.
	/// </summary>
	public CollisionGroups CollisionGroup;

	/// <summary>
	/// The groups with which collision should be ignored.
	/// </summary>
	public List<CollisionGroups> IgnoredGroups;

	public CollisionGroupConfiguration(CollisionGroups CollisionGroup, IEnumerable<CollisionGroups> IgnoredGroups)
	{
		this.CollisionGroup = CollisionGroup;
		this.IgnoredGroups = new List<CollisionGroups>(IgnoredGroups);
	}
}

public enum CollisionGroups
{
	Map,
	CentipedeBody,
	CentipedeLeg,
}