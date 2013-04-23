using UnityEngine;
using System.Collections;

public class RestraintRotation : MonoBehaviour
{
	/// <summary>
	/// The transform by which this transform should be restrainted.
	/// </summary>
	public Transform RestrictedByTranform;

	void Start()
	{
		if (RestrictedByTranform == null)
			RestrictedByTranform = transform.parent;
	}

	// Update is called once per frame
	void Update()
	{
		transform.rotation = RestrictedByTranform.rotation;
	}
}
