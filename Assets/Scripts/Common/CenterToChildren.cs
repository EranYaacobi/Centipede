using UnityEngine;
using System.Collections;

public class CenterToChildren : MonoBehaviour
{
	// Update is called once per frame
	void Update()
	{
		var TotalTransform = Vector3.zero;
		var ChildrenCount = 0;
		var ChildrenTransforms = GetComponentsInChildren<Transform>();

		foreach (var ChildTransform in ChildrenTransforms)
		{
			TotalTransform += ChildTransform.position;
			ChildrenCount += 1;

			if (ChildTransform.parent == transform)
				ChildTransform.parent = null;
		}

		if (ChildrenCount != 0)
			transform.position = TotalTransform / ChildrenCount;

		foreach (var ChildTransform in ChildrenTransforms)
		{
			if (ChildTransform.parent == null)
				ChildTransform.parent = transform;
		}
	}
}
