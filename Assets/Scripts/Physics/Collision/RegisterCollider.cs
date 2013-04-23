using UnityEngine;
using System.Collections;

public class RegisterCollider : MonoBehaviour
{
	/// <summary>
	/// The collision group of the collider.
	/// </summary>
	public CollisionGroups CollisionGroup;

	// Use this for initialization
	void Start() 
	{
		CollisionGroupsManager.AddCollider(collider, CollisionGroup);
	}
}
