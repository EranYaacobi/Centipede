// DeleteComponent.cs
using UnityEngine;
using System.Collections;

namespace GUIDesigner.Scripts.Utilities
{
	/// <summary>
	/// A component that can be used to enable another component to delete itself in the editor
	/// upon creation immediately.
	/// </summary>
	[ExecuteInEditMode]
	public class DeleteComponent : MonoBehaviour
	{
		public Component ComponentReference;

		private void Start()
		{
			if (ComponentReference != null)
				DestroyImmediate(ComponentReference);
			DestroyImmediate(this);
		}
	}
}