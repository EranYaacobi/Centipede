using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections;

public class DebugJoint : MonoBehaviour
{
	public Int32 MaxDepth = 2;

	// Use this for initialization
	private void Start()
	{
	}

	private String PropertiesToString(object Object, Int32 Depth = 0)
	{
		var Result = String.Format("{0}:\n", Object.GetType());
		var ObjectProperties = Object.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
		foreach (var ObjectProperty in ObjectProperties)
		{
			try
			{
				var PropertyValue = ObjectProperty.GetValue(Object, null);

				if ((PropertyValue is SoftJointLimit) ||
				    (PropertyValue is JointDrive) ||
				    (PropertyValue is Rigidbody) ||
				    (PropertyValue is Transform) ||
				    (PropertyValue is SphereCollider))
				{
					if (Depth == 2)
						continue;
					Result += PropertiesToString(PropertyValue, Depth + 1);
				}
				else
				{
					var TabPrefix = "";
					for (var i = 0; i < Depth; i++)
						TabPrefix += "\t";

					Result += String.Format("{0}Property: {1}:\tValue: {2}\n", TabPrefix, ObjectProperty.Name, PropertyValue);
				}
			}
			catch
			{
			}
		}

		return Result;
	}

	// Update is called once per frame
	private void Update()
	{
		if (Input.GetKeyUp("space"))
		{
			var Joint = GetComponent<ConfigurableJoint>();
			Debug.Log(PropertiesToString(Joint));
		}
	}
}