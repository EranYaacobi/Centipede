using System;
using UnityEngine;
using UnityEditor;
using System.Collections;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class CSharpPropertyAttribute : PropertyAttribute
{
	public readonly String PropertyName;

	public CSharpPropertyAttribute(String PropertyName)
	{
		this.PropertyName = PropertyName;
	}
}