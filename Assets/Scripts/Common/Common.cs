using System;
using UnityEngine;
using System.Collections;

public class Common : MonoBehaviour
{
	/// <summary>
	/// Causes a debug-break if the given condition is false, and a debugger is present.
	/// </summary>
	/// <param name="Condition"></param>
	public static void Assert(Boolean Condition)
	{
		System.Diagnostics.Debug.Assert(Condition);
	}
}