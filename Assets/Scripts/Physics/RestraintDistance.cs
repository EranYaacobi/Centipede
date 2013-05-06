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

	/// <summary>
	/// The force applied to translate the transform to the restrainted by transform area.
	/// </summary>
	public Single ForceConstant;

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

		var RelativeAffectedPosition = Model.InverseTransformPoint(Affected.transform.position);

		RelativeAffectedPosition = new Vector3(
			Mathf.Clamp(RelativeAffectedPosition.x, MinimumDistance.x, MaximumDistance.x),
			Mathf.Clamp(RelativeAffectedPosition.y, MinimumDistance.y, MaximumDistance.y),
			Mathf.Clamp(RelativeAffectedPosition.z, MinimumDistance.z, MaximumDistance.z));

		Affected.rigidbody.AddForce(ForceConstant * (Model.TransformPoint(RelativeAffectedPosition) - Affected.position), ForceMode.Force);
	}

	/// <summary>
	/// Returns whether the GameObject is inside the restricted area.
	/// </summary>
	/// <returns></returns>
	public Boolean InArea()
	{
		var Model = RestrictedByTranform;
		var Affected = transform;
		if (ReverseRestraint)
		{
			var Temp = Model;
			Model = Affected;
			Affected = Temp;
		}

		var RelativeAffectedPosition = Model.InverseTransformPoint(Affected.transform.position);

		var ClampedRelativeAffectedPosition = new Vector3(
			Mathf.Clamp(RelativeAffectedPosition.x, MinimumDistance.x, MaximumDistance.x),
			Mathf.Clamp(RelativeAffectedPosition.y, MinimumDistance.y, MaximumDistance.y),
			Mathf.Clamp(RelativeAffectedPosition.z, MinimumDistance.z, MaximumDistance.z));

		return (ClampedRelativeAffectedPosition - RelativeAffectedPosition).magnitude < Single.Epsilon;
	}

	void OnDrawGizmosSelected()
	{
		const Single Depth = -5F;
		var Model = RestrictedByTranform;
		if (ReverseRestraint)
			Model = transform;;

		var TopLeft = new Vector3(MaximumDistance.x, MinimumDistance.y, Depth);
		var TopRight = new Vector3(MaximumDistance.x, MaximumDistance.y, Depth);
		var BottomLeft = new Vector3(MinimumDistance.x, MinimumDistance.y, Depth);
		var BottomRight = new Vector3(MinimumDistance.x, MaximumDistance.y, Depth);

		var TranformedTopLeft = Model.TransformPoint(TopLeft);
		var TranformedTopRight = Model.TransformPoint(TopRight);
		var TranformedBottomLeft = Model.TransformPoint(BottomLeft);
		var TranformedBottomRight = Model.TransformPoint(BottomRight);

		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(TranformedTopLeft, TranformedTopRight);
		Gizmos.DrawLine(TranformedTopRight, TranformedBottomRight);
		Gizmos.DrawLine(TranformedBottomRight, TranformedBottomLeft);
		Gizmos.DrawLine(TranformedBottomLeft, TranformedTopLeft);
	}
}
