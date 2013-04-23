using UnityEngine;
using System.Collections;

public class RestraintDistance : MonoBehaviour
{
	/// <summary>
	/// The transform by which this transform should be restrainted.
	/// </summary>
	public Transform RestrictedByTranform;

	/// <summary>
	/// The minimum distance between the objects in each axis
	/// </summary>
	public Vector3 MinimumDistance;

	/// <summary>
	/// The minimum distance between the objects in each axis
	/// </summary>
	public Vector3 MaximumDistance;

	void Start()
	{
		if (RestrictedByTranform == null)
			RestrictedByTranform = transform.parent;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		var MinimumPosition = new Vector3(
			RestrictedByTranform.position.x + Mathf.Min(MinimumDistance.x, MaximumDistance.x),
			RestrictedByTranform.position.y + Mathf.Min(MinimumDistance.y, MaximumDistance.y),
			RestrictedByTranform.position.z + Mathf.Min(MinimumDistance.z, MaximumDistance.z));

		var MaximumPosition = new Vector3(
			RestrictedByTranform.position.x + Mathf.Max(MinimumDistance.x, MaximumDistance.x),
			RestrictedByTranform.position.y + Mathf.Max(MinimumDistance.y, MaximumDistance.y),
			RestrictedByTranform.position.z + Mathf.Max(MinimumDistance.z, MaximumDistance.z));

		transform.position = new Vector3(
			Mathf.Clamp(transform.position.x, MinimumPosition.x, MaximumPosition.x),
			Mathf.Clamp(transform.position.y, MinimumPosition.y, MaximumPosition.y),
			Mathf.Clamp(transform.position.z, MinimumPosition.z, MaximumPosition.z));
	}
}
