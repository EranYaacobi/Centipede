using System;
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

	/// <summary>
	/// Indicates whether restraining should be reversed
	/// </summary>
	public Boolean ReverseRestraint;

	void Start()
	{
		if (RestrictedByTranform == null)
			RestrictedByTranform = transform.parent;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		var Model = RestrictedByTranform;
		var Affected = transform;
		if (ReverseRestraint)
		{
			var Temp = Model;
			Model = Affected;
			Affected = Temp;
		}
		
		var TranformedMinimumDistance = Model.TransformPoint(MinimumDistance);
		var TranformedMaximumPosition = Model.TransformPoint(MaximumDistance);

		var TransformedFixedMinimumPosition =
			new Vector3(Mathf.Min(TranformedMinimumDistance.x, TranformedMaximumPosition.x),
						Mathf.Min(TranformedMinimumDistance.y, TranformedMaximumPosition.y),
						Mathf.Min(TranformedMinimumDistance.z, TranformedMaximumPosition.z));

		var TransformedFixedMaximumPosition =
			new Vector3(Mathf.Max(TranformedMinimumDistance.x, TranformedMaximumPosition.x),
						Mathf.Max(TranformedMinimumDistance.y, TranformedMaximumPosition.y),
						Mathf.Max(TranformedMinimumDistance.z, TranformedMaximumPosition.z));

		Affected.position = new Vector3(
			Mathf.Clamp(transform.position.x, TransformedFixedMinimumPosition.x, TransformedFixedMaximumPosition.x),
			Mathf.Clamp(transform.position.y, TransformedFixedMinimumPosition.y, TransformedFixedMaximumPosition.y),
			Mathf.Clamp(transform.position.z, TransformedFixedMinimumPosition.z, TransformedFixedMaximumPosition.z));
	}

	void OnDrawGizmos()
	{
		const Single Depth = -5F;
		var Model = RestrictedByTranform;
		if (ReverseRestraint)
			Model = transform;;

		var TranformedMinimumDistance = Model.TransformPoint(MinimumDistance);
		var TranformedMaximumPosition = Model.TransformPoint(MaximumDistance);

		var TransformedFixedMinimumPosition =
			new Vector3(Mathf.Min(TranformedMinimumDistance.x, TranformedMaximumPosition.x),
						Mathf.Min(TranformedMinimumDistance.y, TranformedMaximumPosition.y),
						Mathf.Min(TranformedMinimumDistance.z, TranformedMaximumPosition.z));

		var TransformedFixedMaximumPosition =
			new Vector3(Mathf.Max(TranformedMinimumDistance.x, TranformedMaximumPosition.x),
						Mathf.Max(TranformedMinimumDistance.y, TranformedMaximumPosition.y),
						Mathf.Max(TranformedMinimumDistance.z, TranformedMaximumPosition.z));
		Gizmos.color = Color.magenta;

		var TopLeft = new Vector3(TransformedFixedMaximumPosition.x, TransformedFixedMinimumPosition.y, Depth);
		var TopRight = new Vector3(TransformedFixedMaximumPosition.x, TransformedFixedMaximumPosition.y, Depth);
		var BottomLeft = new Vector3(TransformedFixedMinimumPosition.x, TransformedFixedMinimumPosition.y, Depth);
		var BottomRight = new Vector3(TransformedFixedMinimumPosition.x, TransformedFixedMaximumPosition.y, Depth);
		Gizmos.DrawLine(TopLeft, TopRight);
		Gizmos.DrawLine(TopRight, BottomRight);
		Gizmos.DrawLine(BottomRight, BottomLeft);
		Gizmos.DrawLine(BottomLeft, TopLeft);
	}
}
